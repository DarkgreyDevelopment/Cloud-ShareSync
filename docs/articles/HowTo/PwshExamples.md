#
```ps1
Add-Type -LiteralPath .\Cloud_ShareSync.Core.dll
$Config = [Cloud_ShareSync.Core.Configuration.Config]::GetConfiguration('C:\app\Configuration\appsettings.json')
$Logger = [Cloud_ShareSync.Core.Logging.TelemetryLogger]::New($null,$Config.Log4Net)
[Cloud_ShareSync.Core.CloudProvider.BackBlaze.BackBlazeB2]::Initialize($Config.BackBlaze,$Logger)
[Cloud_ShareSync.Core.CloudProvider.BackBlaze.BackBlazeB2]::UploadFile("C:\Temp\Temp\Testing\Test2\NestedTest2\TestNested2\testFile.xyz").Result
```


```ps1
Add-Type -LiteralPath '.\Cloud-ShareSync.Core.dll'
$Config = [Cloud_ShareSync.Core.Configuration.Config]::GetConfiguration('C:\app\Configuration\appsettings.json')
$Logger = [Cloud_ShareSync.Core.Logging.TelemetryLogger]::New($null,$Config.Log4Net)
[Cloud_ShareSync.Core.CloudProvider.BackBlaze.BackBlazeB2]::Initialize($Config.BackBlaze,$Logger)
[Cloud_ShareSync.Core.CloudProvider.BackBlaze.BackBlazeB2]::ListFileVersions().Result
```

```ps1
Add-Type -LiteralPath '.\Cloud-ShareSync.Core.dll'
$Config = [Cloud_ShareSync.Core.Configuration.Config]::GetConfiguration('C:\app\Configuration\appsettings.json')
$Logger = [Cloud_ShareSync.Core.Logging.TelemetryLogger]::New($null,$Config.Log4Net)
[Cloud_ShareSync.Core.CloudProvider.BackBlaze.BackBlazeB2]::Initialize($Config.BackBlaze,$Logger)
$Response = [Cloud_ShareSync.Core.CloudProvider.BackBlaze.BackBlazeB2]::ListFileVersions().Result
$Download = [Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types.DownloadB2File]::New(
	[System.IO.FileInfo]::New(".\100mb.test"),
	$Response[0].fileId
)
$DLResponse = [Cloud_ShareSync.Core.CloudProvider.BackBlaze.BackBlazeB2]::DownloadFile($Download)
```

# Cloud_ShareSync.Cryptography Functional Examples

```ps1
	Add-Type -LiteralPath .\Cloud-ShareSync.Core.dll

	$FolderPath = (Get-Location).path
	$FileName   = 'Cloud-ShareSync.pdb'

	$FileToEncrypt     = [System.IO.FileInfo]::New("$(Join-Path $FolderPath $FileName)")
	$FileToDecryptTo   = [System.IO.FileInfo]::New("$(Join-Path $FolderPath $FileName + '.new')")
	$FileToDecryptTo2  = [System.IO.FileInfo]::New("$(Join-Path $FolderPath $FileName + '.new2')")
	$EncryptedFile     = [System.IO.FileInfo]::New("$(Join-Path $FolderPath $FileName + '.enc')")
	$EncryptedFile2    = [System.IO.FileInfo]::New("$(Join-Path $FolderPath $FileName + '.enc2')")
	$EncryptionKeyFile = [System.IO.FileInfo]::New("$(Join-Path $FolderPath $FileName + '.enc.key')")

	Write-Host "Getting the SHA512 FileHash of '$($FileToEncrypt.FullName)'."
	$Sha512Hash = [Cloud_ShareSync.Cryptography.FileHash.SHA512Hash]::new()
	$Hash = $Sha512Hash::GetSHA512FileHash($FileToEncrypt.FullName)
	Write-Host "Sha512 FileHash: $Hash" -ForeGroundColor Green

	$ManagedChaCha20Poly1305 = [Cloud_ShareSync.Cryptography.FileEncryption.ManagedChaCha20Poly1305]::new()

	if ($ManagedChaCha20Poly1305::PlatformSupported) {
		[byte[]]$Key = [byte[]]::new(32)
		[System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($Key) # Generate a random key.

		Write-Host "Encrypting File '$($FileToEncrypt.FullName)' and outputting KeyFile to '$($EncryptionKeyFile.FullName)'."
		$ManagedChaCha20Poly1305::Encrypt(
			$Key,
			$FileToEncrypt,
			$EncryptedFile,
			$EncryptionKeyFile
		).Result | Out-Null
		Write-Host "Encrypted File '$($FileToEncrypt.FullName)'" -ForeGroundColor Green

		Write-Host "Decrypting File '$($FileToEncrypt.FullName)' using KeyFile '$($EncryptionKeyFile.FullName)'."
		$ManagedChaCha20Poly1305::DecryptFromKeyFile(
			$EncryptionKeyFile,
			$EncryptedFile,
			$FileToDecryptTo
		).Result | Out-Null
		Write-Host "Decrypted File '$($FileToEncrypt.FullName)'. PlaintextFile: '$($FileToDecryptTo.FullName)'" -ForeGroundColor Green
		$Hash2 = $Sha512Hash::GetSHA512FileHash($FileToDecryptTo.FullName)
		Write-Host "Original Sha512 FileHash:  $Hash"
		Write-Host "Decrypted Sha512 FileHash: $Hash2"


		Write-Host "Encrypting File '$($FileToEncrypt.FullName)' and NOT outputting KeyFile."
		$DecryptionData = $ManagedChaCha20Poly1305::Encrypt(
			$Key,
			$FileToEncrypt,
			$EncryptedFile2
		).Result
		Write-Host "Encrypted File '$($FileToEncrypt.FullName)'" -ForeGroundColor Green

		Write-Host "Decrypting File '$($FileToEncrypt.FullName)' using DecryptionData: $DecryptionData."
		$ManagedChaCha20Poly1305::Decrypt(
			$DecryptionData,
			$EncryptedFile2,
			$FileToDecryptTo2
		).Result | Out-Null
		Write-Host "Decrypted File '$($EncryptedFile2.FullName)'. PlaintextFile: '$($FileToDecryptTo2.FullName)'" -ForeGroundColor Green
		$Hash3 = $Sha512Hash::GetSHA512FileHash($FileToDecryptTo2.FullName)

		Write-Host "Original Sha512 FileHash:  $Hash"
		Write-Host "Decrypted Sha512 FileHash: $Hash3"

		$Sha1Hash = [Cloud_ShareSync.Cryptography.FileHash.SHA1Hash]::new()

		Write-Host "Getting SHA1 FileHashes"

		$Sha1_0 = $Sha1Hash::GetSHA1FileHash($FileToEncrypt.FullName)

	} else {
		Write-Host 'ChaCha20Poly1305 is not supported on this platform.'
	}
```
