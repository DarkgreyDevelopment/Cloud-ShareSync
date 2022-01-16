[CmdletBinding()]
param (
	[string]$SOURCEPATH    = "$Env:CLOUDSHARESYNC_SOURCEPATH",
	[string]$BUILDPATH     = "$Env:CLOUDSHARESYNC_BUILDPATH",
	[string]$PUBLISHPATH   = "$Env:CLOUDSHARESYNC_PUBLISHPATH",
	[string]$B2_AppKeyId   = "$Env:BACKBLAZEB2_AppKeyId",
	[string]$B2_AppKey     = "$Env:BACKBLAZEB2_AppKey",
	[string]$B2_bucketName = "$Env:BACKBLAZEB2_bucketName",
	[string]$B2_bucketId   = "$Env:BACKBLAZEB2_bucketId",
	[switch]$DockerBuild
)

if (-not $DockerBuild) {
	if (
		[string]::IsNullOrWhiteSpace($SOURCEPATH)    -or
		[string]::IsNullOrWhiteSpace($BUILDPATH)     -or
		[string]::IsNullOrWhiteSpace($PUBLISHPATH)   -or
		[string]::IsNullOrWhiteSpace($B2_AppKeyId)   -or
		[string]::IsNullOrWhiteSpace($B2_AppKey)     -or
		[string]::IsNullOrWhiteSpace($B2_bucketName) -or
		[string]::IsNullOrWhiteSpace($B2_bucketId)
	) { throw 'Missing required variables!' }

	$Verbose = $PSBoundParameters.Keys -icontains 'Verbose' ? $PSBoundParameters['Verbose'] : $false

	$SystemRoot = $IsLinux ? '/' : (Resolve-Path -Path $env:SystemDrive).Path

	if (
		((Resolve-Path -Path $BUILDPATH -ErrorAction SilentlyContinue).Path -eq $SystemRoot) -or
		((Resolve-Path -Path $PUBLISHPATH -ErrorAction SilentlyContinue).Path -eq $SystemRoot)
	) { throw 'BUILDPATH and PUBLISHPATH must be somewhere other than the System Root Directory.' }

	# Ensure BUILDPATH exists.
	if ((Test-Path -Path $BUILDPATH) -eq $false) {
		New-Item -Path $BUILDPATH -ItemType Directory -ErrorAction Stop -Verbose:$Verbose 1> $null
	}

	# Cleanup old files.
	$CancelStatement = 'To cancel press Ctrl+C. ' +
	'Otherwise hit any (other) key combination to continue and clean out the specified path.'
	if ((Get-ChildItem -Path $BUILDPATH -ErrorAction SilentlyContinue).Count -gt 0) {
		Read-Host -Prompt "BUILDPATH '$BUILDPATH' has existing files. $CancelStatement"
	}
	Remove-Item -Path "$BUILDPATH/*" -Recurse -Verbose:$Verbose -ErrorAction SilentlyContinue -Force

	if ((Get-ChildItem -Path $PUBLISHPATH -ErrorAction SilentlyContinue).Count -gt 0) {
		Read-Host -Prompt "PUBLISHPATH '$PUBLISHPATH' has existing files. $CancelStatement"
	}
	Remove-Item -Path "$PUBLISHPATH" -Recurse -Verbose:$Verbose -ErrorAction SilentlyContinue -Force

	# Copy code to build path.
	Write-Host "Copying source files to '$BUILDPATH'." -ForegroundColor Green
	Copy-Item -Path "$SOURCEPATH/*" -Destination "$BUILDPATH/" -Recurse -Verbose:$Verbose -ErrorAction Stop

	# Populate BackBlaze appsettings.
	Write-Host 'Replacing backblaze defaults in AppSettings file.' -ForegroundColor Green
	$BuildSettings = "$BUILDPATH/src/Core/Configuration/appsettings.json"
	$Content = Get-Content -Path $BuildSettings -ErrorAction Stop
	$Content = $Content.Replace('{applicationKeyId}', $B2_AppKeyId)
	$Content = $Content.Replace('{applicationKey}',   $B2_AppKey)
	$Content = $Content.Replace('{bucketName}',       $B2_bucketName)
	$Content = $Content.Replace('{bucketId}',         $B2_bucketId)
	Set-Content -Path $BuildSettings -Value $Content -Verbose:$Verbose -ErrorAction Stop

	# Restore, Build, Publish BucketSync.
	Write-Host 'Building and publishing applcation.' -ForegroundColor Green
	dotnet restore "$BUILDPATH/src/BucketSync/Cloud-ShareSync.BucketSync.csproj"
	dotnet publish "$BUILDPATH/src/BucketSync/Cloud-ShareSync.BucketSync.csproj" -c Release -o "$PUBLISHPATH"

	$DefaultConfigPath = Join-Path -Path $PUBLISHPATH -ChildPath 'Configuration'
	Write-Host "Copying appsettings.json to default config path '$DefaultConfigPath'." -ForegroundColor Green
	if ((Test-Path -Path $DefaultConfigPath) -eq $false) {
		New-Item -Path $DefaultConfigPath -ItemType Directory -ErrorAction Stop -Verbose:$Verbose 1> $null
	}
	Copy-Item -Path $BuildSettings -Destination $DefaultConfigPath -ErrorAction Stop -Verbose:$Verbose
} else {
    $DockerBuildDir = (Get-Item $PSCommandPath).Directory.FullName
	$DockerStartPath = (Get-Item $DockerBuildDir).Parent.FullName
	$DockerFile = (Get-Item (Join-Path -Path $DockerBuildDir -ChildPath Dockerfile)).FullName

	# Congigure 7Zip dependency paths
	$DependencyPath = Join-Path -Path $DockerStartPath -ChildPath 'Dependency'
	$Windows7ZipDependency = Join-Path -Path $DependencyPath -ChildPath 'Windows'

	if (((Test-Path -Path $Windows7ZipDependency) -eq $false) -and $IsWindows) {
		New-Item -Path $Windows7ZipDependency -ItemType Directory -ErrorAction Stop -Verbose:$Verbose 1> $null
	}
	try {
		& 'docker' image build --no-cache -t cloud-sharesync-simplebackup:latest -f $DockerFile $DockerStartPath
		if ($LASTEXITCODE -ne 0) { throw }
		"Example Docker Run Cmd:`n"
		"docker run -d --name=simplebackup -e 'CLOUDSHARESYNC_CONFIGPATH=/config/appsettings.json'" +
			" -v 'C:\Temp\config:/config' -v 'C:\Temp\AppTesting:/tmp/AppTesting' 'cloud-sharesync-simplebackup:latest'"

        "`n`n"
        "Example Docker Create Cmd:`n" +
            "docker create --name=simplebackup --restart=always -e 'CLOUDSHARESYNC_CONFIGPATH=/config/appsettings.json'" +
            " -v /mnt/BackupDrive/BackupFolder:/backup -v /mnt/BackupDrive/Working:/working -v /mnt/BackupDrive/Log:/log" +
            " -v /mnt/BackupDrive/Database:/database -v /mnt/BackupDrive/Config:/config cloud-sharesync-simplebackup:latest"
	} catch {
		Write-Error -Message "Failed to create docker image. LastExitCode: $LASTEXITCODE"
	}
}
