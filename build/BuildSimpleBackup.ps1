[CmdletBinding()]
param (
	[Parameter(Mandatory)]
	[string]$SOURCEPATH = "$Env:CLOUDSHARESYNC_SOURCEPATH",

	[Parameter(Mandatory)]
	[string]$BUILDPATH = "$Env:CLOUDSHARESYNC_BUILDPATH",

	[Parameter(Mandatory)]
	[string]$PUBLISHPATH = "$Env:CLOUDSHARESYNC_PUBLISHPATH",

	[Parameter(Mandatory)]
	[string]$B2_AppKeyId = "$Env:BACKBLAZEB2_AppKeyId",

	[Parameter(Mandatory)]
	[string]$B2_AppKey = "$Env:BACKBLAZEB2_AppKey",

	[Parameter(Mandatory)]
	[string]$B2_bucketName = "$Env:BACKBLAZEB2_bucketName",

	[Parameter(Mandatory)]
	[string]$B2_bucketId = "$Env:BACKBLAZEB2_bucketId"
)

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
$Content = $Content.Replace('{applicationKey}', $B2_AppKey)
$Content = $Content.Replace('{bucketName}', $B2_bucketName)
$Content = $Content.Replace('{bucketId}', $B2_bucketId)
Set-Content -Path $BuildSettings -Value $Content -Verbose:$Verbose -ErrorAction Stop

# Restore, Build, Publish SimpleBackup.
Write-Host 'Building and publishing SimpleBackup.' -ForegroundColor Green
dotnet restore "$BUILDPATH/src/SimpleBackup/Cloud-ShareSync.SimpleBackup.csproj"
dotnet publish "$BUILDPATH/src/SimpleBackup/Cloud-ShareSync.SimpleBackup.csproj" -c Release -o "$PUBLISHPATH"

$DefaultConfigPath = Join-Path -Path $PUBLISHPATH -ChildPath 'Configuration'
Write-Host "Copying appsettings.json to default config path '$DefaultConfigPath'." -ForegroundColor Green
if ((Test-Path -Path $DefaultConfigPath) -eq $false) {
	New-Item -Path $DefaultConfigPath -ItemType Directory -ErrorAction Stop -Verbose:$Verbose 1> $null
}
Copy-Item -Path $BuildSettings -Destination $DefaultConfigPath -ErrorAction Stop -Verbose:$Verbose
