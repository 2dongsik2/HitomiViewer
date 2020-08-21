using ExtensionMethods;
using HitomiViewer.Encryption;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitomiViewer.Scripts
{
    partial class File2
    {
        public static double GetFolderByte(string dir)
        {
            DirectoryInfo info = new DirectoryInfo(dir);
            double FolderByte = info.EnumerateFiles().Sum(f => f.Length);
            return FolderByte;
        }
        public static double GetFilesByte(string[] files)
        {
            IEnumerable<FileInfo> fileinfos = files.Select(file => new FileInfo(file));
            double FolderByte = fileinfos.Sum(f => f.Length);
            return FolderByte;
        }
        public static double GetSizePerPage(string dir)
        {
            DirectoryInfo info = new DirectoryInfo(dir);
            double FolderByte = info.EnumerateFiles().Sum(f => f.Length);
            double SizePerPage = FolderByte / info.GetFiles().Length;
            return SizePerPage;
        }
        public static double GetSizePerPage(string dir, string[] allow)
        {
            DirectoryInfo info = new DirectoryInfo(dir);
            IEnumerable<FileInfo> files = info.EnumerateFiles().Where(file => allow.Any(file.FullName.ToLower().EndsWith));
            double FolderByte = files.Sum(f => f.Length);
            double SizePerPage = FolderByte / files.Count();
            return SizePerPage;
        }
        public static string[] GetImages(string dir)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".lock" };
            return Directory.GetFiles(dir).Where(file => allowedExtensions.Any(file.ToLower().EndsWith)).ToArray().ESort();
        }
        public static string SaftyFileName(string s) => s.Replace("|", "｜").Replace("?", "？");
        public static string GetDownloadTitle(string t)
        {
            if (Global.EncryptTitle)
            {
                return Base64.Encrypt(t);
            }
            else if (Global.RandomTitle)
            {
                return Random2.RandomString(Global.RandomStringLength);
            }
            else return t;
        }
        public static string GetDirectory(string dir)
        {
            return dir;
        }
        public static string[] GetDirectories(string root = "", params string[] dirs)
        {
            List<string> items = new List<string>();
            foreach (string dir in dirs)
            {
                if (Directory.Exists(dir))
                    items = items.Concat(Directory.GetDirectories(root + dir)).ToList();
            }
            return items.ToArray();
        }
    }
}
