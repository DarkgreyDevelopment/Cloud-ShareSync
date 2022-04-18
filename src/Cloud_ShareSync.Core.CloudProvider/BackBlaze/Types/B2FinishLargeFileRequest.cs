using System.Runtime.Serialization;
using System.Text.Json;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types {
#nullable disable
#pragma warning disable IDE1006 // Naming Styles
    [DataContract]
    internal class B2FinishLargeFileRequest {
        [DataMember]
        internal string fileId;
        [DataMember]
        internal List<string> partSha1Array;

        public override string ToString( ) {
            return JsonSerializer.Serialize(
                new _tempJsonObj( this ),
                new JsonSerializerOptions( ) {
                    IncludeFields = true,
                    WriteIndented = true,
                }
            );
        }

        private class _tempJsonObj {
            public string fileId { get; set; }
            public string[] partSha1Array { get; set; }
            internal _tempJsonObj( B2FinishLargeFileRequest obj ) {
                fileId = obj.fileId;
                partSha1Array = obj.partSha1Array.ToArray( );
            }
        }
    }
#pragma warning restore IDE1006 // Naming Styles
#nullable enable
}
