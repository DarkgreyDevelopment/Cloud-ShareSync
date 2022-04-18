namespace Cloud_ShareSync.SharedServices.BackgroundService.Interfaces {
    public interface IPrepUploadFileProcess {
        Task Prep( List<string> paths );
        Task Process( );
    }
}
