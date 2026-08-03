using System;
using System.IO;
using System.Security.Cryptography;

namespace RasTweaksCS
{
    /// <summary>
    /// AES-256-CBC decryption for the embedded tweak scripts. The build embeds
    /// ciphertext (see build/encrypt-tweaks.ps1) so the exe never carries readable
    /// .bat/.ps1 source; this decrypts them back to real files at extraction time.
    ///
    /// The key below is intentionally the same bytes as the encryptor's. It ships
    /// inside the exe - this deters casual inspection, it is not real cryptographic
    /// secrecy, since anyone who can run the app can also recover this key.
    /// </summary>
    internal static class TweakCrypto
    {
        private static readonly byte[] Key =
        {
            0x8F, 0x2A, 0x14, 0xC7, 0x53, 0xE9, 0x1B, 0x6D,
            0x40, 0xA2, 0xFB, 0x37, 0x9C, 0x08, 0xD5, 0x71,
            0x2E, 0xB6, 0x4F, 0x83, 0x1A, 0xCD, 0x60, 0x95,
            0x0B, 0xE4, 0x77, 0x38, 0xA9, 0x52, 0xDC, 0x11
        };

        private const int IvLength = 16;

        public static byte[] Decrypt(byte[] encrypted)
        {
            if (encrypted.Length < IvLength)
            {
                throw new ArgumentException("Encrypted payload is too short to contain an IV.");
            }

            var iv = new byte[IvLength];
            Array.Copy(encrypted, 0, iv, 0, IvLength);

            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            using var input = new MemoryStream(encrypted, IvLength, encrypted.Length - IvLength);
            using var crypto = new CryptoStream(input, decryptor, CryptoStreamMode.Read);
            using var output = new MemoryStream();
            crypto.CopyTo(output);
            return output.ToArray();
        }
    }
}
