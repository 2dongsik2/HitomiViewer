﻿using ExtensionMethods;
using HitomiViewer.Encryption;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using static HitomiViewer.Api.Pixiv;
using static HitomiViewer.Api.PixivUser;

namespace HitomiViewer.Api
{
    public partial class Pixiv
    {
        private const string BASE_URL = "https://app-api.pixiv.net";
        private const string CLIENT_ID = "MOBrBDS8blbauoSck0ZfDbtuzpyT";
        private const string CLIENT_SECRET = "lsACyCD94FhDUtGTXi3QzcFE2uU1hqtDaKeqrdwj";
        private const string HASH_SECRET = "28c1fdd170a5204386cb1313c7077b34f83e4aaf4aa829ce78c231e05b0bae2c";

        private JObject auth;
        private bool rememberPassword = false;
        private string username;
        private string password;

        public async Task<JObject> Auth(string username, string password, bool rememberPassword = false)
        {
            string url = "https://oauth.secure.pixiv.net/auth/token";
            List<KeyValuePair<string, string>> contents = new List<KeyValuePair<string, string>>();
            contents.Add(new KeyValuePair<string, string>("client_id", CLIENT_ID));
            contents.Add(new KeyValuePair<string, string>("client_secret", CLIENT_SECRET));
            contents.Add(new KeyValuePair<string, string>("get_secure_url", "1"));
            contents.Add(new KeyValuePair<string, string>("include_policy", "1"));
            contents.Add(new KeyValuePair<string, string>("grant_type", "password"));
            contents.Add(new KeyValuePair<string, string>("username", username));
            contents.Add(new KeyValuePair<string, string>("password", password));
            FormUrlEncodedContent content = new FormUrlEncodedContent(contents);    
            HttpClient client = new HttpClient();
            string local_time = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz");
            client.DefaultRequestHeaders.Add("User-Agent", "PixivAndroidApp/5.0.64 (Android 6.0)");
            client.DefaultRequestHeaders.Add("X-Client-Time", local_time);
            client.DefaultRequestHeaders.Add("X-Client-Hash", MD5.Hash(local_time+HASH_SECRET));
            HttpResponseMessage result = await client.PostAsync(url, content);
            string data = await result.Content.ReadAsStringAsync();
            this.auth = JObject.Parse(data);
            this.rememberPassword = rememberPassword;
            if (rememberPassword)
            {
                this.username = username;
                this.password = password;
            }
            return this.auth;
        }
        public async Task<Pixiv> AuthChain(string username, string password, bool rememberPassword = false)
        {
            await Auth(username, password, rememberPassword);
            return this;
        }
        public async Task<JObject> illustRelated(string id, List<KeyValuePair<string, string>> query = null)
        {
            query = query ?? new List<KeyValuePair<string, string>>();
            query.Add(new KeyValuePair<string, string>("illust_id", id));
            string queryString = query.ToQueryString();
            string result = await requestUrl($"/v1/illust/related{queryString}");
            return JObject.Parse(result);
        }
        [Obsolete]
        public async Task<JObject> illustDetail(string id, List<KeyValuePair<string, string>> query = null)
        {
            query = query ?? new List<KeyValuePair<string, string>>();
            query.Add(new KeyValuePair<string, string>("illust_id", id));
            string queryString = query.ToQueryString();
            string result = await requestUrl($"/v1/illust/detail{queryString}");
            return JObject.Parse(result);
        }
        public async Task<JObject> illustFollow(List<KeyValuePair<string, string>> query = null)
        {
            query = query ?? new List<KeyValuePair<string, string>>();
            query.Add(new KeyValuePair<string, string>("restrict", "all"));
            string queryString = query.ToQueryString();
            string result = await requestUrl($"/v2/illust/follow{queryString}");
            return JObject.Parse(result);
        }
        public async Task<JObject> illustRecommended(List<KeyValuePair<string, string>> query = null)
        {
            query = query ?? new List<KeyValuePair<string, string>>();
            query.Add(new KeyValuePair<string, string>("include_ranking_illusts", "true"));
            string queryString = query.ToQueryString();
            string result = await requestUrl($"/v1/illust/recommended{queryString}");
            return JObject.Parse(result);
        }
        public async Task<JObject> searchUser(string word)
        {
            List<KeyValuePair<string, string>> query = new List<KeyValuePair<string, string>>();
            query.Add(new KeyValuePair<string, string>("word", word));
            string queryString = query.ToQueryString();
            string result = await requestUrl($"/v1/search/user{queryString}");
            return JObject.Parse(result);
        }
        public async Task<JObject> userDetail(string user_id, List<KeyValuePair<string, string>> query = null)
        {
            query = query ?? new List<KeyValuePair<string, string>>();
            query.Add(new KeyValuePair<string, string>("user_id", user_id));
            query.Add(new KeyValuePair<string, string>("filter", "for_ios"));
            string queryString = query.ToQueryString();
            string result = await requestUrl($"/v1/user/detail{queryString}");
            return JObject.Parse(result);
        }
        public async Task<JObject> userIllusts(string user_id, List<KeyValuePair<string, string>> query = null)
        {
            query = query ?? new List<KeyValuePair<string, string>>();
            query.Add(new KeyValuePair<string, string>("user_id", user_id));
            string queryString = query.ToQueryString();
            string result = await requestUrl($"/v1/user/illusts{queryString}");
            return JObject.Parse(result);
        }
        public async Task<JObject> ugoiraMetaData(string id, List<KeyValuePair<string, string>> query = null)
        {
            query = query ?? new List<KeyValuePair<string, string>>();
            query.Add(new KeyValuePair<string, string>("illust_id", id));
            string queryString = query.ToQueryString();
            string result = await requestUrl($"/v1/ugoira/metadata{queryString}");
            return JObject.Parse(result);
        }

        public void DefaultHeaders(ref HttpRequestMessage message)
        {
            string local_time = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz");
            message.Headers.Add("User-Agent", "PixivAndroidApp/5.0.64 (Android 6.0)");
            message.Headers.Add("X-Client-Time", local_time);
            message.Headers.Add("X-Client-Hash", MD5.Hash(local_time + HASH_SECRET));
            message.Headers.Add("Accept-Language", "English");
        }
        public async Task<string> callApi(string url, HttpRequestMessage message)
        {
            string finalUrl = Regex.IsMatch(url, @"/^https?:\/\//i") ? url : BASE_URL + url;
            HttpClient client = new HttpClient();
            message.RequestUri = new Uri(finalUrl);
            HttpResponseMessage response = await client.SendAsync(message);
            if (!response.IsSuccessStatusCode)
                return response.StatusCode.ToString();
            return await response.Content.ReadAsStringAsync();
        }
        public async Task<string> requestUrl(string url, HttpRequestMessage message = null)
        {
            message = message ?? new HttpRequestMessage();
            DefaultHeaders(ref message);
            if (this.auth != null && this.auth["access_token"] != null)
            {
                message.Headers.Add("Authorization", $"Bearer {this.auth["access_token"]}");
            }
            try
            {
                return await callApi(url, message);
            }
            catch
            {
                if (this.rememberPassword)
                {
                    if (this.username != null && this.password != null)
                    {
                        await this.Auth(this.username, this.password);
                        message.Headers.Authorization = new AuthenticationHeaderValue($"Bearer {this.auth["access_token"]}");
                        return await callApi(url, message);
                    }
                }
                return null;
            }
        }

        public PixivUgoira UnZip(byte[] buffer)
        {
            List<byte[]> images = new List<byte[]>();
            using (var z = new ZipArchive(new MemoryStream(buffer)))
            {
                foreach (var entry in z.Entries)
                {
                    string uncompressedFile = Path.Combine("testDownload", entry.Name);
                    images.Add(ReadFully(entry.Open()));
                }
            }
            PixivUgoira ugoira = new PixivUgoira();
            ugoira.bytesofimages = images;
            return ugoira;
        }
        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
    public partial class Pixiv
    {
        public class Illust
        {
            public int id;
            public string title;
            public string type;
            public Image_urls image_urls;
            public string caption;
            public int restrict;
            public User user;
            public Tag[] tags;
            public string tools = null;
            public string create_date;
            public int page_count;
            public int width;
            public int height;
            public int sanity_level;
            public int x_restrict;
            public string series = null;
            public Image_urls meta_single_page;
            public Image_urls[] meta_pages;
            public int total_view;
            public int total_bookmarks;
            public bool is_bookmarked;
            public bool visible;
            public bool is_muted;

            public Illust Parse(JToken data)
            {
                id = data.IntValue("id") ?? 0;
                title = data.StringValue("title");
                type = data.StringValue("type");
                image_urls = new Image_urls().Parse(data["image_urls"]);
                caption = data.StringValue("caption");
                restrict = data.IntValue("restrict") ?? 0;
                user = new User().Parse(data["user"]);
                JArray jtags = data["tags"] as JArray;
                List<Tag> taglist = new List<Tag>();
                for (int i = 0; i < jtags.Count; i++)
                {
                    taglist.Add(new Tag().Parse(jtags[i]));
                }
                tags = taglist.ToArray();
                create_date = data.StringValue("create_date");
                page_count = data.IntValue("page_count") ?? 0;
                width = data.IntValue("width") ?? 0;
                height = data.IntValue("height") ?? 0;
                sanity_level = data.IntValue("sanity_level") ?? 0;
                x_restrict = data.IntValue("x_restrict") ?? 0;
                meta_single_page = new Image_urls().Parse(data["meta_single_page"]);
                meta_pages = data["meta_pages"].Select(x => new Image_urls().Parse(x)).ToArray();
                total_view = data.IntValue("total_view") ?? 0;
                total_bookmarks = data.IntValue("total_bookmarks") ?? 0;
                is_bookmarked = data.BoolValue("is_bookmarked") ?? false;
                visible = data.BoolValue("visible") ?? false;
                is_muted = data.BoolValue("is_muted") ?? false;
                return this;
            }
        }
        public class Novel
        {

        }
        public class Image_urls
        {
            public string square_medium;
            public string medium;
            public string large;
            public string original; //meta_pages
            public string original_image_url; //meta_single_page
            public Image_urls Parse(JToken data)
            {
                this.square_medium = data.StringValue("square_medium");
                this.medium = data.StringValue("medium");
                this.large = data.StringValue("large");
                this.original = data.StringValue("original");
                this.original_image_url = data.StringValue("original_image_url");
                return this;
            }
        }
        public class Tag
        {
            public string name;
            public string translated_name;

            public Tag Parse(JToken data)
            {
                name = data.StringValue("data");
                translated_name = data.StringValue("translated_name");
                return this;
            }
        }
    }
    public partial class PixivUser
    {
        public User user;
        public Illust[] illusts;
        public Novel[] novels;
        public class User
        {
            public int id;
            public string name;
            public string account;
            public Image_urls profile_image_urls;
            public bool is_followed;

            public User Parse(JToken data)
            {
                id = data.IntValue("id") ?? 0;
                name = data.StringValue("name");
                account = data.StringValue("account");
                profile_image_urls = new Image_urls().Parse(data["profile_image_urls"]);
                is_followed = data.BoolValue("is_followed") ?? false;
                return this;
            }
        }
    }
}
