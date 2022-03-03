# 💥❗ Cloud-ShareSync is in a pre-release state. ❗💥
Documentation may change at any time and without any notice.

<br><br>

# Cloud/ShareSync
[![GitHub](https://img.shields.io/github/license/DarkgreyDevelopment/Cloud-ShareSync?style=plastic)](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/blob/main/LICENSE)
[![latest release](https://img.shields.io/github/v/release/DarkgreyDevelopment/Cloud-ShareSync?include_prereleases&label=latest%20release&style=plastic)](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/releases/)
[![official release](https://img.shields.io/github/v/release/DarkgreyDevelopment/Cloud-ShareSync?label=official%20release&style=plastic)](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/releases/)  
[![main](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/actions/workflows/github-actions.yml/badge.svg?branch=main)](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/actions/workflows/github-actions.yml)
[![main](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/actions/workflows/codeql-analysis.yml/badge.svg?branch=main)](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/actions/workflows/codeql-analysis.yml)
[![main](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/actions/workflows/pages/pages-build-deployment/badge.svg?branch=main)](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/actions/workflows/pages/pages-build-deployment)

<br>

<p style='text-align: center;'>
<a href="https://cloud-sharesync.com"><img src="https://docs.cloud-sharesync.com/images/CloudShareSyncLogo.svg" alt="Cloud-ShareSync Logo" style="width:250px;height:250px"></a>
</p>

## Description:
Cloud-ShareSync is an open source backup application that allows you to backup a local directory to a cloud storage bucket.  

The long term goal of this project is to build a backup application, akin to [OneDrive](https://onedrive.com), that extends the Files On Demand/Fuse functionality to new public cloud storage providers. Supporting platforms such as linux and MacOS is also a priority.  

Currently Cloud-ShareSync (v1.x) has a much more limited scope. The project consists of two separate applications; SimpleBackup, and SimpleRestore.  

SimpleBackup and SimpleRestore are both console applications that runs on Windows, Linux[*](a "Linux is tested on Ubuntu latest."), and MacOS[*](a "MacOS is tested on macOS 11 Big Sur.").  

<br>

### SimpleBackup:
SimpleBackup is a backup app that can recursively search through a directory tree and upload files to a preconfigured [BackBlaze B2](https://www.backblaze.com/b2/cloud-storage.html) cloud storage bucket. File compression and encryption is also supported prior to upload to upload.  
A complete feature list can be found [here](https://docs.cloud-sharesync.com/articles/SimpleBackupFeatures.html).  

### SimpleRestore:
SimpleRestore is a complete restore app that takes the database output from SimpleBackup and uses it to download files and restore them to the path specified in the config. File decompression/decryption is also supported after download.  
A complete feature list can be found [here](https://docs.cloud-sharesync.com/articles/SimpleRestoreFeatures.html).  

<br>

## Documentation:
Application documentation can be found at [docs.cloud-sharesync.com](https://docs.cloud-sharesync.com).  

### Getting Started:
Cloud-ShareSync runs on Windows, Linux, and MacOS. Additionally container images are also available.  

Please refer to the [How-To Articles](https://docs.cloud-sharesync.com/articles/HowTo/index.html) before opening any [issues](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/issues).  

### Initial Setup:
- Windows Setup.
- Linux Setup.
- MacOS Setup.
- Docker container setup.

<br>

## Roadmap:
The project [planning & development board](https://github.com/orgs/DarkgreyDevelopment/projects/3) is where the specifics of the roadmap can be found.  

Cloud-ShareSync is currently in a pre-release state and will follow [semantic versioning](https://semver.org) upon release.  
Pre-Release/Phase 01:  
  - Status  : In Progress.  
  - Goal    : Establish initial functional backup and restore functionality.  
  - Features:  
    - A single config file controls all application settings.  
    - Console logging and rolling log files are enabled by default. OpenTelemetry trace logging is also available when enabled.  
    - Application data can be stored in either a local Sqlite database or a remote postgres database.  
    - Backup application is a simple console app.
      - Files are uploaded to a BackBlaze B2 bucket.  
        - BackBlaze B2 bucket specified by user in config.  
      - When the backup application is run repeatedly; Files that were uploaded to the bucket on prior runs will be skipped if they are unchanged.  
      - Optionally compress files before upload.  
        - You may also elect to have a unique password generated during the compression process to password protect compressed files.  
      - Optionally encrypt files before upload.  
        - File encryption depends on ChaCha20Poly1305 platform support.
    - Restore application downloads files from BackBlaze B2 that were uploaded by the the backup application. 
      - When the restore application is run repeatedly; Unchanged files that were downloaded from the bucket on prior runs will not be re-downloaded.  
      - Decompresses the optionally compressed files after download.  
      - Decrypts the optionally encrypted files after download.  

Phase 02:  
  - Status  : Developing Acceptance Criteria.  
  - Goal    : Build Selective Restore Application  
  - Features:  
    - All phase 01 features retained.  
    - Selective restore must be a GUI application.  
    - Selective restore will present a table/spreadsheet like view of previously uploaded files and will restore files upon selection.

Phase 03:  
  - Status  : Developing Acceptance Criteria.  
  - Goal    : Add additional cloud providers.  
  - Features:  
    - Retain features from all previous phases.  
    - Add ability to upload/download files to/from Azure Blob Storage.  
    - Add ability to upload/download files to/from Aws S3.  
    - Add ability to upload/download files to/from Google Cloud Storage.  

Phase 04:  
  - Status  : Developing Acceptance Criteria.  
  - Goal    : Combine SimpleBackup, SimpleRestore, and SelectiveRestore into a single app.  
  - Features:  
    - Unified application should run as a background service.  
    - Unified application should have an associated management GUI.  
      - Management GUI should primarily allow input of app settings.  
      - GUI should also contain the SelectiveRestore menu.  

Phase 05:  
  - Status  : Developing Acceptance Criteria.  
  - Goal    : Add Files On Demand/FUSE functionality.  
  - Features:  
    - Feature set TBD.  


Phase 06:  
  - Status  : Developing Acceptance Criteria.  
  - Goal    : Enable multi device support.  
  - Features:  
    - Feature set TBD.  
<br>

### Disclaimers*
Cloud-ShareSync is not affiliated with Microsoft, BackBlaze, Google, or Amazon.  
All code is provided free of charge, as is, and with no warranty under an [MIT license](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/blob/main/LICENSE).