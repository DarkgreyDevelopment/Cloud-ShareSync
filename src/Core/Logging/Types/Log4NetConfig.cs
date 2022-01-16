﻿using System.Text.Json;

namespace Cloud_ShareSync.Core.Logging.Types {
    public class Log4NetConfig {
        public string? ConfigurationFile { get; set; } = null;
        public bool EnableDefaultLog { get; set; }
        public DefaultLogConfig? DefaultLogConfiguration { get; set; } = null;
        public bool EnableTelemetryLog { get; set; }
        public TelemetryLogConfig? TelemetryLogConfiguration { get; set; } = null;
        public bool EnableConsoleLog { get; set; }
        public ConsoleLogConfig? ConsoleConfiguration { get; set; } = null;

        public override string ToString( ) {
            JsonSerializerOptions options = new( ) {
                IncludeFields = true,
                WriteIndented = true,
            };
            return JsonSerializer.Serialize( this, options );
        }
    }
}
