using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace Fixie.AutoRun
{
   public class Solution : IDisposable
   {
      private readonly string _solutionPath;
      private readonly Func<string, string> _fileReader;
      private readonly IFileSystemWatcher _fileSystemWatcher;
      private readonly IDictionary<string, ProjectFile> _projects;

      private Solution(string solutionPath, Func<string, string> fileReader, IFileSystemWatcher fileSystemWatcher, IScheduler scheduler)
      {
         _solutionPath = solutionPath;
         _fileReader = fileReader;
         _fileSystemWatcher = fileSystemWatcher;
         _projects = new Dictionary<string, ProjectFile>(StringComparer.CurrentCultureIgnoreCase);

         ReloadSolution();
         SetupFileSystemWatcherEvents(scheduler);

         _fileSystemWatcher.Directory = Path.GetDirectoryName(_solutionPath);
      }

      private void SetupFileSystemWatcherEvents(IScheduler scheduler)
      {
         var changed = Observable
            .FromEventPattern<ChangedEventArgs>(h => _fileSystemWatcher.Changed += h, h => _fileSystemWatcher.Changed -= h)
            .Select(x => new Action<SolutionChangedEventArgs>(y => FileSystemItemChanged(x.EventArgs, y)));
         var created = Observable
            .FromEventPattern<CreatedEventArgs>(h => _fileSystemWatcher.Created += h, h => _fileSystemWatcher.Created -= h)
            .Select(x => new Action<SolutionChangedEventArgs>(y => FileSystemItemCreated(x.EventArgs, y)));
         var renamed = Observable
            .FromEventPattern<RenamedEventArgs>(h => _fileSystemWatcher.Renamed += h, h => _fileSystemWatcher.Renamed -= h)
            .Select(x => new Action<SolutionChangedEventArgs>(y => FileSystemItemRenamed(x.EventArgs, y)));
         Observable.Merge(changed, created, renamed)
                   .Buffer(1.Seconds(), scheduler)
                   .Subscribe(source =>
                              {
                                 if (!source.Any()) return;
                                 var args = new SolutionChangedEventArgs();
                                 source.Each(x => x(args));
                                 if (args.IsEmpty) return;
                                 Changed(this, args);
                              });
      }

      public static Solution Load(string path)
      {
         return Load(path, File.ReadAllText, new FileSystemWatcherWrapper(), ThreadPoolScheduler.Instance);
      }

      public static Solution Load(string path, Func<string, string> fileReader, IFileSystemWatcher fileSystemWatcher, IScheduler scheduler)
      {
         return new Solution(path, fileReader, fileSystemWatcher, scheduler);
      }

      private void ReloadSolution()
      {
         var solutionFile = LoadSolution(_solutionPath);
         var projectFiles = solutionFile.Projects.ToDictionary(x => x, LoadProject);
         _projects.Clear();
         projectFiles.Each(x => _projects.Add(x.Key, x.Value));
      }

      private SolutionFile LoadSolution(string path)
      {
         var content = _fileReader(path);
         return SolutionFileParser.Parse(content, path);
      }

      private ProjectFile LoadProject(string path)
      {
         var content = _fileReader(path);
         return ProjectFileParser.Parse(content, path);
      }

      private void FileSystemItemChanged(ChangedEventArgs args, SolutionChangedEventArgs solutionArgs)
      {
         if (_solutionPath.Equals(args.Path, StringComparison.CurrentCultureIgnoreCase))
         {
            var keys = _projects.Keys;
            ReloadSolution();
            keys.Where(x => !_projects.ContainsKey(x)).Each(x => solutionArgs.DeletedProjects.Add(x));
            _projects.Keys.Where(x => !keys.Contains(x)).Each(x => solutionArgs.AddedProjects.Add(x));
            return;
         }

         if (_projects.ContainsKey(args.Path))
         {
            _projects[args.Path] = LoadProject(args.Path);
            solutionArgs.ChangedProjects.Add(args.Path);
            return;
         }

         foreach (var projectPath in _projects.Where(x => x.Value.Files.Contains(args.Path)).Select(x => x.Key))
         {
            solutionArgs.ChangedProjects.Add(projectPath);
            return;
         }

         foreach (var projectPath in _projects.Where(x => x.Value.References.Contains(args.Path)).Select(x => x.Key))
         {
            solutionArgs.ChangedProjects.Add(projectPath);
            return;
         }
      }

      private void FileSystemItemCreated(CreatedEventArgs args, SolutionChangedEventArgs solutionArgs)
      {
         if (_projects.ContainsKey(args.Path))
         {
            _projects[args.Path] = LoadProject(args.Path);
            solutionArgs.ChangedProjects.Add(args.Path);
            return;
         }

         foreach (var projectPath in _projects.Where(x => x.Value.Files.Contains(args.Path)).Select(x => x.Key))
         {
            solutionArgs.ChangedProjects.Add(projectPath);
            return;
         }

         foreach (var projectPath in _projects.Where(x => x.Value.References.Contains(args.Path)).Select(x => x.Key))
         {
            solutionArgs.ChangedProjects.Add(projectPath);
            return;
         }
      }

      private void FileSystemItemRenamed(RenamedEventArgs args, SolutionChangedEventArgs solutionArgs)
      {
         if (_projects.ContainsKey(args.NewPath))
         {
            _projects[args.NewPath] = LoadProject(args.NewPath);
            solutionArgs.ChangedProjects.Add(args.NewPath);
            return;
         }

         foreach (var projectPath in _projects.Where(x => x.Value.Files.Contains(args.NewPath)).Select(x => x.Key))
         {
            solutionArgs.ChangedProjects.Add(projectPath);
            return;
         }

         foreach (var projectPath in _projects.Where(x => x.Value.References.Contains(args.NewPath)).Select(x => x.Key))
         {
            solutionArgs.ChangedProjects.Add(projectPath);
            return;
         }
      }

      public event EventHandler<SolutionChangedEventArgs> Changed = delegate { };

      private class FileSystemWatcherWrapper : IFileSystemWatcher
      {
         private readonly FileSystemWatcher _watcher;

         public event EventHandler<CreatedEventArgs> Created = delegate { };
         public event EventHandler<ChangedEventArgs> Changed = delegate { };
         public event EventHandler<DeletedEventArgs> Deleted = delegate { };
         public event EventHandler<RenamedEventArgs> Renamed = delegate { };

         public FileSystemWatcherWrapper()
         {
            _watcher = new FileSystemWatcher { IncludeSubdirectories = true };
            _watcher.Changed += (sender, args) => Changed(this, new ChangedEventArgs(args.FullPath));
            _watcher.Created += (sender, args) => Created(this, new CreatedEventArgs(args.FullPath));
            _watcher.Deleted += (sender, args) => Deleted(this, new DeletedEventArgs(args.FullPath));
            _watcher.Renamed += (sender, args) => Renamed(this, new RenamedEventArgs(args.OldFullPath, args.FullPath));
         }

         public string Directory
         {
            get { return _watcher.Path; }
            set
            {
               _watcher.Path = value;
               _watcher.EnableRaisingEvents = true;
               _watcher.IncludeSubdirectories = true;
            }
         }

         public void Dispose()
         {
            _watcher.Dispose();
         }
      }

      public void Dispose()
      {
         _fileSystemWatcher.Dispose();
      }
   }

   public class SolutionChangedEventArgs
   {
      public static readonly SolutionChangedEventArgs Empty = new SolutionChangedEventArgs();

      public SolutionChangedEventArgs()
      {
         AddedProjects = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
         ChangedProjects = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
         DeletedProjects = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
         RenamedProjects = new HashSet<Rename>();
      }

      public SolutionChangedEventArgs(IEnumerable<string> addedProjects, IEnumerable<string> changedProjects, IEnumerable<string> deletedProjects, IEnumerable<Rename> renamedProjects)
      {
         AddedProjects = (addedProjects ?? new string[] { }).ToSet(StringComparer.CurrentCultureIgnoreCase);
         ChangedProjects = (changedProjects ?? new string[] { }).ToSet(StringComparer.CurrentCultureIgnoreCase);
         DeletedProjects = (deletedProjects ?? new string[] { }).ToSet(StringComparer.CurrentCultureIgnoreCase);
         RenamedProjects = (renamedProjects ?? new Rename[] { }).ToSet();
      }

      public ISet<string> AddedProjects { get; private set; }
      public ISet<string> ChangedProjects { get; private set; }
      public ISet<string> DeletedProjects { get; private set; }
      public ISet<Rename> RenamedProjects { get; private set; }

      public bool IsEmpty
      {
         get { return !AddedProjects.Any() && !ChangedProjects.Any() && !DeletedProjects.Any() && !RenamedProjects.Any(); }
      }

      public class Rename
      {
         public Rename(string oldPath, string newPath)
         {
            OldPath = oldPath;
            NewPath = newPath;
         }

         public string OldPath { get; private set; }
         public string NewPath { get; private set; }
      }
   }

   public interface IFileSystemWatcher : IDisposable
   {
      event EventHandler<CreatedEventArgs> Created;
      event EventHandler<ChangedEventArgs> Changed;
      event EventHandler<DeletedEventArgs> Deleted;
      event EventHandler<RenamedEventArgs> Renamed;

      string Directory { get; set; }
   }

   public class ChangedEventArgs : EventArgs
   {
      public ChangedEventArgs(string path)
      {
         Path = path;
      }

      public string Path { get; private set; }
   }

   public class CreatedEventArgs : EventArgs
   {
      public CreatedEventArgs(string path)
      {
         Path = path;
      }

      public string Path { get; private set; }
   }

   public class DeletedEventArgs : EventArgs
   {
      public DeletedEventArgs(string path)
      {
         Path = path;
      }

      public string Path { get; private set; }
   }

   public class RenamedEventArgs : EventArgs
   {
      public RenamedEventArgs(string oldPath, string newPath)
      {
         OldPath = oldPath;
         NewPath = newPath;
      }

      public string OldPath { get; private set; }
      public string NewPath { get; private set; }
   }
}