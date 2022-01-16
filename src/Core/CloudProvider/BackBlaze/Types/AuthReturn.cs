namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types {
    internal class AuthReturn {
        internal AuthProcessData AuthData { get; set; }
        internal string? AuthToken { get; set; }

        internal AuthReturn(
            AuthProcessData authData,
            string? authToken
        ) {
            AuthData = authData;
            AuthToken = authToken;
        }
    }
}
