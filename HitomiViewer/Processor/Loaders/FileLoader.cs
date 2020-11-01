using ExtensionMethods;
using HitomiViewer.Plugin;
using HitomiViewer.Scripts;
using HitomiViewerLibrary.Loaders;
using HitomiViewerLibrary.Structs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitomiViewer.Processor.Loaders
{
    class FileLoader : ILoader
    {
        public Action<Hitomi, int, int> hitomiupdate = null;
        public Action<HiyobiGallery, int, int> hiyobiupdate = null;

        public override void Default()
        {
            base.Default();
            this.hitomiupdate = (Hitomi h, int index, int max) =>
            {
                Global.MainWindow.label.Content = $"{index}/{max}";
                Global.MainWindow.MainPanel.Children.Add(new UserControls.Panels.HitomiPanel(h, true));
            };
            this.hiyobiupdate = (HiyobiGallery h, int index, int max) =>
            {
                Global.MainWindow.label.Content = $"{index}/{max}";
                Global.MainWindow.MainPanel.Children.Add(new UserControls.Panels.HiyobiPanel(h, true));
            };
        }
        public override void Parser()
        {
            Parser(Global.MainWindow.path);
        }
        public void Parser(string directory)
        {
            int SelectedPage = Global.MainWindow.Page;
            uint PageCount = Global.MainWindow.Page_itemCount;
            string[] files = Directory.GetDirectories(directory);
            Parser(files);
        }
        public void Parser(string[] files)
        {
            int SelectedPage = Global.MainWindow.Page;
            uint PageCount = Global.MainWindow.Page_itemCount;
            files = Global.FolderSort(files);
            if (Global.MainWindow.searchType == MainWindow.SearchType.reversal)
                files = files.Reverse().ToArray();
            files = files.Where(x => CheckRangePC(Array.IndexOf(files, x) + 1, SelectedPage, checked((int)PageCount))).ToArray();
            int i = 0;
            for (; i < files.Length; i++)
            {
                string folder = files[i];
                Console.WriteLine("{0}: {1}", i, folder);
                if (!File.Exists(Path.Combine(folder, "info.json"))) continue;
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".avif", ".lock" };
                IEnumerable<string> innerFiles = Directory.GetFiles(folder);
                innerFiles = innerFiles.Where(file => allowedExtensions.Any(file.ToLower().EndsWith)).ESort();
                JObject data = JObject.Parse(File.ReadAllText(Path.Combine(folder, "info.json")));
                CheckType(data, folder, i, files.Length);
            }
        }
        private void CheckType(JObject data, string path, int index, int length)
        {
            if (data["thumb"] != null)
            {
                DeprecatedFile(data, index, length);
                return;
            }
            int? type = (int?)(data["fileType"] ?? data["type"]);
            switch (type)
            {
                case (int)IHitomi.HFileType.Hitomi:
                    Hitomi hitomi = data.ToObject<Hitomi>();
                    hitomi.FileInfo.dir = path;
                    hitomiupdate(hitomi, index, length);
                    break;
                case (int)IHitomi.HFileType.Hiyobi:
                    HiyobiGallery hiyobi = data.ToObject<HiyobiGallery>();
                    hiyobi.FileInfo.dir = path;
                    hiyobiupdate(hiyobi, index, length);
                    break;
                default:
                    PluginHandler.FireUnknownFileLoaded(type ?? 0, data);
                    break;
            }
        }
        private void DeprecatedFile(JObject data, int index, int length)
        {
            int? type = (int?)(data["fileType"] ?? data["type"]);
            if (type == 1 || type == 2)
            {
                Hitomi h = new Hitomi();
                h.id = data.Value<string>("id");
                h.name = data.Value<string>("name") + " (다시 다운로드가 필요함)";
                h.thumbnail.preview_url = data.Value<string>("thumbpath");
                h.files = new Hitomi.HFile[0];
                hitomiupdate(h, index, length);
            }
            
        }
        private bool CheckRangePC(int index, int page, int count) => CheckRange(index, (page - 1) * count, page * count);
        private bool CheckRange(int index, int start, int end) => index <= end && index > start;
    }
}
