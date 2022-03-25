# Docker Setup:

This how to article assumes that you are already reasonably familiar with [docker](https://docs.docker.com/get-started/overview/).  
If you are not familiar with docker please refer to the [docker startup guide](https://docs.docker.com/get-started/).  

<br>

## Step 1: Download container image.
Start off by downloading the appropriate container image. A list of available container image tags can be [found here](https://github.com/darkgreydevelopment/Cloud-ShareSync/pkgs/container/cloud-sharesync/versions).  
```
docker pull ghcr.io/darkgreydevelopment/cloud-sharesync:{version-tag}
```

<br>

## Step 2: Create local containers.

### Backup:
```bash
docker create \
    --name=cloud-sharesync.backup \
    --restart=no \
    -v /App/Config:/config \
    -v /mnt/FileShare:/backup \
    -v /App/Working:/working \
    -v /App/Log:/log \
    -v /App/Database:/database \
    -e "CLOUDSHARESYNC_CONFIGPATH=/config/appsettings.json" \
    ghcr.io/darkgreydevelopment/cloud-sharesync.backup:restore-prerelease-20220324
```

### Parameters:
| Parameter                                                                    | Functionality |
| ---------------------------------------------------------------------------- | ------------- |
| --restart=no                                                                 | Do not auto restart on errors. |
| -v /App/Config:/config                                                       | Attaches the local path `/App/Config` to the container path `/config`. |
| -v /mnt/FileShare:/backup                                                    | Attaches the local path `/mnt/FileShare` to the container path `/backup`. |
| -v /App/Working:/working                                                     | Attaches the local path `/App/Working` to the container path `/working`. |
| -v /App/Log:/log                                                             | Attaches the local path `/App/Log` to the container path `/log`. |
| -v /App/Database:/database                                                   | Attaches the local path `/App/Database` to the container path `/database`. |
| -e "CLOUDSHARESYNC_CONFIGPATH=/config/appsettings.json"                      | Sets the `CLOUDSHARESYNC_CONFIGPATH` environment variable to `/config/appsettings.json` |

The paths mentioned in the container creation parameters should correlate with the paths listed in the appsettings.json config file.  

<br>

### Restore:
```bash
# Instructions to be added soon.
```

### Parameters:
| Parameter  | Functionality |
| ---------- | ------------- |
| Content Cell  | Content Cell  |
| Content Cell  | Content Cell  |


## Step 3: Start containers.
`docker start cloud-sharesync.backup`
`docker start cloud-sharesync.restore`