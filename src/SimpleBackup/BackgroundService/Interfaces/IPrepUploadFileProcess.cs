namespace Cloud_ShareSync.SimpleBackup.BackgroundService.Interfaces {
    public interface IPrepUploadFileProcess {
        Task Prep( List<string> paths );
        Task Process( );
    }
}
