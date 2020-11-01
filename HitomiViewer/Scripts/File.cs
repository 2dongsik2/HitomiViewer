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
    public class CF //CustomFunctions
    {
        public partial class File
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
            public static string SaftyFileName(string s) => s
                .Replace("|", "｜").Replace("?", "？")
                .Replace(":", "：").Replace("<", "＜")
                .Replace(">", "＞").Replace("*", "⋆")
                .Replace("\"", "”").Replace("/", "／")
                .Replace("\\", "／");
            public static string GetDownloadTitle(string t)
            {
                if (Global.config.encrypt_title.Get<bool>())
                {
                    return Base64.Encrypt(t);
                }
                else if (Global.config.random_title.Get<bool>())
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
            public static string SizeStr(object val) => SizeStr((double)val);
            public static string SizeStr(double val)
            {
                const int KB = 1024;
                const int MB = 1024 * 1024;
                const int GB = 1024 * 1024 * 1024;
                if (val > KB)
                    return Math.Round(val / KB, 2) + "KB";
                if (val > MB)
                    return Math.Round(val / MB, 2) + "MB";
                if (val > GB)
                    return Math.Round(val / GB, 2) + "GB";
                return val + "B";
            }
        }
    }
}
