using System.Text.Json;

namespace Cloud_ShareSync.Configuration.Types {
    public class CompleteConfig {

        /// <summary>
        /// Creating the <see cref="CompleteConfig"/> requires the <see cref="SyncConfig"/> be set at a minimum.
        /// </summary>
        /// <param name="sync"></param>
        public CompleteConfig( SyncConfig sync ) { Sync = sync; }


        #region Fields

        public SyncConfig Sync { get; set; }
        public DatabaseConfig Database { get; set; } = new( );
        public Log4NetConfig? Logging { get; set; } = new( );
        public CompressionConfig? Compression { get; set; }
        public B2Config? BackBlaze { get; set; }

        #endregion Fields


        #region Methods

        /// <summary>
        /// Returns the <see cref="CompleteConfig"/> as a json string.
        /// </summary>
        public override string ToString( ) =>
            JsonSerializer.Serialize(
                this,
                new JsonSerializerOptions( ) {
                    IncludeFields = true,
                    WriteIndented = true,
                }
            );

        public static CompleteConfig FromString( string value ) =>
            JsonSerializer
                .Deserialize<CompleteConfig>(
                    value,
                    new JsonSerializerOptions( ) {
                        ReadCommentHandling = JsonCommentHandling.Skip
                    }
                )!;

        #endregion Methods

    }
}
