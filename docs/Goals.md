# Cloud-ShareSync Primary Goals:
## Description:
My number one goal for this project is to write an application that will allow me to backup my home media to
 a cloud storage bucket (from a variety of cloud storage providers [BackBlaze Only at first.]).

<br>

## Cloud-ShareSync Feature List:
1. Upload files successfully to Cloud Storage*.
2. Download files successfully from Cloud Storage*.
3. Offer pre-upload compression**.
4. Offer post-download de-compression**.
5. Offer pre-upload file encryption***.
6. Offer post-download file decryption***.
7. Application uses an internal sqllite database by default.
8. Application can optionally use an external postgres database instead.

<br><br>

### Cloud Storage*:
Allows for buckets from BackBlaze B2, AWS S3, Google Cloud Storage, Azure Blob Storage

<br>

### Encryption/Decryption**:
File encryption/decryption relies on the ChaCha20Poly1305 AEAD cryptographic cypher.
 This cypher is not available on all platforms yet. If you select encryption and the ChaCha20Poly1305 cypher is not
 supported on your device then the application will abort. Validate ChaCha20Poly1305 support manually before deploying
 in an automated environment.
Files can also (optionally) be password protected during the compression process, this password protection provides
 another form of (AES based) encryption.

<br>

### Compression/De-compression***:
Compression/De-Compression relies on the user providing the 7zip executable (and dll on windows).
Files can, optionally, be password protected during the compression process.

<br><br>

## Initial Application Compromises:
1. Cloud providers for alpha app are currently limited to support for Backblaze B2.
2. Skipping internal/built in compression/decompression. Relying on external 7zip dependency.
	a. The end user must provide the 7zip executable as this project is MIT licenced.
	- I'll dig into the licensing differences and figure out how to properly integrate LGPL code later.
3. App only uses sqllite for now. Will add postgres integration later.
