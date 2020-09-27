using ExtensionMethods;
using HitomiViewer.Scripts;
using HitomiViewer.Structs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static HitomiViewer.Hitomi;
using static HitomiViewer.IHitomi;

namespace HitomiViewer.Processor
{
    partial class InternetP
    {
        public async void HiyobiSearch(Action<string> callback) => callback(await HiyobiSearch());
        public async Task<List<HiyobiGallery>> HiyobiList()
        {
            List<HiyobiGallery> output = new List<HiyobiGallery>();
            url = $"https://api.hiyobi.me/list/{index}";
            JObject obj = await LoadJObject();
            foreach (JToken item in obj["list"])
            {
                HiyobiGallery h = HiyobiParse(item);
                output.Add(h);
            }
            return output;
        }
        public async Task<List<HFile>> HiyobiFiles()
        {
            List<HFile> files = new List<HFile>();
            url = $"https://cdn.hiyobi.me/data/json/{index}_list.json";
            JArray arr = await TryLoadJArray();
            if (arr == null)
                return new List<HFile>();
            foreach (JToken tk in arr)
            {
                files.Add(new HFile
                {
                    hasavif = (tk.IntValue("hasavif") ?? 0).ToBool(),
                    hash = tk.StringValue("hash"),
                    haswebp = (tk.IntValue("haswebp") ?? 0).ToBool(),
                    height = tk.IntValue("height") ?? 0,
                    width = tk.IntValue("width") ?? 0,
                    name = tk.StringValue("name")
                });
            }
            return files;
        }
        public async Task<HiyobiGallery> HiyobiDataNumber(int? index = null)
        {
            url = $"https://api.hiyobi.me/gallery/{index ?? this.index}";
            JObject obj = await LoadJObject();
            HiyobiGallery h = HiyobiParse(obj);
            return h;
        }
        public async Task<string> HiyobiSearch()
        {
            HttpClient client = new HttpClient();
            JObject body = new JObject();
            body.Add("search", JToken.FromObject(this.keyword));
            body.Add("paging", this.index);
            var response = await client.PostAsync("https://api.hiyobi.me/search", new StringContent(body.ToString(), Encoding.UTF8, "application/json"));
            var pageContents = await response.Content.ReadAsStringAsync();
            return pageContents;
        }
        public HiyobiGallery HiyobiParse(JToken item)
        {
            return item.ToObject<HiyobiGallery>();
            /*
            HiyobiGallery h = new HiyobiGallery();
            h.authors.Set(item["artists"].Select(x => x.StringValue("display")));
            h.id = item.StringValue("id");
            h.language = item.StringValue("language");
            h.tags.JsonParseFromName(item["tags"]);
            if (item["artists"] != null)
                h.artists = item["artists"].Select(x => new DisplayValue { Display = x.StringValue("display"), Value = x.StringValue("value") }).ToList();
            if (item["characters"] != null)
                h.characters = item["characters"].Select(x => new DisplayValue { Display = x.StringValue("display"), Value = x.StringValue("value") }).ToList();
            if (item["parodys"] != null)
                h.parodys = item["parodys"].Select(x => new DisplayValue { Display = x.StringValue("display"), Value = x.StringValue("value") }).ToList();
            h.name = item.StringValue("title");
            h.designType = DesignTypeFromString(item.StringValue("type"));
            h.thumbpath = $"https://cdn.hiyobi.me/tn/{h.id}.jpg";
            h.dir = $"https://hiyobi.me/reader/{h.id}";
            h.page = 0;
            h.AutoAuthor();
            return h;
            */
        }
        public async Task<JArray> HiyobiTags()
        {
            url = "https://api.hiyobi.me/auto.json";
            return await LoadJArray();
        }
        /// <summary>
        /// Hiyobi 에서 불러올 수 있는지와 불러올 수 있다면 Hitomi 데이터까지 반환합니다.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task<Tuple<bool, HiyobiGallery>> isHiyobiData(int? index = null)
        {
            try
            {
                HiyobiGallery h = await HiyobiDataNumber(index);
                return new Tuple<bool, HiyobiGallery>(true, h);
            }
            catch { return new Tuple<bool, HiyobiGallery>(false, null); }
        }
        public async Task<bool> isHiyobi(int? index = null)
        {
            try
            {
                _ = await Load($"https://cdn.hiyobi.me/data/json/{index ?? this.index}.json");
                return true;
            }
            catch { return false; }
        }
    }
}
