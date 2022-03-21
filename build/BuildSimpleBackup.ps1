[CmdletBinding()]
param (
	[Parameter(Mandatory)]
	[string]$SOURCEPATH,

	[Parameter(Mandatory)]
	[string]$PUBLISHPATH
)

# Restore, Build, Publish SimpleBackup.
Write-Host 'Building and publishing SimpleBackup.' -ForegroundColor Green
dotnet restore "$SOURCEPATH/src/SimpleBackup/Cloud-ShareSync.SimpleBackup.csproj"

$PublishArguments = @(
    "$SOURCEPATH/src/SimpleBackup/Cloud-ShareSync.SimpleBackup.csproj",
    '-c', 'Release',
    '-o', "$PUBLISHPATH/Windows",
    '-r', 'win-x64',
    '-p:PublishSingleFile=true',
    '-p:PublishReadyToRun=true',
    '--self-contained'
)
dotnet publish @PublishArguments

# Add PublishReadyToRunComposite=true & TieredCompilation=false per https://docs.microsoft.com/en-us/dotnet/core/deploying/ready-to-run#composite-readytorun
$PublishArguments = @(
    "$SOURCEPATH/src/SimpleBackup/Cloud-ShareSync.SimpleBackup.csproj",
    '-c', 'Release',
    '-o', "$PUBLISHPATH/Linux",
    '-r', 'linux-x64',
    '-p:PublishSingleFile=true',
    '-p:PublishReadyToRun=true',
    '-p:PublishReadyToRunComposite=true'
    '-p:TieredCompilation=false'
    '--self-contained'
)
dotnet publish @PublishArguments

$PublishArguments = @(
    "$SOURCEPATH/src/SimpleBackup/Cloud-ShareSync.SimpleBackup.csproj",
    '-c', 'Release',
    '-o', "$PUBLISHPATH/MacOS",
    '-r', 'osx.11.0-x64',
    '-p:PublishSingleFile=true',
    '-p:PublishReadyToRun=true',
    '--self-contained'
)
dotnet publish @PublishArguments


$AppSettings = Join-Path -Path $SOURCEPATH -ChildPath 'appsettings.json'
$LicensePath = Join-Path -Path $SOURCEPATH -ChildPath 'LICENSE'
$READMEPath = Join-Path -Path $SOURCEPATH -ChildPath 'README.md'
$CopyParam = @{
    Verbose = $true
    Force   = $true
}

Foreach ($OsDir in "$PUBLISHPATH/Windows", "$PUBLISHPATH/Linux", "$PUBLISHPATH/MacOS") {
    $ConfigDir = Join-Path -Path $OsDir -ChildPath 'Configuration'
    New-Item -Path $ConfigDir -ItemType Directory -Force | Out-Null

    $ASOutput = Join-Path -Path $ConfigDir -ChildPath 'appsettings.json'
    Copy-Item -Path $AppSettings -Destination $ASOutput @CopyParam

    $LicenseOutput = Join-Path -Path $OsDir -ChildPath 'LICENSE'
    Copy-Item -Path $LicensePath -Destination $LicenseOutput @CopyParam

    $READMEOutput = Join-Path -Path $OsDir -ChildPath 'README.md'
    Copy-Item -Path $READMEPath -Destination $READMEOutput @CopyParam
}
