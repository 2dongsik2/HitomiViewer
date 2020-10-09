using ExtensionMethods;
using HitomiViewerLibrary;
using HitomiViewerLibrary.Loaders;
using HitomiViewerLibrary.Structs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace HitomiViewer.Processor.Loaders
{
    partial class SearchLoader
    {
        /// <summary>
        /// 0. int => index
        /// 1. int => count
        /// </summary>
        public Action<int, int> update = null;
        /// <summary>
        /// 0. int => index
        /// 1. int => count
        /// </summary>
        public Action<int> start = null;
        public Action end = null;
        public string[] tags = null;
        public int itemCount = 0;
        public int index = 1;

        private const bool search_range = false;
        private const int search_count = 1000;

        private static Dictionary<string[], int[]> cache = new Dictionary<string[], int[]>();

        public SearchLoader(Action<int, int> update = null,
                            Action<int> start = null,
                            Action end = null,
                            string[] tags = null,
                            int? itemCount = null,
                            int? index = null)
        {
            this.update = update ?? this.update;
            this.start = start ?? this.start;
            this.end = end ?? this.end;
            this.tags = tags ?? this.tags;
            this.itemCount = itemCount ?? this.itemCount;
            this.index = index ?? 0;
        }

        public SearchLoader Default()
        {
            this.start = (int count) =>
            {
                Global.MainWindow.MainPanel.Children.Clear();
                Global.MainWindow.label.Content = "0/" + count;
                Global.MainWindow.label.Visibility = System.Windows.Visibility.Visible;
                Global.MainWindow.Searching(true);
            };
            this.update = (int index, int max) =>
            {
                Global.MainWindow.label.Content = $"{index}/{max}";
            };
            this.end = () =>
            {
                Global.MainWindow.label.Visibility = System.Windows.Visibility.Collapsed;
                Global.MainWindow.Searching(false);
            };
            return this;
        }

        public void HitomiSearch()
        {
            List<int> idlist = new List<int>();
            int compcount = 0;
            if (cache.Keys.Any(x => x.ItemsEqual(tags)))
            {
                string[] key = cache.Keys.Where(x => x.ItemsEqual(tags)).First();
                this.start(0);
                idlist = cache[key].ToList();
                compcount = tags.Length;
            }
            else
            {
                List<Task> tasks = new List<Task>();
                this.start(tags.Length);
                Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
                foreach (string tag in tags)
                {
                    Hitomi.HTag tag2 = Hitomi.HTag.Parse(tag);
                    string path = Path.Combine(Global.rootDir, "Cache", tag2.type, tag2.tag + ".json");
                    if (Global.CacheSearch)
                    {
                        if (File.Exists(path))
                        {
                            JArray arr = JArray.Parse(File.ReadAllText(path));
                            int[] ids = arr.Select(x => int.Parse(x.ToString())).ToArray();
                            idlist = idlist.Concat(ids).ToList();
                            compcount++;
                            continue;
                        }
                    }
                    Thread th = new Thread(new ThreadStart(async () =>
                    {
                        int[] ids;
                        InternetP parser = new InternetP();
                        parser.index = index - 1;
                        parser.Count = search_count;
                        if (HiyobiTags.Tags.Select(y => y.full).Contains(tag))
                        {
                            try
                            {
                                ids = parser.ByteArrayToIntArray(await parser.LoadNozomiTag(tag2.type.ToString(), tag2.tag, range: !Global.CacheSearch, start: 0, end: 99));
                            }
                            catch
                            {
                                ids = new int[0];
                            }
                        }
                        else
                        {
                            ids = await parser.LoadQuery(tag);
                        }
                        if (Global.CacheSearch && !File.Exists(path))
                        {
                            if (!Directory.Exists(Path.GetDirectoryName(path)))
                                Directory.CreateDirectory(Path.GetDirectoryName(path));
                            File.WriteAllText(path, JArray.FromObject(ids).ToString());
                        }
                        dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                                idlist = idlist.Concat(ids).ToList()));
                        compcount++;
                        dispatcher.Invoke(() => update(compcount, tags.Length));
                    }));
                    th.Start();
                }
            }
            int start = (index - 1) * itemCount;
            int PageCount = start + itemCount;
            Task.Factory.StartNew(() =>
            {
                while (compcount != tags.Length) { }
                if (!cache.Keys.Any(x => x.ItemsEqual(tags)))
                    cache.Add(tags, idlist.ToArray());
                List<int> new_idlist = new List<int>();
                for (int i = 0; i < idlist.Count; i++)
                {
                    int count = idlist.Count(y => y == idlist[i]);
                    if (count == tags.Length) new_idlist.Add(idlist[i]);
                    if (new_idlist.Count >= PageCount) break;
                }
                new_idlist = new_idlist.Skip(start).ToList();
                Global.dispatcher.Invoke(delegate
                {
                    HitomiLoader hitomi = new HitomiLoader();
                    hitomi.Default();
                    LoaderDefaults.Hitomis.Setup(hitomi);
                    hitomi.Parser(new_idlist.ToArray());
                });
            });
        }
    }
}