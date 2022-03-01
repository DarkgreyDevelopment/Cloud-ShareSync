# Cloud/ShareSync
[![GitHub](https://img.shields.io/github/license/DarkgreyDevelopment/Cloud-ShareSync?style=plastic)](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/blob/main/LICENSE)
[![latest release](https://img.shields.io/github/v/release/DarkgreyDevelopment/Cloud-ShareSync?include_prereleases&label=latest%20release&style=plastic)](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/releases/)
[![official release](https://img.shields.io/github/v/release/DarkgreyDevelopment/Cloud-ShareSync?label=official%20release&style=plastic)](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/releases/)  
[![main](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/actions/workflows/github-actions.yml/badge.svg?branch=main)](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/actions/workflows/github-actions.yml)
[![main](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/actions/workflows/codeql-analysis.yml/badge.svg?branch=main)](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/actions/workflows/codeql-analysis.yml)
[![main](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/actions/workflows/pages/pages-build-deployment/badge.svg?branch=main)](https://github.com/DarkgreyDevelopment/Cloud-ShareSync/actions/workflows/pages/pages-build-deployment)

<br>

## Description:

[!NOTE]
Information the user should notice even if skimming.
[!TIP]
Optional information to help a user be more successful.
[!IMPORTANT]
Essential information required for user success.
[!CAUTION]
Negative potential consequences of an action.
[!WARNING]
Dangerous certain consequences of an action.

<p style='text-align: right;'>
<img src="https://docs.cloud-sharesync.com/images/CloudShareSyncLogo.svg">
</p>
<p style='text-align: left;'>
</p>
[!CAUTION]
Cloud-ShareSync is currently in a pre-release state. all information provided may change without notice. ***

Cloud-ShareSync is an open source backup application that allows you to backup a directory tree to a cloud storage bucket.  

This projects long term goal is to build a cloud backup application, akin to [OneDrive](https://onedrive.com), that extends the Files On Demand/Fuse functionality to additional public cloud storage providers. Supporting additional platforms such as linux and MacOS is also a priority.  

Currently Cloud-ShareSync (v1.x) has a much more limited scope. The project consists of two separate applications; SimpleBackup, and SimpleRestore.  

SimpleBackup is a console application that runs on Windows, Linux*, and MacOS. It is a backup app that can recursively search through a directory tree and upload all files to a preconfigured BackBlaze B2 cloud storage bucket. Files can also be optionally compressed and/or encrypted prior to upload.
- 

\* Linux support is tested on Ubuntu

Version 1 of the app supports backing up files and restoring files from BackBlaze B2 isonly has support backing up and restoring files to BackBlaze B2 cloud storage.  
Files On Demand/Fuse functionality will be added after additional cloud storage providers are supported. Next on the list for support is Azure Blob Storage. After that the roadmap contains both AWS S3 as well as Google Cloud Storage.

</p>

## Documentation:
Documentation can be found at [docs.cloud-sharesync.com](https://docs.cloud-sharesync.com).  
[Api Documentation](https://docs.cloud-sharesync.com/api/index.html), [How-To Articles](https://docs.cloud-sharesync.com/articles/HowTo/index.html)  


<br>


## Getting Started:
### Installation:
Cloud-ShareSync 

### Docker:


### Configuration:
Cloud-ShareSync expects that you have already created an account with BackBlaze and that you have the ability to create an [application key](https://www.backblaze.com/b2/docs/application_keys.html) for your account.  
Here is a [helpful BackBlaze article](https://help.backblaze.com/hc/en-us/articles/360052129034-Creating-and-Managing-Application-Keys) that outlines the process of creating an application key via the web gui.  



## Roadmap:
The project [planning & development board](https://github.com/orgs/DarkgreyDevelopment/projects/3) is where the initial development roadmap can be found.  

<br>


## Contributing:
