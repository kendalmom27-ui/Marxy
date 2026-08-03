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

$key = [byte[]]@(
    0x8F, 0x2A, 0x14, 0xC7, 0x53, 0xE9, 0x1B, 0x6D,
    0x40, 0xA2, 0xFB, 0x37, 0x9C, 0x08, 0xD5, 0x71,
    0x2E, 0xB6, 0x4F, 0x83, 0x1A, 0xCD, 0x60, 0x95,
    0x0B, 0xE4, 0x77, 0x38, 0xA9, 0x52, 0xDC, 0x11
)

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
