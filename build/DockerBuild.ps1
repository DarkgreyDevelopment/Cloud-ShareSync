$buildDir = (Get-Item $PSCommandPath -ErrorAction Stop).Directory.Parent

$SolutionRoot = Resolve-Path $buildDir -ErrorAction Stop
$SimpleBackupDockerfile = Resolve-Path (Join-Path $SolutionRoot 'src' 'SimpleBackup' 'Dockerfile') -ErrorAction Stop
$SimpleRestoreDockerfile = Resolve-Path (Join-Path $SolutionRoot 'src' 'SimpleRestore' 'Dockerfile') -ErrorAction Stop

try {
	& 'docker' image build --no-cache -t cloud-sharesync-simplebackup:latest -f $SimpleBackupDockerfile $SolutionRoot
	if ($LASTEXITCODE -ne 0) { throw }

    Write-Host "SimpleBackup docker create command example:"
    Write-Host (
        "docker create --name=simplebackup --restart=no " +
        "-e 'CLOUDSHARESYNC_CONFIGPATH=/config/appsettings.json' " +
        "-v /App/Config:/config -v /mnt/FileShare:/backup " +
        "-v /App/Working:/working " +
        "-v /App/Log:/log " +
        "-v /App/Database:/database " + 
        "cloud-sharesync-simplebackup:latest`n"
    )

	& 'docker' image build --no-cache -t cloud-sharesync-simplerestore:latest -f $SimpleRestoreDockerfile $SolutionRoot
	if ($LASTEXITCODE -ne 0) { throw }

} catch {
	Write-Error -Message "Failed to create docker image. LastExitCode: $LASTEXITCODE"
}
