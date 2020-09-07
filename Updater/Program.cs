using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Updater
{
    sealed class Program
    {
        public static readonly string rootDir = AppDomain.CurrentDomain.BaseDirectory;
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(ResolveAssembly);
            Console.WriteLine("업데이트 후 자동으로 실행됩니다.");
            Thread.Sleep(1000 * 2);
            if (File.Exists(Path.Combine(rootDir, "HitomiViewer.exe")))
                Update(args);
            else
                Download(args);
            while (true) { }
        }
        public static async void Update(string[] args)
        {
            Console.WriteLine("Update");
            string file;
            if (args.Length <= 0)
            {
                FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Path.Combine(rootDir, "HitomiViewer.exe"));
                Version filev = Version.Parse(versionInfo.FileVersion);
                string s = await Load("https://api.github.com/repos/rmagur1203/HitomiViewer/releases");
                JArray jarray = JArray.Parse(s);
                Console.WriteLine("현재 버전: {0}\n", filev);

                Console.WriteLine("0: 최신 버전 (프리 릴리즈 제외)");
                for (int i = 0; i < jarray.Count; i++)
                {
                    Console.Write("{0}: {1}", i + 1, jarray[i]["tag_name"].ToString());
                    if (bool.Parse(jarray[i]["prerelease"].ToString()) == true)
                        Console.Write(" (프리 릴리즈)");
                    Console.Write("\n");
                }
                Console.WriteLine();
                Console.Write("선택: ");
                int sel = int.Parse(Console.ReadLine());

                if (sel == 0)
                    sel = jarray.IndexOf(jarray.Where(x => !bool.Parse(x["prerelease"].ToString())).First()) + 1;

                JToken item;
                if (jarray[sel - 1]["assets"].Any(x => x["name"].ToString().ToLower() == "debug.zip"))
                    item = jarray[sel - 1]["assets"].Where(x => x["name"].ToString().ToLower() == "debug.zip").First();
                else
                {
                    IEnumerable<JToken> js = jarray[sel - 1]["assets"].Where(x => x["name"].ToString().EndsWith(".zip")).OrderBy(x => x["created_at"]);
                    item = js.Last();
                }

                for (int i = 0; i < (jarray[sel - 1]["assets"] as JArray).Count; i++)
                {
                    Console.WriteLine(jarray[sel - 1]["assets"][i]["name"]);
                }
                Console.WriteLine();

                Console.WriteLine(item["browser_download_url"].ToString());
                Thread.Sleep(1000);
                WebClient wc = new WebClient();
                if (File.Exists(Path.Combine(rootDir, "Update.zip")))
                    File.Delete(Path.Combine(rootDir, "Update.zip"));
                wc.DownloadFile(new Uri(item["browser_download_url"].ToString()), Path.Combine(rootDir, "Update.zip"));
                wc.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) =>
                {
                    Console.WriteLine(e.ProgressPercentage);
                };
                file = "Update.zip";
            }
            else file = args[0];
            using (ZipArchive archive = ZipFile.OpenRead(Path.Combine(rootDir, file)))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    try
                    {
                        if (Directory.Exists(entry.FullName))
                            Directory.Delete(entry.FullName, true);
                        if (File.Exists(entry.FullName))
                            File.Delete(entry.FullName);
                        if (!Directory.Exists(Path.GetDirectoryName(Path.Combine(rootDir, entry.FullName))))
                            Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(rootDir, entry.FullName)));
                        entry.ExtractToFile(entry.FullName);
                    }
                    catch (Exception ex) { Console.WriteLine(ex.Message); }
                }
            }
            Thread.Sleep(1000);
            File.Delete(Path.Combine(rootDir, file));
            Process.Start(Path.Combine(rootDir, "HitomiViewer.exe"));
            Environment.Exit(0);
        }
        public static async void Download(string[] args)
        {
            Console.WriteLine("Download");
            string file;
            if (args.Length <= 0)
            {
                string s = await Load("https://api.github.com/repos/rmagur1203/HitomiViewer/releases");
                JArray jarray = JArray.Parse(s);

                Console.WriteLine("0: 최신 버전 (프리 릴리즈 제외)");
                for (int i = 0; i < jarray.Count; i++)
                {
                    Console.Write("{0}: {1}", i + 1, jarray[i]["tag_name"].ToString());
                    if (bool.Parse(jarray[i]["prerelease"].ToString()) == true)
                        Console.Write(" (프리 릴리즈)");
                    Console.Write("\n");
                }
                Console.WriteLine();
                Console.Write("선택: ");
                int sel = int.Parse(Console.ReadLine());

                if (sel == 0)
                    sel = jarray.IndexOf(jarray.Where(x => !bool.Parse(x["prerelease"].ToString())).First()) + 1;

                JToken item;
                if (jarray[sel - 1]["assets"].Any(x => x["name"].ToString().ToLower() == "debug.zip"))
                    item = jarray[sel - 1]["assets"].Where(x => x["name"].ToString().ToLower() == "debug.zip").First();
                else
                {
                    IEnumerable<JToken> js = jarray[sel - 1]["assets"].Where(x => x["name"].ToString().EndsWith(".zip")).OrderBy(x => x["created_at"]);
                    item = js.Last();
                }
                Console.WriteLine(item["browser_download_url"].ToString());
                Thread.Sleep(1000);
                WebClient wc = new WebClient();
                if (File.Exists(Path.Combine(rootDir, "Update.zip")))
                    File.Delete(Path.Combine(rootDir, "Update.zip"));
                wc.DownloadFile(new Uri(item["browser_download_url"].ToString()), Path.Combine(rootDir, "Update.zip"));
                wc.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) =>
                {
                    Console.WriteLine(e.ProgressPercentage);
                };
                file = "Update.zip";
            }
            else file = args[0];
            using (ZipArchive archive = ZipFile.OpenRead(Path.Combine(rootDir, file)))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    try
                    {
                        if (Directory.Exists(entry.FullName))
                            Directory.Delete(entry.FullName, true);
                        if (File.Exists(entry.FullName))
                            File.Delete(entry.FullName);
                        if (!Directory.Exists(Path.GetDirectoryName(Path.Combine(rootDir, entry.FullName))))
                            Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(rootDir, entry.FullName)));
                        entry.ExtractToFile(entry.FullName);
                    }
                    catch (Exception e) { Console.WriteLine(e.Message); }
                }
            }
            File.Delete(Path.Combine(rootDir, file));
            Process.Start(Path.Combine(rootDir, "HitomiViewer.exe"));
            Environment.Exit(0);
        }
        public static async Task<string> Load(string Url)
        {
            if (Url.Last() == '/') Url = Url.Remove(Url.Length - 1);
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd($"HitomiViewerUpdater");
            var response = await client.GetAsync(Url);
            var pageContents = await response.Content.ReadAsStringAsync();
            return pageContents;
        }
        static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            Assembly thisAssembly = Assembly.GetExecutingAssembly();
            var name = args.Name.Substring(0, args.Name.IndexOf(',')) + ".dll";

            var resources = thisAssembly.GetManifestResourceNames().Where(s => s.EndsWith(name));
            if (resources.Count() > 0)
            {
                string resourceName = resources.First();
                using (Stream stream = thisAssembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        byte[] assembly = new byte[stream.Length];
                        stream.Read(assembly, 0, assembly.Length);
                        Console.WriteLine("Dll file load : " + resourceName);
                        return Assembly.Load(assembly);
                    }
                }
            }
            return null;
        }
    }
}
