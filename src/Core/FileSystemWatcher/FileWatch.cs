using System.Collections.Concurrent;

namespace Cloud_ShareSync.Core.FileSystemWatcher {
    public class FileWatch : IDisposable {

        public ConcurrentDictionary<System.IO.FileSystemWatcher, Exception> _errorEvents = new( );
        public ConcurrentQueue<EventData> _createdEvents = new( );
        public ConcurrentQueue<EventData> _changedEvents = new( );
        public ConcurrentQueue<EventData> _renamedEvents = new( );
        public ConcurrentQueue<EventData> _deletedEvents = new( );

        private readonly System.IO.FileSystemWatcher _fileSystemWatcher;

        public FileWatch(
            FileSystemInfo path,
            string? filter = null,
            NotifyFilters? notifyFilter = null,
            bool includeSubdirectories = false
        ) {
            System.IO.FileSystemWatcher watcher = (filter == null) ?
                                                        new( path.FullName ) :
                                                        new( path.FullName, filter );

            watcher.NotifyFilter = (notifyFilter != null) ?
                                        (NotifyFilters)notifyFilter :
                                        NotifyFilters.DirectoryName
                                        | NotifyFilters.FileName
                                        | NotifyFilters.Size
                                        | NotifyFilters.CreationTime
                                        | NotifyFilters.LastAccess
                                        | NotifyFilters.LastWrite;

            watcher.Changed += OnChanged;
            watcher.Created += OnCreated;
            watcher.Deleted += OnDeleted;
            watcher.Renamed += OnRenamed;
            watcher.Error += OnError;

            watcher.IncludeSubdirectories = includeSubdirectories;
            watcher.EnableRaisingEvents = true;

            _fileSystemWatcher = watcher;
        }

        void IDisposable.Dispose( ) {
            _fileSystemWatcher.Dispose( );
            GC.SuppressFinalize( this );
        }

        private void OnCreated( object sender, FileSystemEventArgs e )
            => UniqueEnqueue( _createdEvents, new( e.FullPath ) );

        private void OnChanged( object sender, FileSystemEventArgs e )
            => UniqueEnqueue( _changedEvents, new( e.FullPath ) );

        private void OnRenamed( object sender, RenamedEventArgs e )
            => UniqueEnqueue( _renamedEvents, new( e.FullPath, e.OldFullPath ) );

        private void OnDeleted( object sender, FileSystemEventArgs e )
            => UniqueEnqueue( _deletedEvents, new( e.FullPath ) );

        private void OnError( object sender, ErrorEventArgs e ) =>
            _errorEvents.AddOrUpdate(
                (System.IO.FileSystemWatcher)sender,
                e.GetException( ),
                ( key, oldValue ) => e.GetException( )
            );

        public static void UniqueEnqueue( ConcurrentQueue<EventData> queue, EventData @event ) {
            if (queue.Contains( @event ) == false)
                queue.Enqueue( @event );
        }

    }
}
