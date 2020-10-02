using ExtensionMethods;
using HitomiViewer.Scripts;
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
                Global.MainWindow.MainPanel.Children.Add(new UserControls.HitomiPanel(h, true));
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
            string[] files = Directory.GetFiles(directory);
            files = Global.FolderSort(files);
            if (Global.MainWindow.searchType == MainWindow.SearchType.reversal)
                files = files.Reverse().ToArray();
            files = files.Where(x => CheckRangePC(Array.IndexOf(files, x) + 1, SelectedPage, checked((int)PageCount))).ToArray();
            int i = 0;
            for (; i < files.Length; i++)
            {
                string folder = files[i];
                Console.WriteLine("{0}: {1}", i, folder);
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".avif", ".lock" };
                IEnumerable<string> innerFiles = Directory.GetFiles(folder);
                innerFiles = innerFiles.Where(file => allowedExtensions.Any(file.ToLower().EndsWith)).ESort();
                JObject data = JObject.Parse(File.ReadAllText(Path.Combine(folder, "info.json")));
                CheckType(data, innerFiles);
            }
        }
        private void CheckType(JObject data, IEnumerable<string> files)
        {

        }
        private bool CheckRangePC(int index, int page, int count) => CheckRange(index, (page - 1) * count, page * count);
        private bool CheckRange(int index, int start, int end) => index <= end && index > start;
    }
}
