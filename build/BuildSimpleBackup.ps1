[CmdletBinding()]
param (
	[Parameter(Mandatory)]
	[string]$SOURCEPATH = "$Env:CLOUDSHARESYNC_SOURCEPATH",

	[Parameter(Mandatory)]
	[string]$PUBLISHPATH = "$Env:CLOUDSHARESYNC_PUBLISHPATH"
)

$Verbose = $PSBoundParameters.Keys -icontains 'Verbose' ? $PSBoundParameters['Verbose'] : $false

# Restore, Build, Publish SimpleBackup.
Write-Host 'Building and publishing SimpleBackup.' -ForegroundColor Green
dotnet restore "$SOURCEPATH/src/SimpleBackup/Cloud-ShareSync.SimpleBackup.csproj"
dotnet publish "$SOURCEPATH/src/SimpleBackup/Cloud-ShareSync.SimpleBackup.csproj" -c Release -o "$PUBLISHPATH"

$AppSettings = Resolve-Path -Path "$SOURCEPATH/appsettings.json"
$DefaultConfigPath = Join-Path -Path $PUBLISHPATH -ChildPath 'Configuration'
Write-Host "Copying appsettings.json to default config path '$DefaultConfigPath'." -ForegroundColor Green
if ((Test-Path -Path $DefaultConfigPath) -eq $false) {
	New-Item -Path $DefaultConfigPath -ItemType Directory -ErrorAction Stop -Verbose:$Verbose 1> $null
}
Copy-Item -Path $AppSettings -Destination $DefaultConfigPath -ErrorAction Stop -Verbose:$Verbose
