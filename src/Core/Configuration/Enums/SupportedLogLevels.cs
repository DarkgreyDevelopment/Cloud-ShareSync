﻿namespace Cloud_ShareSync.Core.Configuration.Enums {
    [Flags]
    public enum SupportedLogLevels {
        Fatal = 2,
        Error = 4,
        Warn = 8,
        Info = 16,
        Debug = 32,
        Telemetry = 64
    }
}