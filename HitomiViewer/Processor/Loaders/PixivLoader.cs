using ExtensionMethods;
using HitomiViewer.Api;
using HitomiViewer.Structs;
using HitomiViewer.UserControls;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static HitomiViewer.Api.Pixiv;
using Pixiv = HitomiViewer.Structs.Pixiv;
using Tag = HitomiViewer.Structs.Tag;

namespace HitomiViewer.Processor.Loaders
{
    class PixivLoader
    {
        public int index = 0;
        public int count = 0;
        public Action<Pixiv, int, int> update = null;
        public Action<PixivUser, int, int> pixivUpdate = null;
        public Action<int> start = null;
        public Action<int> pagination = null;
        public Action end = null;

        public PixivLoader FastDefault()
        {
            Default();
            this.update = (Pixiv h, int index, int max) =>
            {
                Global.MainWindow.label.Content = $"{index}/{max}";
                Global.MainWindow.MainPanel.Children.Add(new UserControls.HitomiPanel(h, Global.MainWindow, true, true));
            };
            return this;
        }
        public PixivLoader Default()
        {
            this.start = (int count) =>
            {
                Global.MainWindow.label.Content = "0/" + count;
                Global.MainWindow.label.Visibility = System.Windows.Visibility.Visible;
                Global.MainWindow.Searching(true);
            };
            this.update = (Pixiv h, int index, int max) =>
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
        public PixivLoader UserDefault()
        {
            Default();
            this.pixivUpdate = (PixivUser h, int index, int max) =>
            {
                Global.MainWindow.label.Content = $"{index}/{max}";
                Global.MainWindow.MainPanel.Children.Add(new PixivUserPanel(h));
            };
            return this;
        }

        public async void FastParser()
        {
            if (Global.Account.Pixiv == null)
                return;
            JObject data = await Global.Account.Pixiv.illustFollow();
            FastParser(data);
        }
        public void FastParser(JObject data)
        {
            JArray items = JArray.FromObject(data["illusts"]);
            start(items.Count);
            for (int i = 0; i < items.Count; i++)
            {
                //x_restrict == R-18

                Pixiv h = new Pixiv();
                h.id = items[i].StringValue("id");
                h.dir = "https://www.pixiv.net/artworks/" + h.id;
                h.name = items[i].StringValue("title");
                h.authors.SetAuthor(new string[] { items[i]["user"].StringValue("name") });
                h.thumbnail.preview_url = items[i]["image_urls"].StringValue("square_medium");
                h.page = items[i].IntValue("page_count") ?? 0;
                h.tags = items[i]["tags"].Select(x => new Tag { name = x.StringValue2("translated_name") ?? x.StringValue("name"), types = Tag.Types.none }).ToList();
                if (items[i].IntValue("page_count") <= 1)
                    h.files = new string[] { items[i]["meta_single_page"].StringValue("original_image_url") };
                else
                    h.files = items[i]["meta_pages"].Select(x => x["image_urls"].StringValue("original")).ToArray();
                update(h, i, items.Count);
            }
            end();
        }
        public async void Parser(JObject data)
        {
            JArray items = JArray.FromObject(data["illusts"]);
            start(items.Count);
            for (int i = 0; i < items.Count; i++)
            {
                //x_restrict == R-18

                Hitomi h = new Hitomi();
                h.type = Hitomi.Type.Pixiv;
                h.id = items[i].StringValue("id");
                h.dir = "https://www.pixiv.net/artworks/" + h.id;
                h.name = items[i].StringValue("title");
                h.author = items[i]["user"].StringValue("name");
                h.authors = new string[] { items[i]["user"].StringValue("name") };
                h.thumbpath = items[i]["image_urls"].StringValue("square_medium");
                h.thumb = await ImageProcessor.PixivImage(h.thumbpath);
                h.page = items[i].IntValue("page_count") ?? 0;
                h.tags = items[i]["tags"].Select(x => new Tag { name = x.StringValue2("translated_name") ?? x.StringValue("name"), types = Tag.Types.none }).ToList();
                try
                {
                    Pixiv pixiv = Global.Account.Pixiv;
                    JObject obj = await pixiv.ugoiraMetaData(h.id);
                    WebClient wc = new WebClient();
                    wc.Headers.Add("Referer", "https://www.pixiv.net/");
                    byte[] zipbyte = await wc.DownloadDataTaskAsync(obj["ugoira_metadata"]["zip_urls"].StringValue("medium"));
                    h.ugoiraImage = pixiv.UnZip(zipbyte);
                    h.ugoiraImage.original = obj["ugoira_metadata"]["zip_urls"].StringValue("medium");
                    h.ugoiraImage.delays = obj["ugoira_metadata"]["frames"].Select(x => x.IntValue("delay") ?? 0).ToList();
                    h.name += " (우고이라)";
                }
                catch { }
                if (items[i].IntValue("page_count") <= 1)
                    h.files = new string[] { items[i]["meta_single_page"].StringValue("original_image_url") };
                else
                    h.files = items[i]["meta_pages"].Select(x => x["image_urls"].StringValue("original")).ToArray();
                update(h, i, items.Count);
            }
            end();
        }

        public void UserParser(JObject data)
        {
            JArray jusers = data["user_previews"] as JArray;
            start(jusers.Count);
            for (int i = 0; i < jusers.Count; i++)
            {
                JToken juser = jusers[i];
                PixivUser user = new PixivUser();
                user.user = new PixivUser.User().Parse(juser["user"]);
                JArray illusts = juser["illusts"] as JArray;
                List<Illust> illustlist = new List<Illust>();
                for (int j = 0; j < illusts.Count; j++)
                {
                    illustlist.Add(new Illust().Parse(illusts[j]));
                }
                user.illusts = illustlist.ToArray();
                pixivUpdate(user, i, jusers.Count);
            }
            end();
        }
    }
}
