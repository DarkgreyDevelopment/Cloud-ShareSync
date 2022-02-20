[CmdletBinding()]
param (
	[Parameter(Mandatory)]
	[string]$SOURCEPATH = "$Env:CLOUDSHARESYNC_SOURCEPATH",

	[Parameter(Mandatory)]
	[string]$BUILDPATH = "$Env:CLOUDSHARESYNC_BUILDPATH",

	[Parameter(Mandatory)]
	[string]$PUBLISHPATH = "$Env:CLOUDSHARESYNC_PUBLISHPATH"
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

# Ensure BUILDPATH/PUBLISHPATH are empty.
if ((Get-ChildItem -Path $BUILDPATH -ErrorAction SilentlyContinue).Count -gt 0) {
	throw "BUILDPATH '$BUILDPATH' has existing files."
}
if ((Get-ChildItem -Path $PUBLISHPATH -ErrorAction SilentlyContinue).Count -gt 0) {
	throw "PUBLISHPATH '$PUBLISHPATH' has existing files."
}

# Copy code to build path.
Write-Host "Copying source files to '$BUILDPATH'." -ForegroundColor Green
Copy-Item -Path "$SOURCEPATH/*" -Destination "$BUILDPATH/" -Recurse -Verbose:$Verbose -ErrorAction Stop

# Restore, Build, Publish SimpleBackup.
Write-Host 'Building and publishing SimpleBackup.' -ForegroundColor Green
dotnet restore "$BUILDPATH/src/SimpleBackup/Cloud-ShareSync.SimpleBackup.csproj"
dotnet publish "$BUILDPATH/src/SimpleBackup/Cloud-ShareSync.SimpleBackup.csproj" -c Release -o "$PUBLISHPATH"

$AppSettings = Resolve-Path -Path "$BUILDPATH/src/Core/Configuration/appsettings.json"
$DefaultConfigPath = Join-Path -Path $PUBLISHPATH -ChildPath 'Configuration'
Write-Host "Copying appsettings.json to default config path '$DefaultConfigPath'." -ForegroundColor Green
if ((Test-Path -Path $DefaultConfigPath) -eq $false) {
	New-Item -Path $DefaultConfigPath -ItemType Directory -ErrorAction Stop -Verbose:$Verbose 1> $null
}
Copy-Item -Path $AppSettings -Destination $DefaultConfigPath -ErrorAction Stop -Verbose:$Verbose
