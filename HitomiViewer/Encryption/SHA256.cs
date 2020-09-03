using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HitomiViewer.Encryption
{
    class SHA256
    {
        private static SHA256Managed sha256 = new SHA256Managed();
        public static string Hash(string data)
        {
            byte[] digest = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(digest).Replace("-","").ToLower();
        }
        public static string Hash(byte[] data)
        {
            byte[] digest = sha256.ComputeHash(data);
            return BitConverter.ToString(digest).Replace("-","").ToLower();
        }
        public static byte[] HashArray(string data)
        {
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
        }
        public static byte[] HashArray(byte[] data)
        {
            return sha256.ComputeHash(data);
        }
    }
}
