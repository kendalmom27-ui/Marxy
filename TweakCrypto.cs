using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace RasTweaksCS
{
    /// <summary>
    /// AES-256-CBC decryption for the embedded tweak scripts. The build embeds
    /// ciphertext (see build/encrypt-tweaks.ps1) so the exe never carries readable
    /// .bat/.ps1 source; this decrypts them back to real files at extraction time.
    ///
    /// The key is DERIVED at run time (PBKDF2), never stored as a literal byte
    /// array, and the passphrase is assembled from fragments rather than a single
    /// string constant - so static inspection of the exe (search, decompile) turns
    /// up no findable key or password. This is a deliberately high bar against
    /// static analysis. It is NOT unbreakable: a debugger can still read the derived
    /// key at the moment it is used, because the app must reconstruct the real key
    /// to decrypt. That last step is unavoidable for any program that ships.
    /// </summary>
    internal static class TweakCrypto
    {
        private const int IvLength = 16;
        private const int Iterations = 100000;

        private static readonly byte[] Salt =
        {
            0x52, 0x41, 0x53, 0x58, 0x73, 0x61, 0x6C, 0x74,
            0x76, 0x31, 0x9A, 0x3C, 0xE7, 0x08, 0xBD, 0x44
        };

        // Assembled from fragments + one computed segment so no complete passphrase
        // string appears anywhere in the compiled binary for a search to land on.
        private static string BuildPassphrase()
        {
            var sb = new StringBuilder();
            sb.Append("rasx");
            sb.Append((char)0x3A).Append((char)0x3A);          // "::"
            sb.Append("twe").Append("ak");
            sb.Append((char)0x3A).Append((char)0x3A);
            sb.Append("va").Append("ult");
            sb.Append((char)0x3A).Append((char)0x3A);
            sb.Append("9F2C").Append("7B14");
            sb.Append((char)0x3A).Append((char)0x3A);
            sb.Append("do").Append('-').Append("not").Append('-').Append("share");
            return sb.ToString();
        }

        private static byte[] DeriveKey()
        {
            using var kdf = new Rfc2898DeriveBytes(BuildPassphrase(), Salt, Iterations, HashAlgorithmName.SHA256);
            return kdf.GetBytes(32);
        }

        public static byte[] Decrypt(byte[] encrypted)
        {
            if (encrypted.Length < IvLength)
            {
                throw new ArgumentException("Encrypted payload is too short to contain an IV.");
            }

            var iv = new byte[IvLength];
            Array.Copy(encrypted, 0, iv, 0, IvLength);

            using var aes = Aes.Create();
            aes.Key = DeriveKey();
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
