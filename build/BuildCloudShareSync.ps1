[CmdletBinding()]
param (
    [Parameter(Mandatory)]
    [string]$SOURCEPATH,

    [Parameter(Mandatory)]
    [string]$PUBLISHPATH
)

# Restore, Build, Publish SimpleBackup.
Push-Location $SOURCEPATH
$ProjectPath = Resolve-Path -Path "$SOURCEPATH/src/Cloud-ShareSync.csproj"
Write-Host 'Restoring Cloud-ShareSync' -ForegroundColor Green
dotnet restore $ProjectPath

$PublishProfiles = @('PublishWindows', 'PublishLinux', 'PublishMacOS')
Foreach ($PubProfile in $PublishProfiles) {
    $joinPathSplat = @{
        Path                = $SOURCEPATH
        ChildPath           = 'src'
        AdditionalChildPath = @('Properties', 'PublishProfiles', "$PubProfile.pubxml")
        Resolve             = $true
    }
    $ProfilePath = Join-Path @joinPathSplat
    Write-Host "Cloud-ShareSync $PubProfile" -ForegroundColor Green
    dotnet publish "/p:PublishProfile=$ProfilePath"
}

$AppSettings = Join-Path -Path $SOURCEPATH -ChildPath 'appsettings.json'
$LicensePath = Join-Path -Path $SOURCEPATH -ChildPath 'LICENSE'
$READMEPath = Join-Path -Path $SOURCEPATH -ChildPath 'README.md'
$CopyParam = @{
    Verbose = $true
    Force   = $true
}

Foreach ($OsDir in "$PUBLISHPATH/windows", "$PUBLISHPATH/linux", "$PUBLISHPATH/macos") {
    $ConfigDir = Join-Path -Path $OsDir -ChildPath 'Configuration'
    New-Item -Path $ConfigDir -ItemType Directory -Force | Out-Null

    $ASOutput = Join-Path -Path $ConfigDir -ChildPath 'appsettings.json'
    Copy-Item -Path $AppSettings -Destination $ASOutput @CopyParam

    $LicenseOutput = Join-Path -Path $OsDir -ChildPath 'LICENSE'
    Copy-Item -Path $LicensePath -Destination $LicenseOutput @CopyParam

    $READMEOutput = Join-Path -Path $OsDir -ChildPath 'README.md'
    Copy-Item -Path $READMEPath -Destination $READMEOutput @CopyParam
}
