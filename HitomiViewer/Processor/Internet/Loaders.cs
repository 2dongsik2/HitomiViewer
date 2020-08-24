﻿using ExtensionMethods;
using HitomiViewer.Processor;
using HitomiViewer.Structs;
using HitomiViewer.UserControls;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Hitomi = HitomiViewer.Hitomi;

namespace HitomiViewer.Scripts.Loaders
{
    class HiyobiLoader
    {
        public readonly Hitomi.Type type = HitomiViewer.Hitomi.Type.Hiyobi;

        public string text;
        public int index;
        public Action<Hitomi, int, int> update = null;
        public Action<int> start = null;
        public Action<int> search = null;
        public Action<int> pagination = null;
        public Action end = null;

        public HiyobiLoader(string text = null, int? index = null, Action<Hitomi, int, int> update = null, Action<int> start = null, Action end = null)
        {
            this.text = text ?? this.text;
            this.index = index ?? this.index;
            this.update = update ?? this.update;
            this.start = start ?? this.start;
            this.end = end ?? this.end;
        }

        public HiyobiLoader Default()
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
        public HiyobiLoader FastDefault()
        {
            Default();
            this.update = (Hitomi h, int index, int max) =>
            {
                Global.MainWindow.label.Content = $"{index}/{max}";
                Global.MainWindow.MainPanel.Children.Add(new UserControls.HitomiPanel(h, Global.MainWindow, true, true));
            };
            return this;
        }
        public HiyobiLoader Pagination(int page)
        {
            pagination = (int pages) =>
            {
                Pagination pagination = new Pagination(page, pages);
                pagination.btnFirs_Clk = (object sender, RoutedEventArgs e) => search(1);
                pagination.btnPrev_Clk = (object sender, RoutedEventArgs e) => search(page - 1);
                pagination.btnNext_Clk = (object sender, RoutedEventArgs e) => search(page + 1);
                pagination.btnLast_Clk = (object sender, RoutedEventArgs e) => search(pages);
                pagination.cbNumberOfRecords_SelectionChanged1 = (object sender, SelectionChangedEventArgs e) => search((int)e.AddedItems[0]);
                Global.MainWindow.MainPanel.Children.Add(pagination);
            };
            return this;
        }

        public async void Parser(JObject jobject)
        {
            start(jobject["list"].Count());
            foreach (JToken tk in jobject["list"])
            {
                InternetP parser = new InternetP(url: $"https://cdn.hiyobi.me/data/json/{tk["id"]}_list.json");
                JArray imgs = await parser.TryLoadJArray();
                if (imgs == null) continue;
                Hitomi h = new Hitomi
                {
                    id = tk["id"].ToString(),
                    name = tk["title"].ToString(),
                    type = type,
                    page = imgs.Count,
                    dir = $"https://hiyobi.me/reader/{tk["id"]}",
                    thumb = ImageProcessor.LoadWebImage($"https://cdn.hiyobi.me/tn/{tk["id"]}.jpg"),
                    thumbpath = $"https://cdn.hiyobi.me/tn/{tk["id"]}.jpg",
                    files = imgs.ToList().Select(x => $"https://cdn.hiyobi.me/data/{tk["id"]}/{x["name"]}").ToArray(),
                    author = string.Join(", ", tk["artists"].Select(x => x["display"].ToString())),
                    authors = tk["artists"].Select(x => x["display"].ToString()).ToArray(),
                    Json = tk
                };
                foreach (JToken tags in tk["tags"])
                {
                    Tag tag = new Tag();
                    if (tags["value"].ToString().Contains(":"))
                        tag.types = (Tag.Types)Enum.Parse(typeof(Tag.Types), tags["value"].ToString().Split(':')[0]);
                    else
                        tag.types = Tag.Types.tag;
                    tag.name = tags["display"].ToString();
                    h.tags.Add(tag);
                }
                update(h,
                    jobject["list"].ToList().IndexOf(tk),
                    jobject["list"].Count());
            }
            end();
        }
        public void FastParser(JObject jobject)
        {
            start(jobject["list"].Count());
            JArray arr = jobject["list"] as JArray;
            for (int i = 0; i < arr.Count; i++)
            {
                Hitomi h = new InternetP().HiyobiParse(arr[i]);
                h.type = Hitomi.Type.Hiyobi;
                Config config = new Config();
                config.Load();
                if (!config.ArrayValue<string>(Settings.except_tags).Any(x => h.tags.Select(y => y.full).Contains(x)) || !(config.BoolValue(Settings.block_tags) ?? false))
                    update(h, i, arr.Count);
            }
            int pages = (int)Math.Ceiling((jobject.IntValue("count") ?? 0) / ((double)arr.Count));
            end();
            pagination?.Invoke(pages);
        }
        public async Task<Hitomi> Parser()
        {
            InternetP parser;
            parser = new InternetP(url: $"https://api.hiyobi.me/gallery/{text}");
            JObject obj = await parser.LoadJObject();
            parser = new InternetP(url: $"https://cdn.hiyobi.me/data/json/{text}_list.json");
            JArray imgs = await parser.TryLoadJArray();
            if (imgs == null) return null;
            Hitomi h = new Hitomi
            {
                id = obj.StringValue("id"),
                name = obj.StringValue("title"),
                type = type,
                page = imgs.Count,
                dir = $"https://hiyobi.me/reader/{text}",
                thumb = ImageProcessor.LoadWebImage($"https://cdn.hiyobi.me/tn/{text}.jpg"),
                thumbpath = $"https://cdn.hiyobi.me/tn/{text}.jpg",
                files = imgs.ToList().Select(x => $"https://cdn.hiyobi.me/data/{text}/{x["name"]}").ToArray(),
                author = string.Join(", ", obj["artists"].Select(x => x["display"].ToString())),
                authors = obj["artists"].Select(x => x["display"].ToString()).ToArray(),
                Json = obj,
                designType = new InternetP().DesignTypeFromInt(obj.IntValue("type") ?? 0),
                language = obj.StringValue("language")
            };
            foreach (JToken tags in obj["tags"])
            {
                Tag tag = new Tag();
                if (tags["value"].ToString().Contains(":"))
                    tag.types = (Tag.Types)Enum.Parse(typeof(Tag.Types), tags["value"].ToString().Split(':')[0]);
                else
                    tag.types = Tag.Types.tag;
                tag.name = tags["display"].ToString();
                h.tags.Add(tag);
            }
            return h;
        }
        public async Task Parser(string[] ids)
        {
            start(ids.Length);
            for (int i = 0; i < ids.Length; i++)
            {
                this.text = ids[i];
                Hitomi h = await Parser();
                update(h, i, ids.Length);
            }
            end();
        }
        public void Search()
        {
            InternetP parser = new InternetP(keyword: text.Split(' ').ToList(), index: index);
            parser.HiyobiSearch(data => new InternetP(data: data).ParseJObject(Parser));
        }
    }
    class HitomiLoader
    {
        public int index = 0;
        public int count = 0;
        public Action<Hitomi, int, int> update = null;
        public Action<int> start = null;
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
            parser.count = count;
            parser.url = "https://ltn.hitomi.la/index-all.nozomi";
            int[] ids = parser.ByteArrayToIntArray(await parser.LoadNozomi());
            FastParser(ids);
            /*
            start(ids.Count());
            for (int i = 0; i < ids.Length; i++)
            {
                InternetP parser2 = new InternetP();
                parser2.index = ids[i];
                Hitomi h = await parser2.HitomiData();
                h.type = Hitomi.Type.Hitomi;
                h.id = ids[i].ToString();
                update(h, i, ids.Count());
            }
            end();
            */
        }
        public async void FastParser(int[] ids)
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
            parser.count = count;
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
    class InfoLoader
    {
        public static HitomiInfoOrg parseTXT(string s)
        {
            HitomiInfoOrg hitomiInfoOrg = new HitomiInfoOrg();
            string[] lines = s.Split(new char[] { '\n', '\r' }).Where(x => x.Length > 0).ToArray();
            foreach (string line in lines)
            {
                if (line.StartsWith("태그: "))
                {
                    hitomiInfoOrg.Tags = line.Remove(0, "태그: ".Length);
                }
                if (line.StartsWith("작가: "))
                {
                    hitomiInfoOrg.Author = line.Remove(0, "작가: ".Length);
                }
                if (line.StartsWith("갤러리 넘버: "))
                {
                    hitomiInfoOrg.Number = line.Remove(0, "갤러리 넘버: ".Length);
                }
            }
            return hitomiInfoOrg;
        }
    }
}
