﻿using ExtensionMethods;
using HitomiViewer.Properties;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace HitomiViewer.Scripts
{
    class CheckUpdate
    {
        public static void Auto()
        {
            _ = UpdateChain();
        }
        public static async Task Main()
        {
            Github github = new Github();
            github.owner = "rmagur1203";
            github.repos = "HitomiViewer";
            JArray jarray = await github.Releases();
            JToken release = jarray.Where(x => !(x.BoolValue("prerelease") ?? false)).First();
            Version thisv = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            Version repov = Version.Parse(release["tag_name"].ToString());
            if (repov > thisv)
            {
                MessageBoxResult result = MessageBox.Show("업데이트가 필요합니다.", "", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.Cancel) return;
                JToken item = release["assets"].Where(x => x["browser_download_url"].ToString().EndsWith(".zip")).First();
                WebClient wc = new WebClient();
                if (File.Exists(Path.Combine(MainWindow.rootDir, "Update.zip")))
                    File.Delete(Path.Combine(MainWindow.rootDir, "Update.zip"));
                wc.DownloadFileAsync(new Uri(item["browser_download_url"].ToString()), Path.Combine(MainWindow.rootDir, "Update.zip"));
                wc.DownloadFileCompleted += (object sender, AsyncCompletedEventArgs e) =>
                {
                    Process.Start(Path.Combine(MainWindow.rootDir, "Updater.exe"), "Update.zip");
                    Environment.Exit(0);
                };
            }
        }
        public static async Task Updater()
        {
            Version version = new Version(0, 0, 0, 0);
            if (File.Exists(Path.Combine(MainWindow.rootDir, "Updater.exe")))
            {
                FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Path.Combine(MainWindow.rootDir, "Updater.exe"));
                version = Version.Parse(versionInfo.FileVersion);
            }
            string s = await Load("https://api.github.com/repos/rmagur1203/HitomiViewer/releases");
            JArray jarray = JArray.Parse(s);
            JToken latest = jarray.Where(x => x["assets"].Any(y => y["name"].ToString() == "Updater.exe")).First();
            Version latestv = Version.Parse(latest["tag_name"].ToString());
            string download = latest["assets"].Where(x => x["name"].ToString() == "Updater.exe").First()["browser_download_url"].ToString();
            if (latestv > version)
            {
                File.Delete(Path.Combine(MainWindow.rootDir, "Updater.exe"));
                WebClient wc = new WebClient();
                await wc.DownloadFileTaskAsync(download, Path.Combine(MainWindow.rootDir, "Updater.exe"));
                Console.WriteLine("Updater has been updated!");
            }
            Console.WriteLine(download);
        }
        public static async Task UpdateChain()
        {
            if (!File.Exists(Path.Combine(Global.rootDir, "WebPTest.exe")))
                File.WriteAllBytes(Path.Combine(Global.rootDir, "WebPTest.exe"), Resources.WebPTest);
            await Updater();
            await Main();
        }
        public static async Task<string> Load(string Url)
        {
            if (Url.Last() == '/') Url = Url.Remove(Url.Length - 1);
            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd($"HitomiViewerUpdater");
            var response = await client.GetAsync(Url);
            var pageContents = await response.Content.ReadAsStringAsync();
            return pageContents;
        }
    }
}
