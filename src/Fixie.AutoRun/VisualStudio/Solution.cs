using Fixie.AutoRun.FileSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using RenamedEventArgs = Fixie.AutoRun.FileSystem.RenamedEventArgs;

namespace Fixie.AutoRun.VisualStudio
{
   public class Solution : ISolution
   {
      private readonly string _solutionPath;
      private readonly Func<string, string> _fileReader;
      private readonly IFileSystemWatcher _fileSystemWatcher;
      private IDictionary<string, ProjectFile> _projects;

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

      public IReadOnlyCollection<string> Configurations { get; private set; }
      public IReadOnlyCollection<string> Platforms { get; private set; }

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

         Configurations = solutionFile.Configurations.ToList();
         Platforms = solutionFile.Platforms.ToList();

         _projects = solutionFile.Projects.ToDictionary(x => x, LoadProject);
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

      public void Dispose()
      {
         _fileSystemWatcher.Dispose();
      }

      public IEnumerator<IProject> GetEnumerator()
      {
         return _projects.Values.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         return GetEnumerator();
      }

      public IProject this[string projectPath]
      {
         get { return _projects[projectPath]; }
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
   }
}