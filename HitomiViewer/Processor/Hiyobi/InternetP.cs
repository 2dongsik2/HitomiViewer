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
        /// <summary>
        /// Fetch recently data(Json)
        /// </summary>
        /// <typeparam name="T">JObject</typeparam>
        /// <param name="index"></param>
        /// <returns>return recently galleries data to json</returns>
        public async Task<JObject> HiyobiList<T>(int? index = null) where T : JObject
        {
            url = $"https://api.hiyobi.me/list/{index ?? this.index}";
            return await LoadJObject();
        }
        /// <summary>
        /// Fetch recently data
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task<List<HiyobiGallery>> HiyobiList(int? index = null)
        {
            List<HiyobiGallery> output = new List<HiyobiGallery>();
            JObject obj = await HiyobiList<JObject>(index);
            foreach (JToken item in obj["list"])
            {
                HiyobiGallery h = HiyobiParse(item);
                output.Add(h);
            }
            return output;
        }
        /// <summary>
        /// Fetch HFiles from id
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task<List<Hiyobi.HFile>> HiyobiFiles(int? index = null)
        {
            List<Hiyobi.HFile> files = new List<Hiyobi.HFile>();
            url = $"https://cdn.hiyobi.me/data/json/{index ?? this.index}_list.json";
            JArray arr = await TryLoadJArray();
            if (arr == null)
                return new List<Hiyobi.HFile>();
            foreach (JToken tk in arr)
            {
                files.Add(new Hiyobi.HFile
                {
                    hasavif = (tk.IntValue("hasavif") ?? 0).ToBool(),
                    hash = tk.StringValue("hash"),
                    haswebp = (tk.IntValue("haswebp") ?? 0).ToBool(),
                    height = tk.IntValue("height") ?? 0,
                    width = tk.IntValue("width") ?? 0,
                    name = tk.StringValue("name"),
                    id = index ?? this.index
                });
            }
            return files;
        }
        /// <summary>
        /// Fetch data from id(Json)
        /// </summary>
        /// <typeparam name="T">JObject</typeparam>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task<JObject> HiyobiGallery<T>(int? index = null) where T : JObject
        {
            url = $"https://api.hiyobi.me/gallery/{index ?? this.index}";
            return await LoadJObject();
        }
        /// <summary>
        /// Fetch Hiyobi data from id
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task<HiyobiGallery> HiyobiGallery(int? index = null)
        {
            JObject obj = await HiyobiGallery<JObject>(index);
            HiyobiGallery h = HiyobiParse(obj);
            return h;
        }
        public async Task<JObject> HiyobiSearch<T>(int? index = null, List<string> keyword = null) where T : JObject => JObject.Parse(await HiyobiSearch(index, keyword));
        public async Task<string> HiyobiSearch(int? index = null, List<string> keyword = null)
        {
            HttpClient client = new HttpClient();
            JObject body = new JObject();
            body.Add("search", JToken.FromObject(keyword ?? this.keyword));
            body.Add("paging", index ?? this.index);
            var response = await client.PostAsync("https://api.hiyobi.me/search", new StringContent(body.ToString(), Encoding.UTF8, "application/json"));
            var pageContents = await response.Content.ReadAsStringAsync();
            return pageContents;
        }
        public HiyobiGallery HiyobiParse(JToken item)
        {
            return item.ToObject<HiyobiGallery>();
        }
        public async Task<JArray> HiyobiTags()
        {
            url = "https://api.hiyobi.me/auto.json";
            return await LoadJArray();
        }
        /// <summary>
        /// Hiyobi 에서 불러올 수 있는지와 불러올 수 있다면 Hitomi 데이터까지 반환합니다.
        /// </summary>
        /// <param name="index">id</param>
        /// <returns></returns>
        public async Task<Tuple<bool, HiyobiGallery>> isHiyobiData(int? index = null)
        {
            try
            {
                HiyobiGallery h = await HiyobiGallery(index);
                return new Tuple<bool, HiyobiGallery>(true, h);
            }
            catch { return new Tuple<bool, HiyobiGallery>(false, null); }
        }
        public async Task<bool> isHiyobi(int? index = null) => (await isHiyobiData(index)).Item1;
    }
}
