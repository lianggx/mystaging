using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace IdentityHost.Helpers
{
    public class AesFactory
    {
        /// <summary>
        ///  加密字符串
        /// </summary>
        /// <param name="plainText">待加密文本</param>
        /// <param name="key">密钥，长度为32</param>
        /// <param name="iv">向量，长度为16</param>
        /// <returns></returns>
        public static byte[] EncryptStringToBytes(string plainText, string key, string iv)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException(nameof(plainText));
            if (string.IsNullOrEmpty(key) || key.Length != 32)
                throw new ArgumentOutOfRangeException(nameof(key));
            if (string.IsNullOrEmpty(iv) || iv.Length != 16)
                throw new ArgumentOutOfRangeException(nameof(iv));

            byte[] encrypted;
            using (RijndaelManaged rijndael = new RijndaelManaged())
            {
                rijndael.Key = Encoding.UTF8.GetBytes(key);
                rijndael.IV = Encoding.UTF8.GetBytes(iv);

                ICryptoTransform encryptor = rijndael.CreateEncryptor(rijndael.Key, rijndael.IV);
                using MemoryStream msEncrypt = new MemoryStream();
                using CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
                using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(plainText);
                }
                encrypted = msEncrypt.ToArray();
            }

            return encrypted;
        }

        /// <summary>
        ///  解密字符串
        /// </summary>
        /// <param name="cipherData">待解密数据</param>
        /// <param name="key">密钥，长度为32</param>
        /// <param name="iv">向量，长度为16</param>
        /// <returns></returns>
        public static string DecryptStringFromBytes(byte[] cipherData, string key, string iv)
        {
            if (cipherData == null)
                throw new ArgumentNullException(nameof(cipherData));
            if (string.IsNullOrEmpty(key) || key.Length != 32)
                throw new ArgumentOutOfRangeException(nameof(key));
            if (string.IsNullOrEmpty(iv) || iv.Length != 16)
                throw new ArgumentOutOfRangeException(nameof(iv));

            string plainText;
            using (RijndaelManaged rijndael = new RijndaelManaged())
            {
                rijndael.Key = Encoding.UTF8.GetBytes(key);
                rijndael.IV = Encoding.UTF8.GetBytes(iv);

                ICryptoTransform decryptor = rijndael.CreateDecryptor(rijndael.Key, rijndael.IV);
                using MemoryStream msDecrypt = new MemoryStream(cipherData);
                using CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using StreamReader srDecrypt = new StreamReader(csDecrypt);
                plainText = srDecrypt.ReadToEnd();
            }
            return plainText;
        }
    }
}
