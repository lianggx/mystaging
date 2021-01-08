using System;
using System.Security.Cryptography;
using System.Text;

namespace IdentityHost.Helpers
{
    public class SecurityHelper
    {
        public static string GetMD5String(string content)
        {
            byte[] result = Encoding.UTF8.GetBytes(content);
            using (MD5 md5 = MD5.Create())
            {
                var md5result = BitConverter.ToString(md5.ComputeHash(result)).Replace("-", "");
                return md5result.ToLower();
            };
        }

        public static string GetSHA1SignString(string content)
        {
            SHA1 sha1 = SHA1.Create();
            byte[] result = Encoding.UTF8.GetBytes(content);
            byte[] sha1result = sha1.ComputeHash(result);
            string sha1str = BitConverter.ToString(sha1result).Replace("-", "").ToLower();
            return sha1str;
        }


        public static string GetSHA256SignString(string content)
        {
            SHA256 sha256 = SHA256.Create();
            byte[] result = Encoding.UTF8.GetBytes(content);
            byte[] sha1result = sha256.ComputeHash(result);
            string sha1str = BitConverter.ToString(sha1result).Replace("-", "").ToLower();
            return sha1str;
        }

        public static string GetStrBySHA1ToBase64(string value)
        {
            byte[] source = Encoding.UTF8.GetBytes(value);
            using (SHA1 sha1 = SHA1.Create())
            {
                var crypto = sha1.ComputeHash(source);
                var str = Convert.ToBase64String(crypto, Base64FormattingOptions.None);
                return str;
            }
        }
    }
}