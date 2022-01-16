namespace Cloud_ShareSync.BucketSync.Process {
    public interface ILocalSyncProcess {
        void Startup( );
        Task Process( );
    }
}
