[CmdletBinding()]
param (
    [Parameter(Mandatory)]
    [string]$SOURCEPATH,

    [Parameter(Mandatory)]
    [string]$PUBLISHPATH
)

# Restore, Build, Publish SimpleBackup.
Push-Location $SOURCEPATH
$Project1Path = Resolve-Path -Path "$SOURCEPATH/src/Cloud-ShareSync.GUI/Cloud-ShareSync.GUI.csproj"
$Project2Path = Resolve-Path -Path "$SOURCEPATH/src/Cloud-ShareSync.Commandline/Cloud-ShareSync.Commandline.csproj"
Write-Host 'Restoring Cloud-ShareSync' -ForegroundColor Green
dotnet restore $Project1Path
dotnet restore $Project2Path

$PublishProfiles = @('PublishWindows', 'PublishWindows_FrameWork', 'PublishLinux', 'PublishLinux_FrameWork', 'PublishMacOS', 'PublishMacOS_FrameWork')
Foreach ($PubProfile in $PublishProfiles) {
    $joinPathSplat1 = @{
        Path                = Split-Path $Project1Path
        ChildPath           = 'Properties'
        AdditionalChildPath = @('PublishProfiles', "$PubProfile.pubxml")
        Resolve             = $true
    }
    $ProfilePath = Join-Path @joinPathSplat1
    Write-Host "Cloud-ShareSync GUI $PubProfile" -ForegroundColor Green
    dotnet publish "/p:PublishProfile=$ProfilePath"

    $joinPathSplat2 = @{
        Path                = Split-Path $Project2Path
        ChildPath           = 'Properties'
        AdditionalChildPath = @('PublishProfiles', "$PubProfile.pubxml")
        Resolve             = $true
    }
    $ProfilePath = Join-Path @joinPathSplat2
    Write-Host "Cloud-ShareSync Commandline $PubProfile" -ForegroundColor Green
    dotnet publish "/p:PublishProfile=$ProfilePath"
}

$PublishProfileOutputPath = Join-Path -Path $SOURCEPATH -ChildPath 'publish'
$PublishProfileOSDirectories = @(
    (Join-Path -Path $PublishProfileOutputPath -ChildPath 'framework' -AdditionalChildPath 'windows'),
    (Join-Path -Path $PublishProfileOutputPath -ChildPath 'selfcontained' -AdditionalChildPath 'windows'),
    (Join-Path -Path $PublishProfileOutputPath -ChildPath 'framework' -AdditionalChildPath 'linux'),
    (Join-Path -Path $PublishProfileOutputPath -ChildPath 'selfcontained' -AdditionalChildPath 'linux'),
    (Join-Path -Path $PublishProfileOutputPath -ChildPath 'framework' -AdditionalChildPath 'macos'),
    (Join-Path -Path $PublishProfileOutputPath -ChildPath 'selfcontained' -AdditionalChildPath 'macos')
)

$AppSettings = Join-Path -Path $SOURCEPATH -ChildPath 'appsettings.json'
$LicensePath = Join-Path -Path $SOURCEPATH -ChildPath 'LICENSE'
$READMEPath = Join-Path -Path $SOURCEPATH -ChildPath 'README.md'
$CopyParam = @{
    Verbose = $true
    Force   = $true
}

Foreach ($OsDir in $PublishProfileOSDirectories) {
    $ASOutput = Join-Path -Path $OsDir -ChildPath 'appsettings.json'
    Copy-Item -Path $AppSettings -Destination $ASOutput @CopyParam

    $LicenseOutput = Join-Path -Path $OsDir -ChildPath 'LICENSE'
    Copy-Item -Path $LicensePath -Destination $LicenseOutput @CopyParam

    $READMEOutput = Join-Path -Path $OsDir -ChildPath 'README.md'
    Copy-Item -Path $READMEPath -Destination $READMEOutput @CopyParam
}
if ($PublishProfileOutputPath -ine $PublishProfileOutputPath ) {
    Move-Item -Path $PublishProfileOutputPath -Destination $PUBLISHPATH @CopyParam
}
