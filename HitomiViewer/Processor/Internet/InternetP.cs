using ExtensionMethods;
using HitomiViewer.Processor.Loaders;
using HitomiViewer.Scripts;
using HitomiViewer.Structs;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HitomiViewer.Processor
{
    partial class InternetP
    {
        public string url { get; set; }
        public string data { get; set; }
        public List<string> keyword { get; set; }
        public int index { get; set; }
        public int Count { get; set; }
        public Action<Hitomi, int, int> update = null;
        public Action<int> start = null;
        public Action end = null;

        public InternetP(string url = null, string data = null, List<string> keyword = null, int? index = null)
        {
            if (url != null) this.url = url;
            if (data != null) this.data = data;
            if (keyword != null) this.keyword = keyword;
            if (index != null) this.index = index.Value;
        }

        public InternetP SetData(string data)
        {
            this.data = data;
            return this;
        }

        
        
        public List<Hitomi> LoadCompre(List<string> items)
        {
            return null;
        }

        public long GetWebSize(string url = null)
        {
            System.Net.WebClient client = new System.Net.WebClient();
            client.OpenRead(url ?? this.url);
            long bytes_total = Convert.ToInt64(client.ResponseHeaders["Content-Length"]);
            return bytes_total;
        }
        public async Task<string> Load(string url = null)
        {
            url = url ?? this.url;
            if (url.Last() == '/') url = url.Remove(url.Length - 1);
            HttpClient client = new HttpClient();
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }
        public async Task<string> LoadRange(int start, int end, string url = null)
        {
            url = url ?? this.url;
            if (url.Last() == '/') url = url.Remove(url.Length - 1);
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Range = new System.Net.Http.Headers.RangeHeaderValue(start, end);
            HttpResponseMessage response = await client.GetAsync(url);
            return await response.Content.ReadAsStringAsync();
        }
        public async Task<byte[]> LoadRangeByte(int start, int end, string url = null)
        {
            url = url ?? this.url;
            if (url.Last() == '/') url = url.Remove(url.Length - 1);
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Range = new System.Net.Http.Headers.RangeHeaderValue(start, end);
            HttpResponseMessage response = await client.GetAsync(url);
            return await response.Content.ReadAsByteArrayAsync();
        }
        public int[] ByteArrayToIntArray(byte[] arr)
        {
            List<int> intarr = new List<int>();
            int co = arr.Length / 4;
            for (var i = 0; i < co; i++)
            {
                byte[] iarr = arr.ToList().Skip(i * 4).Take(4).ToArray();
                intarr.Add(BitConverter.ToInt32(iarr.Reverse().ToArray(), 0));
            }
            return intarr.ToArray();
        }
        public int[] ByteArrayToIntArrayBig(byte[] arr)
        {
            List<int> intarr = new List<int>();
            int co = arr.Length / 4;
            for (var i = 0; i < co; i++)
            {
                byte[] iarr = arr.ToList().Skip(i * 4).Take(4).ToArray();
                intarr.Add(BitConverter.ToInt32(iarr.ToArray(), 0));
            }
            return intarr.ToArray();
        }

        public string HitomiFullPath(string str)
        {
            string first = str.Last().ToString();
            string second = string.Join("", str.Substring(str.Length - 3).Take(2));
            return $"{first}/{second}/{str}";
        }
    }
}
