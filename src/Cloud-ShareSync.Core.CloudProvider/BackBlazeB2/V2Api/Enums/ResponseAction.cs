using System.Text.Json.Serialization;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Enums {
    [JsonConverter( typeof( JsonStringEnumConverter ) )]
    public enum ResponseAction {
        /// <summary>
        /// "start" means that a large file has been started, but not finished or canceled.
        /// </summary>
        start,

        /// <summary>
        /// "upload" means a file that was uploaded to B2 Cloud Storage.
        /// </summary>
        upload,

        /// <summary>
        /// "hide" means a file version marking the file as hidden, so that it will not show up in b2_list_file_names.
        /// </summary>
        hide,

        /// <summary>
        /// "folder" is used to indicate a virtual folder when listing files.
        /// </summary>
        folder
    }
}
