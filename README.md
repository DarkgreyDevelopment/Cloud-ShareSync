<p style='text-align: center;'>
<a href="https://cloud-sharesync.com"><img src="https://docs.cloud-sharesync.com/images/CloudShareSyncBanner.png" alt="Cloud-ShareSync Banner"></a>
</p>

[![GitHub](https://img.shields.io/github/license/DarkgreyDevelopment/Cloud-ShareSync?style=plastic)](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/blob/main/LICENSE)
[![latest release](https://img.shields.io/github/v/release/DarkgreyDevelopment/Cloud-ShareSync?include_prereleases&label=latest%20release&style=plastic)](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/releases/)
[![official release](https://img.shields.io/github/v/release/DarkgreyDevelopment/Cloud-ShareSync?label=official%20release&style=plastic)](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/releases/)  
[![Releases](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/actions/workflows/github-actions.yml/badge.svg?branch=main)](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/actions/workflows/github-actions.yml)
[![CodeQL](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/actions/workflows/codeql-analysis.yml/badge.svg?branch=main)](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/actions/workflows/codeql-analysis.yml)
[![Documentation](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/actions/workflows/pages/pages-build-deployment/badge.svg)](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/actions/workflows/pages/pages-build-deployment)

<br>


# 💥❗ Cloud-ShareSync is in a pre-release state. ❗💥
Note: Documentation may change at any time and without any notice.

<br>

## Description:
Cloud-ShareSync is an open source cloud storage backup and restore application.

The long term goal of this project is to build a backup application, akin to [OneDrive](https://onedrive.com), that extends the Files On Demand/Fuse functionality to new public cloud storage providers. Supporting platforms such as linux and MacOS is also a priority.

Cloud-ShareSync (v1.x) will have a much more limited scope. The application currently (v0.7.0-PreRelease) has two separate modes of operation; Backup and Restore.

Cloud-ShareSync runs on Windows[*](https://docs.cloud-sharesync.com/articles/Testing.html "Windows is tested on Windows Server 2022."), Linux[*](https://docs.cloud-sharesync.com/articles/Testing.html "Linux is tested on Ubuntu latest."), and MacOS[*](https://docs.cloud-sharesync.com/articles/Testing.html "MacOS is tested on macOS 11 Big Sur.").


<br>


## Documentation:
Project documentation can be found at [docs.cloud-sharesync.com](https://docs.cloud-sharesync.com).

### Getting Started:
Cloud-ShareSync runs on Windows, Linux, and MacOS. Additionally container images are also available.

Please refer to the [How-To Articles](https://docs.cloud-sharesync.com/articles/HowTo/index.html) before opening any [issues](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/issues).

### Initial Setup:
- [Windows Setup](https://docs.cloud-sharesync.com/articles/HowTo/WindowsSetup.html).
- [Linux Setup](https://docs.cloud-sharesync.com/articles/HowTo/LinuxSetup.html).
- [MacOS Setup](https://docs.cloud-sharesync.com/articles/HowTo/MacOsSetup.html).
- [Docker container setup](https://docs.cloud-sharesync.com/articles/HowTo/DockerSetup.html).


<br>


## Roadmap:
The project [planning & development board](https://github.com/orgs/DarkgreyDevelopment/projects/3) is where the specifics of the roadmap can be found.  
Cloud-ShareSync is currently in a pre-release state and will follow [semantic versioning](https://semver.org) upon release.

### Phase 01 - V0.7:
  - Status  : Nearing Completion.
  - Goal    : Establish initial backup and restore functionality.
  - Features:
    - Backup / Restore modes are specified by passing in arguments via the commandline.
      - Ex: `Cloud-ShareSync.exe backup`
      - Ex: `Cloud-ShareSync.exe restore`
    - A config file controls the the remaining application settings.
    - Console logging and rolling log files are enabled by default.
	- OpenTelemetry trace logging is also available when enabled (currently partially enabled).
    - Application data can be stored in a local Sqlite database.
    - Application can run in either console or GUI mode. 
    - Files can be uploaded/downloaded to/from a BackBlaze B2 bucket.
      - BackBlaze B2 bucket specified by user in config.
    - When the backup mode is run repeatedly; Files that were uploaded to the bucket on prior runs will be skipped if they are unchanged.
    - Files can be compressed before upload.
      - You may also elect to have a unique password generated during the compression process to password protect compressed files.
      - File compression has a [dependency on 7-Zip](https://docs.cloud-sharesync.com/articles/7ZipDependency.html).
    - Files can be encrypted prior to upload.
      - File encryption depends on [ChaCha20Poly1305 platform support](https://docs.cloud-sharesync.com/api/Cloud_ShareSync.Core.Cryptography.FileEncryption.ManagedChaCha20Poly1305.html).
    - Restore mode uses the database to restore all files that were uploaded to BackBlaze B2 by the the backup application.
    - Restore mode will decompresses files after download if they were compressed during the upload process.
    - Restore mode decrypts files after download if they were encrypted during the upload process.

### Phase 02 - v0.8:
  - Status  : Solidifying Acceptance Criteria.
  - Goal    : Add Azure Blob Storage, Postgres, and Selective Restore functionality
  - Features:
    - Add ability to upload/download files to/from [Azure Blob Storage](https://azure.microsoft.com/en-us/services/storage/blobs).
      - Backup process can archive to both BackBlazeB2 and Azure Blob Storage simultaneously.
      - Restore mode requires specifying which cloud provider to restore files from.
    - Enable Postgres alternative DB functionality.
      - Postgres OR Sqlite. Not both.
    - Add selective restore GUI option.
      - Selective restore GUI will present a table/spreadsheet like view of previously uploaded files.
      - Allows the user to restore individual files from either configured cloud provider.

### Phase 03 - v0.9:
  - Status  : Developing Acceptance Criteria.
  - Goal    : Add "Sync" functionality & convert to background service.
  - Features:
    - Application needs to combine backup and restore modes into an always on "Sync" mode.
    - Application should run as a background service.
    - Application should (continue to) have an associated management GUI.
      - Management GUI should primarily allow input of app settings.
      - GUI should also contain the SelectiveRestore menu.

### Phase 04 - Pre-1.0 Release:
  - Status  : Developing Acceptance Criteria.
  - Goal    : Focus on code quality and tests.


### Beyond:
  - Status  : Developing Acceptance Criteria.
  - Goal    : Add additional cloud providers & Add Files On Demand/FUSE functionality.
  - Features:
    - Add ability to upload/download files to/from [Aws S3](https://aws.amazon.com/s3).
    - Add ability to upload/download files to/from [Google Cloud Storage](https://cloud.google.com/storage).
  - Reference Docs:
    - https://docs.microsoft.com/en-us/windows/win32/cfapi/cloud-files-api-portal
    - https://docs.microsoft.com/en-us/windows/win32/cfapi/build-a-cloud-file-sync-engine


<br>


### Contributing:
Contributing is encouraged!  
Please help keep the repository inclusive and fun! Abusive, rude, disrespectful or inappropriate behavior will not be tolerated.  
Read the [Code of Conduct](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/blob/main/CODE_OF_CONDUCT.md) for more details.  
- ToDo: Finish writing contribution guides.
  - Good examples: [here](https://github.com/microsoft/terminal/blob/main/CONTRIBUTING.md), [here](https://github.com/microsoft/vscode#contributing), and [here](https://github.com/microsoft/PowerToys/blob/main/CONTRIBUTING.md).
  - Example [CLA](https://opensource.microsoft.com/pdf/microsoft-contribution-license-agreement.pdf).


<br>


### Disclaimers*
Cloud-ShareSync is not affiliated with Microsoft, BackBlaze, Google, or Amazon.
All code is provided free of charge, as is, and with no warranty under an [MIT license](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/blob/main/LICENSE).
