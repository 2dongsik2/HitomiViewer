using System;
using System.Linq;
using System.Threading.Tasks;

namespace HitomiViewer.Processor.Loaders
{
    class HitomiLoader
    {
        public int index = 0;
        public int count = 0;
        public Action<Hitomi, int, int> update = null;
        public Action<int> start = null;
        public Action<int> pagination = null;
        public Action end = null;

        public HitomiLoader FastDefault()
        {
            Default();
            this.update = (Hitomi h, int index, int max) =>
            {
                Global.MainWindow.label.Content = $"{index}/{max}";
                Global.MainWindow.MainPanel.Children.Add(new UserControls.HitomiPanel(h, Global.MainWindow, true, true));
            };
            return this;
        }
        public HitomiLoader Default()
        {
            this.start = (int count) =>
            {
                Global.MainWindow.label.Content = "0/" + count;
                Global.MainWindow.label.Visibility = System.Windows.Visibility.Visible;
                Global.MainWindow.Searching(true);
            };
            this.update = (Hitomi h, int index, int max) =>
            {
                Global.MainWindow.label.Content = $"{index}/{max}";
                Global.MainWindow.MainPanel.Children.Add(new UserControls.HitomiPanel(h, Global.MainWindow, true));
            };
            this.end = () =>
            {
                Global.MainWindow.label.Visibility = System.Windows.Visibility.Collapsed;
                Global.MainWindow.Searching(false);
            };
            return this;
        }

        public async void FastParser()
        {
            InternetP parser = new InternetP();
            parser.index = (index - 1) * count;
            parser.Count = count;
            parser.url = "https://ltn.hitomi.la/index-all.nozomi";
            Tuple<byte[], long?> result = await parser.LoadNozomiAndRangeMax();
            int[] ids = parser.ByteArrayToIntArray(result.Item1);
            await FastParser(ids);
            int pages = (int)Math.Ceiling((result.Item2 ?? 0) / ((double)count));
            pagination?.Invoke(pages);
        }
        public async Task FastParser(int[] ids)
        {
            start(ids.Length);
            for (int i = 0; i < ids.Length; i++)
            {
                InternetP parser = new InternetP();
                parser.index = ids[i];
                Hitomi h = await parser.HitomiData();
                h.type = Hitomi.Type.Hitomi;
                h.id = ids[i].ToString();
                update(h, i, ids.Count());
            }
            end();
        }
        public async void Parser()
        {
            InternetP parser = new InternetP();
            parser.index = (index - 1) * count;
            parser.Count = count;
            parser.url = "https://ltn.hitomi.la/index-all.nozomi";
            int[] ids = parser.ByteArrayToIntArray(await parser.LoadNozomi());
            start(ids.Count());
            foreach (int id in ids)
            {
                parser.index = id;
                Hitomi h = await parser.HitomiData2();
                update(h, ids.ToList().IndexOf(id), ids.Count());
            }
            end();
        }
        public async Task Parser(int[] ids)
        {
            start(ids.Length);
            InternetP parser = new InternetP();
            for (int i = 0; i < ids.Length; i++)
            {
                parser.index = ids[i];
                Hitomi h = await parser.HitomiData2();
                update(h, i, ids.Count());
            }
            end();
        }
    }
    class HitomiSyncLoader
    {
        public int index = 0;
        public int count = 0;
        public Action<Hitomi, int, int> update = null;
        public Action<int> start = null;
        public Action<int> pagination = null;
        public Action end = null;

        public HitomiSyncLoader Default()
        {
            this.start = (int count) =>
            {
                Global.MainWindow.label.Content = "0/" + count;
                Global.MainWindow.label.Visibility = System.Windows.Visibility.Visible;
                Global.MainWindow.Searching(true);
            };
            this.update = (Hitomi h, int index, int max) =>
            {
                Global.MainWindow.label.Content = $"{index}/{max}";
                Global.MainWindow.MainPanel.Children.Add(new UserControls.HitomiPanel(h, Global.MainWindow, true, true));
            };
            this.end = () =>
            {
                Global.MainWindow.label.Visibility = System.Windows.Visibility.Collapsed;
                Global.MainWindow.Searching(false);
            };
            return this;
        }

        public async void Parser()
        {
            InternetP parser = new InternetP();
            parser.index = (index - 1) * count;
            parser.Count = count;
            parser.url = "https://ltn.hitomi.la/index-all.nozomi";
            Tuple<byte[], long?> result = await parser.LoadNozomiAndRangeMax();
            int[] ids = parser.ByteArrayToIntArray(result.Item1);
            await Parser(ids);
            int pages = (int)Math.Ceiling((result.Item2 ?? 0) / ((double)count));
            pagination?.Invoke(pages);
        }
        public async Task Parser(int[] ids)
        {
            start(ids.Length);
            for (int i = 0; i < ids.Length; i++)
            {
                InternetP parser = new InternetP();
                parser.index = ids[i];
                Hitomi h = await parser.HitomiData();
                h.id = ids[i].ToString();
                update(h, i, ids.Count());
            }
            end();
        }
    }
}
