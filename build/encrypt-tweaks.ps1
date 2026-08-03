# Build-time encryptor: AES-256-CBC encrypts every file in the Tweaks folder
# into <name>.enc blobs (16-byte random IV prepended to the ciphertext), so
# the shipped exe embeds ciphertext instead of readable .bat/.ps1 source.
# The matching key/decrypt lives in TweakCrypto.cs - keep the key bytes here
# and there identical.
#
# NOTE (stated plainly, same as the user was told): the decryption key ships
# inside the exe, so this raises the effort bar against casual inspection but
# is not unbreakable. The scripts are still briefly written as plaintext to
# %LOCALAPPDATA% at run time because cmd/powershell need real files.
param(
    [Parameter(Mandatory = $true)][string]$Source,
    [Parameter(Mandatory = $true)][string]$Dest
)

$ErrorActionPreference = 'Stop'

# Key is DERIVED (PBKDF2-HMAC-SHA256), never stored as literal bytes, so the
# shipped exe contains no findable key array. This .ps1 lives only in the
# private repo (it is not embedded in the exe), so the passphrase being visible
# here is fine - what matters is that TweakCrypto.cs reconstructs the identical
# key without any greppable literal. Both sides MUST use identical passphrase,
# salt, iteration count and hash, or nothing decrypts.
$passphrase = 'rasx::tweak::vault::9F2C7B14::do-not-share'
$salt = [byte[]]@(0x52, 0x41, 0x53, 0x58, 0x73, 0x61, 0x6C, 0x74, 0x76, 0x31, 0x9A, 0x3C, 0xE7, 0x08, 0xBD, 0x44)
$iterations = 100000

$kdf = New-Object System.Security.Cryptography.Rfc2898DeriveBytes($passphrase, $salt, $iterations, [System.Security.Cryptography.HashAlgorithmName]::SHA256)
$key = $kdf.GetBytes(32)
$kdf.Dispose()

if (Test-Path $Dest) { Remove-Item $Dest -Recurse -Force }
New-Item -ItemType Directory -Path $Dest -Force | Out-Null

$count = 0
Get-ChildItem -Path $Source -File | ForEach-Object {
    $plain = [System.IO.File]::ReadAllBytes($_.FullName)

    $aes = [System.Security.Cryptography.Aes]::Create()
    $aes.Key = $key
    $aes.Mode = [System.Security.Cryptography.CipherMode]::CBC
    $aes.Padding = [System.Security.Cryptography.PaddingMode]::PKCS7
    $aes.GenerateIV()

    $encryptor = $aes.CreateEncryptor()
    $cipher = $encryptor.TransformFinalBlock($plain, 0, $plain.Length)

    $out = New-Object byte[] ($aes.IV.Length + $cipher.Length)
    [Array]::Copy($aes.IV, 0, $out, 0, $aes.IV.Length)
    [Array]::Copy($cipher, 0, $out, $aes.IV.Length, $cipher.Length)

    $encryptor.Dispose()
    $aes.Dispose()

    # Keep the original filename (encrypted content, same name) so the embedded
    # logical name maps 1:1 back to the real script name at extraction time.
    [System.IO.File]::WriteAllBytes((Join-Path $Dest $_.Name), $out)
    $count++
}

Write-Host "Encrypted $count tweak file(s) into $Dest"
