## VERSION 00.01.000-alpha -- A useful backblaze backup agent.
### Goals:
- Create a docker container that is entirely self contained.
	- Also ensure app works when directly hosted.
- Be able to mount config and media folders into the container.
	- /home/docker/Cloud-ShareSync/config == /config
	- \\UNC.Path\Share\Pictures           == /mnt/Pictures
	- \\UNC.Path\Share\Movies             == /mnt/Movies
	- \\UNC.Path\Share\Television         == /mnt/Tv
	- \\UNC.Path\Share\Music              == /mnt/Music
	- \\UNC.Path\Share\Books              == /mnt/Books
- Once mounted the application should read the configuration file.
	- Configure general app settings
		- ex: WorkingDirectory, RootFolders, UseEncryption, UseCompression, CloudProviders to use, etc.
	- Configure Database Connection.
		- Initialize w/ configuration settings.
	- For each CloudProvider
		- Initialize w/ configuration settings.
	- Check Encryption Configuration
		- If enabled check platform supported.
	- Check Compression Configuration
		- If enabled check dependency exists.
		- Initialize w/ configuration settings.
- Once the application has been configured it should begin syncing the root folders up with all cloud providers configured.
	- Only BackBlaze will be enabled for use at first.

<br><br>


### Initial Sync Process:
- Application should get a list of all files under the root directories (recursively
  as specified in the config).
- Application should get a list of folders that it needs to begin monitoring.
- Application should take the list of files, sort it by last modified (oldest first), then add the list to the processing queue.
- Application should then place a filewatch on all required folders.
	- OnCreate/OnChange - Add file path to processing queue (if its not already on it).
	- OnRename - Search processing queue for old name. Update processing queue with name change if file should be processed.
		- If file is not in processing queue then add it.
	- OnDelete - Add file to delete processing queue.

### Processing Queue:
- TBD

### Delete Processing Queue:
- TBD


VERSION 01.00.000 SHOULD HAVE WORKING PROJECTED FILESYSTEM APP ON WINDOWS AND LINUX.
	(Similar to onedrive's ability - but extended for multiple cloud storage providers.)
	Note for Windows: Projected File System only works on ntfs drives.
	Tech Doc: https://docs.microsoft.com/en-us/windows/win32/cfapi/build-a-cloud-file-sync-engine
