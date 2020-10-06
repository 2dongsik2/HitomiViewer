using System;
using System.Linq;
using System.Threading.Tasks;

namespace HitomiViewer.Processor.Loaders
{
    class HitomiLoader : ILoader
    {
        public Action<Hitomi, int, int> update = null;

        public override void Default()
        {
            base.Default();
            this.update = (Hitomi h, int index, int max) =>
            {
                Global.MainWindow.label.Content = $"{index}/{max}";
                Global.MainWindow.MainPanel.Children.Add(new UserControls.HitomiPanel(h, true));
            };
        }

        public override async void Parser()
        {
            base.Parser();
            InternetP parser = new InternetP();
            parser.index = (index - 1) * count;
            parser.Count = count;
            parser.url = "https://ltn.hitomi.la/index-all.nozomi";
            Tuple<byte[], long?> result = await parser.LoadNozomiAndRangeMax();
            if (result == null) return;
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
