using ExtensionMethods;
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
    public static partial class Extensions
    {
        public static string toString(this SearchTarget target)
        {
            if (target == SearchTarget.TAGS_PARTIAL)
                return "partial_match_for_tags";
            if (target == SearchTarget.TAGS_EXACT)
                return "exact_match_for_tags";
            if (target == SearchTarget.TITLE_AND_CAPTION)
                return "title_and_caption";
            return "";
        }
        public static string toString(this Sort target)
        {
            if (target == Sort.DATE_ASC)
                return "date_asc";
            if (target == Sort.DATE_DESC)
                return "date_desc";
            return "";
        }
    }

    public partial class Pixiv
    {
        private const string BASE_URL = "https://app-api.pixiv.net";
        private const string CLIENT_ID = "MOBrBDS8blbauoSck0ZfDbtuzpyT";
        private const string CLIENT_SECRET = "lsACyCD94FhDUtGTXi3QzcFE2uU1hqtDaKeqrdwj";
        private const string HASH_SECRET = "28c1fdd170a5204386cb1313c7077b34f83e4aaf4aa829ce78c231e05b0bae2c";
        private const string FILTER = "for_ios";

        private JObject auth;
        private bool rememberPassword = false;
        private string username;
        private string password;
        public bool authStatus { get => !auth.Value<bool>("has_error"); }
        public string error
        {
            get
            {
                if (authStatus) return null;
                return auth.Value<string>("error");
            }
        }

        public enum SearchTarget
        {
            TAGS_PARTIAL, //partial_match_for_tags
            TAGS_EXACT, //exact_match_for_tags
            TITLE_AND_CAPTION, //title_and_caption
        }
        public enum Sort
        {
            DATE_DESC, //date_desc
            DATE_ASC //date_asc
        }

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
        #region illust
        public async Task<JObject> illustRelated(string id, List<KeyValuePair<string, string>> query = null)
        {
            query = query ?? new List<KeyValuePair<string, string>>();
            query.Add(new KeyValuePair<string, string>("illust_id", id));
            string queryString = query.ToQueryString();
            string result = await requestUrl($"/v1/illust/related{queryString}");
            return JObject.Parse(result);
        }
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
        #endregion
        #region Search
        public async Task<JObject> searchIllust(string word, SearchTarget search_target = SearchTarget.TAGS_PARTIAL, Sort sort = Sort.DATE_DESC)
        {
            List<KeyValuePair<string, string>> query = new List<KeyValuePair<string, string>>();
            query.Add(new KeyValuePair<string, string>("word", word));
            query.Add(new KeyValuePair<string, string>("search_target", search_target.toString()));
            query.Add(new KeyValuePair<string, string>("sort", sort.toString()));
            //query.Add(new KeyValuePair<string, string>("filter", FILTER));
            string queryString = query.ToQueryString();
            string result = await requestUrl($"/v1/search/illust{queryString}");
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
        public async Task<JObject> searchAutoComplete(string word)
        {
            List<KeyValuePair<string, string>> query = new List<KeyValuePair<string, string>>();
            query.Add(new KeyValuePair<string, string>("word", word));
            string queryString = query.ToQueryString();
            string result = await requestUrl($"/v1/search/autocomplete{queryString}");
            return JObject.Parse(result);
        }
        #endregion
        #region User
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
        #endregion
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
            public object[] tools = null;
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
                Image_urls obj = data.ToObject<Image_urls>();
                this.square_medium = obj.square_medium;
                this.medium = obj.medium;
                this.large = obj.large;
                this.original = obj.original;
                this.original_image_url = obj.original_image_url;
                return data.ToObject<Image_urls>();
            }
            public string LargestSizeUrl()
            {
                if (original_image_url != null)
                    return original_image_url;
                if (original != null)
                    return original;
                if (large != null)
                    return large;
                if (medium != null)
                    return medium;
                if (square_medium != null)
                    return medium;
                return null;
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
        }
    }
    public partial class PixivUgoira
    {
        public List<byte[]> bytesofimages;
        public List<int> delays;
        public List<BitmapImage> images;
        public string original;
        public int index = 0;
    }
}
