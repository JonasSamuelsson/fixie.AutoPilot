using System;

namespace Fixie.AutoRun.FileSystem
{
   public interface IFileSystemWatcher : IDisposable
   {
      event EventHandler<CreatedEventArgs> Created;
      event EventHandler<ChangedEventArgs> Changed;
      event EventHandler<DeletedEventArgs> Deleted;
      event EventHandler<RenamedEventArgs> Renamed;

      string Directory { get; set; }
   }
}