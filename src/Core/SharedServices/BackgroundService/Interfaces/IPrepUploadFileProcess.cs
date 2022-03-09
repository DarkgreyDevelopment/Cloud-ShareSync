namespace Cloud_ShareSync.Core.SharedServices.BackgroundService.Interfaces {
    public interface IPrepUploadFileProcess {
        Task Prep( List<string> paths );
        Task Process( );
    }
}
