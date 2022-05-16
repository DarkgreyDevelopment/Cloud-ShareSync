[CmdletBinding()]
param (
    [Parameter()]
    [switch]$Backup,

    [Parameter()]
    [switch]$Restore,

    [Parameter()]
    [switch]$NoBuild
)
$CreateBothImages  = ($Backup -eq $false) -and ($Restore -eq $false)

$SolutionSplat = @{
    Path        = (Get-Item $PSCommandPath -ErrorAction Stop).Directory.Parent
    ErrorAction = 'Stop'
}
$SolutionRoot      = Resolve-Path @SolutionSplat
$DockerPath        = Join-Path -Path $SolutionRoot -ChildPath 'docker'
$DockerFile        = Join-Path -Path $DockerPath -ChildPath 'Dockerfile'
$DockerFileContent = Get-Content -Path $DockerFile -Raw

try {
    if ($Backup -or $CreateBothImages) {
        Write-Host "Created dockerfile for cloud-sharesync.backup"
        $BackupDockerPath = Join-Path -Path $DockerPath -ChildPath 'BackupDockerfile'
        $BackupDockerSplat = @{
            Path        = $BackupDockerPath
            Value       = $DockerFileContent.Replace('{Action}','backup')
            ErrorAction = 'Stop'
            Encoding    = 'utf8'

        }
        Set-Content @BackupDockerSplat | Out-Null

        if ($NoBuild -eq $false) {
            Write-Host "Building docker image for cloud-sharesync.backup"
            $Output = & 'docker' image build --no-cache -t cloud-sharesync.backup:latest -f $BackupDockerPath $SolutionRoot
            if ($LASTEXITCODE -ne 0) {
                throw (
                    "An error has occured building the cloud-sharesync.backup image.`n" +
                    "Docker Output:`n" +
                    $Output
                )
            }

            Write-Host 'Example Docker Create Command:'
            Write-Host (
                'docker create --name=backup --restart=no ' +
                "-e 'CLOUDSHARESYNC_CONFIGPATH=/config/appsettings.json' " +
                '-v /App/Config:/config -v /mnt/FileShare:/backup ' +
                '-v /App/Working:/working ' +
                '-v /App/Log:/log ' +
                '-v /App/Database:/database ' +
                "cloud-sharesync.backup:latest`n"
            )
        }
    }

    if ($Restore -or $CreateBothImages) {
        Write-Host "Created dockerfile for cloud-sharesync.restore"
        $RestoreDockerPath = Join-Path -Path $DockerPath -ChildPath 'RestoreDockerfile'
        $RestoreDockerSplat = @{
            Path        = $RestoreDockerPath
            Value       = $DockerFileContent.Replace('{Action}','restore')
            ErrorAction = 'Stop'
            Encoding    = 'utf8'

        }
        Set-Content @RestoreDockerSplat | Out-Null

        if ($NoBuild -eq $false) {
            Write-Host "Building docker image for cloud-sharesync.restore"
            $Output = & 'docker' image build --no-cache -t cloud-sharesync.restore:latest -f $RestoreDockerPath $SolutionRoot
            if ($LASTEXITCODE -ne 0) {
                throw (
                    "An error has occured building the cloud-sharesync.restore image.`n" +
                    "Docker Output:`n" +
                    $Output
                )
            }

            Write-Host 'Example Docker Create Command:'
            Write-Host (
                'docker create --name=restore --restart=no ' +
                "-e 'CLOUDSHARESYNC_CONFIGPATH=/config/appsettings.json' " +
                '-v /App/Config:/config -v /mnt/FileShare:/restore ' +
                '-v /App/Working:/working ' +
                '-v /App/Log:/log ' +
                '-v /App/Database:/database ' +
                "cloud-sharesync.restore:latest`n"
            )
        }
    }

} catch {
    Write-Error -Message "Failed to create docker image. LastExitCode: $LASTEXITCODE"
}
