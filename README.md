# Cloud/ShareSync
[![GitHub](https://img.shields.io/github/license/DarkgreyDevelopment/Cloud-ShareSync?style=plastic)](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/blob/main/LICENSE)
[![latest release](https://img.shields.io/github/v/release/DarkgreyDevelopment/Cloud-ShareSync?include_prereleases&label=latest%20release&style=plastic)](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/releases/)
[![official release](https://img.shields.io/github/v/release/DarkgreyDevelopment/Cloud-ShareSync?label=official%20release&style=plastic)](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/releases/)  
[![main](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/actions/workflows/github-actions.yml/badge.svg?branch=main)](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/actions/workflows/github-actions.yml)
[![main](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/actions/workflows/codeql-analysis.yml/badge.svg?branch=main)](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/actions/workflows/codeql-analysis.yml)
[![main](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/actions/workflows/pages/pages-build-deployment/badge.svg?branch=main)](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/actions/workflows/pages/pages-build-deployment)

<br>

# 💥❗ Cloud-ShareSync is currently in a pre-release state. ❗💥
Documentation may change at any time and without any notice.

<p style='text-align: right;'>
<img src="https://docs.cloud-sharesync.com/images/CloudShareSyncLogo.svg">
</p>

## Description:
Cloud-ShareSync is an open source backup application that allows you to backup a local directory to a cloud storage bucket.  

The long term goal of this project is to build a backup application, akin to [OneDrive](https://onedrive.com), that extends the Files On Demand/Fuse functionality to new public cloud storage providers. Supporting platforms such as linux and MacOS is also a priority.  

Currently Cloud-ShareSync (v1.x) has a much more limited scope. The project consists of two separate applications; SimpleBackup, and SimpleRestore.  

SimpleBackup and SimpleRestore are both console applications that runs on Windows, Linux[*](a "Linux is tested on Ubuntu latest."), and MacOS[*](a "MacOS is tested on macOS 11 Big Sur.").  

<br>

### SimpleBackup:
SimpleBackup is a backup app that can recursively search through a directory tree and upload files to a preconfigured [BackBlaze B2](https://www.backblaze.com/b2/cloud-storage.html) cloud storage bucket. File compression and encryption is also supported prior to upload to upload. A complete feature list can be found [here](https://docs.cloud-sharesync.com/articles/SimpleBackupFeatures.html).  

<br>

### SimpleRestore:
SimpleRestore is a complete restore app that takes the database output from SimpleBackup and uses it to download files and restore them to the path specified in the config. File decompression/decryption is also supported after download. A complete feature list can be found [here](https://docs.cloud-sharesync.com/articles/SimpleRestoreFeatures.html).  

<br>

## Documentation:
Documentation can be found at [docs.cloud-sharesync.com](https://docs.cloud-sharesync.com).  

<br>

## Getting Started:
Cloud-ShareSync runs on Windows, Linux, and MacOS. Additionally a container images are provided to make trying Cloud-ShareSync as simple as possible.  

Please refer to the [How-To Articles](https://docs.cloud-sharesync.com/articles/HowTo/index.html) before opening any [issues](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/issues).  


### Installation:
- Windows Install Guide.
- Linux Install Guide.
- MacOS Install Guide.
- Docker container setup.

<br>

## Roadmap:
The project [planning & development board](https://github.com/orgs/DarkgreyDevelopment/projects/3) is where the initial development roadmap can be found.  
Files On Demand/Fuse functionality will be added after the projects primary cloud storage providers are supported. Next on the list for support is Azure Blob Storage. After that the roadmap contains both AWS S3 as well as Google Cloud Storage.

<br>
<br>

### Disclaimers*
Cloud-ShareSync is not affiliated with Microsoft, BackBlaze, Google, or Amazon.  
All code is provided free of charge, as is, and with no warranty under an [MIT license](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/blob/main/LICENSE).