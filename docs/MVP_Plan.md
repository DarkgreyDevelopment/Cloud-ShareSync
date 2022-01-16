# MVP plan:
1. To accomplish the primary goal main goal I need to
```
4.  Mosly Done -- Get Zip cmdline dependency working.
        a. Not Done Yet -- Will be in Console App not dll -- Check for existence of dependency upon app startup. Make that info public.
5.  Create FileWatcher. -- Shell is done, need implementation.
8.  Get database / entity framework working.
9.  Setup db tables for each cloud vendor.
10. get backblaze upload/download working.
        a. Be able to set auto-delete/retention on a per file basis.
        - Upon seeing a file on the filesystem as deleted - set retention period of X
11. Build console interface/application to run Sync process.
        a. Build configuration settings
               i. Which CloudStorage Provider
              ii. Compression.
                    1. Enable/Disable.
                    2. DependencyPath.
                    3. Use Password Y/N?
             iii. Encryption.
                    1. Enable/Disable.
                    2. Export KeyFiles?
              iv. Custom log path.
               v. Working Directory.
              vi. Array of Paths to sync.
             vii. Array of Sub-paths/files to exclude.
            viii. CloudStorage Provider Specific Info.
                    1. AWS S3
                        - Stub.
                    2. Azure Blob Storage
                        - Stub.
                    3. BackBlaze B2
                        a. applicationKeyId
                        b. applicationKey
                        c. bucketName
                        d. bucketId
                        e. UploadThreads
                        f. MaxConsecutiveErrors
                        g. General Retention Period
                        h. Deleted Files Retention Period
                        i. Use Encryption Y/N?             // If your entire bucket is encrypted and you dont want to
                                1. customerKey Y/N?        // provide a custom key/unique key per file then you can turn
                                2. UniqueKey per file Y/N? // this off and the bucket encryption will still work.
                    d. Google Cloud Storage
                        - Stub.
        b. Accepts commandline args.
              i. Help
             ii. ConfigPath
            iii. WriteConfig
             iv. ExportDatabaseAsJson {PATH}
        c. Process should, initially, be started manually via the cmdline. Eventually convert to service model?
12. Build console interface/application to perform downloads. Menu driven console app.
        a. Be able to download a single file by
              i. Local Name/Path
             ii. Remote (cloud specifc) guid
            iii. Optionally by individual filename (selecting from a list if multiples exist.)
        b. Be able to re-sync all (remaining/non-autodeleted by retention policy) files from cloud storage.
        c. Decompression/Decryption should happen automatically.
        d. Stretch goal -
            Should also be able to re-sync a specific file from a specific timeframe (as allowed by cloud storage provider).
```
