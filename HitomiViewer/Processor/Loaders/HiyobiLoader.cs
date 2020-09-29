using ExtensionMethods;
using HitomiViewer.Processor.Parser;
using HitomiViewer.Scripts;
using HitomiViewer.Structs;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Tag = HitomiViewer.Structs.Tag;

namespace HitomiViewer.Processor.Loaders
{
    class HiyobiLoader : ILoader
    {
        public Action<HiyobiGallery, int, int> update = null;

        public override void Default()
        {
            base.Default();
            this.update = (HiyobiGallery h, int index, int max) =>
            {
                Global.MainWindow.label.Content = $"{index}/{max}";
                Global.MainWindow.MainPanel.Children.Add(new UserControls.Panels.HiyobiPanel(h, true));
            };
        }

        /// <summary>
        /// Parse from search/recently
        /// </summary>
        /// <param name="obj"></param>
        public void Parser(JObject obj)
        {
            base.Parser();
            start(obj["list"].Count());
            JArray arr = obj["list"] as JArray;
            for (int i = 0; i < arr.Count; i++)
            {
                HiyobiGallery h = HiyobiParser.Parse(arr[i]);
                update(h, i, arr.Count);
            }
            end();
            int pages = (int)Math.Ceiling((obj.IntValue("count") ?? 0) / ((double)arr.Count));
            pagination?.Invoke(pages);
        }
        public async Task Parser(int[] ids)
        {
            start(ids.Length);
            for (int i = 0; i < ids.Length; i++)
            {
                InternetP parser = new InternetP();
                HiyobiGallery h = await parser.HiyobiGallery(ids[i]);
                update(h, i, ids.Count());
            }
            end();
        }
    }
}
