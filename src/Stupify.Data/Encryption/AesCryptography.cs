using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Stupify.Data.Encryption
{
    public class AesCryptography
    {
        private readonly byte[] _passwordBytes;
        private readonly byte[] _saltBytes;

        public AesCryptography(string password)
        {
            _saltBytes = Encoding.UTF8.GetBytes("StupifySalt");
            _passwordBytes = Encoding.UTF8.GetBytes(password);
            _passwordBytes = SHA256.Create().ComputeHash(_passwordBytes);
        }

        public string Encrypt(string stringToBeEncrypted)
        {
            var bytesToBeEncrypted = Encoding.UTF8.GetBytes(stringToBeEncrypted);
            var bytesEncrypted = Encrypt(bytesToBeEncrypted);
            return Convert.ToBase64String(bytesEncrypted);
        }

        public string Decrypt(string stringToBeDecrypted)
        {
            var bytesToBeDecrypted = Convert.FromBase64String(stringToBeDecrypted);
            var plainTextBytes = Decrypt(bytesToBeDecrypted);
            return Encoding.UTF8.GetString(plainTextBytes);
        }

        private byte[] Encrypt(byte[] bytesToBeEncrypted)
        {
            byte[] encryptedBytes;

            using (var ms = new MemoryStream())
            {
                using (var aes = new RijndaelManaged())
                {
                    aes.KeySize = 256;
                    aes.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(_passwordBytes, _saltBytes, 1000);
                    aes.Key = key.GetBytes(aes.KeySize / 8);
                    aes.IV = key.GetBytes(aes.BlockSize / 8);

                    aes.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }

            return encryptedBytes;
        }

        private byte[] Decrypt(byte[] bytesToBeDecrypted)
        {
            byte[] decryptedBytes;

            using (var ms = new MemoryStream())
            {
                using (var aes = new RijndaelManaged())
                {
                    aes.KeySize = 256;
                    aes.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(_passwordBytes, _saltBytes, 1000);
                    aes.Key = key.GetBytes(aes.KeySize / 8);
                    aes.IV = key.GetBytes(aes.BlockSize / 8);

                    aes.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }
                    decryptedBytes = ms.ToArray();
                }
            }

            return decryptedBytes;
        }
    }
}
