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

namespace HitomiViewer.Api
{
    partial class Pixiv
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
}
