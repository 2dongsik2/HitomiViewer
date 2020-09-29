using ExtensionMethods;
using HitomiViewer.Encryption;
using HitomiViewer.Structs;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HitomiViewer.Processor
{
    partial class InternetP
    {
        private const string nozomiextension = ".nozomi";
        private const string compressed_nozomi_prefix = "";
        private const string domain = "ltn.hitomi.la";
        private const string index_dir = "tagindex";
        private const string galleries_index_dir = "galleriesindex";
        private const int max_node_size = 464;
        private const bool adapose = false;

        public async Task<Hitomi> HitomiData()
        {
            Hitomi h = await HitomiData1();
            h = await HitomiData2(h);
            return h;
        }
        public async Task<Hitomi> HitomiData1()
        {
            url = url ?? $"https://ltn.hitomi.la/galleryblock/{index}.html";
            string html = await Load(url);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            Hitomi h = new Hitomi();
            h.url = $"https://hitomi.la/reader/{index}.html";
            HtmlNode name = doc.DocumentNode.SelectSingleNode("//h1[@class=\"lillie\"]");
            h.name = name.InnerText;
            HtmlNode image = doc.DocumentNode.SelectSingleNode("//div[@class=\"dj-img1\"]/picture/img");
            image = image ?? doc.DocumentNode.SelectSingleNode("//div[@class=\"cg-img1\"]/picture/img");
            h.thumbnail.preview_url = image.GetAttributeValue("src", "");
            if (!(h.thumbnail.preview_url.StartsWith("https:") || h.thumbnail.preview_url.StartsWith("http:")))
                h.thumbnail.preview_url = "https:" + h.thumbnail.preview_url;
            if (h.thumbnail.preview_url == "")
                h.thumbnail.preview_url = image.GetDataAttribute("src").Value;  
            HtmlNodeCollection artists = doc.DocumentNode.SelectNodes("//div[@class=\"artist-list\"]/ul/li");
            if (artists != null)
            {
                h.authors.Set(artists.Select(x => x.InnerText));
            }
            else
            {
                h.authors.Set(new string[0]);
            }
            HtmlNode table = doc.DocumentNode.SelectSingleNode("//table[@class=\"dj-desc\"]");
            for (var i = 0; i < table.ChildNodes.Count - 1; i += 2)
            {
                HtmlNode tr = table.ChildNodes[i + 1];
                string trname = tr.ChildNodes[0].InnerHtml;
                string trtext = tr.ChildNodes[tr.ChildNodes.Count / 2].InnerHtml.Trim();
            }
            return h;
        }
        public async Task<Hitomi> HitomiData2(Hitomi h, int? index = null)
        {
            url = $"https://ltn.hitomi.la/galleries/{index ?? this.index}.js";
            JObject info = await HitomiGalleryInfo();
            h.tags = HitomiTags(info).ToArray();
            h.files = HitomiFiles(info).ToArray();
            h.thumbnail.preview_img = await ImageProcessor.LoadWebImageAsync("https:" + h.thumbnail.preview_url);
            return await HitomiGalleryData(h);
        }
        public async Task<JObject> HitomiGalleryInfo()
        {
            string html = await Load(url);
            JObject jObject = JObject.Parse(html.Replace("var galleryinfo = ", ""));
            return jObject;
        }
        public async Task<Hitomi> HitomiGalleryData(Hitomi org)
        {
            JObject jObject = await HitomiGalleryInfo();
            List<Hitomi.HFile> files = new List<Hitomi.HFile>();
            foreach (JToken tag1 in jObject["files"])
            {
                files.Add(new Hitomi.HFile
                {
                    hash = tag1["hash"].ToString(),
                    name = tag1["name"].ToString(),
                    width = tag1.IntValue("width") ?? 0,
                    height = tag1.IntValue("height") ?? 0,
                    hasavif = Convert.ToBoolean(int.Parse(tag1["hasavif"].ToString())),
                    hasavifsmalltn = Convert.ToBoolean(int.Parse(tag1["hasavifsmalltn"].ToString())),
                    haswebp = Convert.ToBoolean(int.Parse(tag1["haswebp"].ToString()))
                });
            }
            org.language = jObject.StringValue("language");
            org.id = jObject.StringValue("id");
            org.type = Hitomi.HType.Find(jObject.StringValue("type"));
            return org;
        }
        public List<Hitomi.HTag> HitomiTags(JObject obj) //반환값 변경으로 인한 코드 수정
        {
            List<Hitomi.HTag> tags = new List<Hitomi.HTag>();
            foreach (JToken tag in obj["tags"])
                tags.Add(Hitomi.HTag.Parse(tag));
            return tags;
        }
        public List<Hitomi.HFile> HitomiFiles(JObject jObject)
        {
            List<Hitomi.HFile> files = new List<Hitomi.HFile>();
            foreach (JToken tk in jObject["files"])
            {
                files.Add(new Hitomi.HFile().JsonParseFromName(tk));
            }
            return files;
        }
        public async Task<byte[]> LoadNozomi(string url = null)
        {
            url = (url ?? this.url ?? "https://ltn.hitomi.la/index-all.nozomi").https();
            if (url.Last() == '/') url = url.Remove(url.Length - 1);
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Range = new System.Net.Http.Headers.RangeHeaderValue(index * 4, (index + Count) * 4 - 1);
            var response = await client.GetAsync(url);
            var pageContents = await response.Content.ReadAsByteArrayAsync();
            return pageContents;
        }
        public async Task<Tuple<byte[], long?>> LoadNozomiAndRangeMax(string url = null)
        {
            url = url ?? this.url ?? "https://ltn.hitomi.la/index-all.nozomi";
            if (url.Last() == '/') url = url.Remove(url.Length - 1);
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Range = new System.Net.Http.Headers.RangeHeaderValue(index * 4, (index + Count) * 4 - 1);
            var response = await client.GetAsync(url);
            var MaxRange = response.Content.Headers.ContentRange.Length;
            var pageContents = await response.Content.ReadAsByteArrayAsync();
            return new Tuple<byte[], long?>(pageContents, MaxRange);
        }
        public async Task<byte[]> LoadNozomiTag(string type, string tag, bool range = true, int? start = null, int? end = null)
        {
            start = start ?? index * 4;
            end = end ?? (index + Count) * 4 - 1;
            tag = tag.Replace("_", "%20");
            string url;
            if (type == "female" || type == "male")
                url = $"https://ltn.hitomi.la/tag/{type}:{tag}-all.nozomi";
            else if (type == "language")
                url = $"https://ltn.hitomi.la/index-{tag}.nozomi";
            else
                url = $"https://ltn.hitomi.la/{type}/{tag}-all.nozomi";
            if (url.Last() == '/') url = url.Remove(url.Length - 1);
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromMinutes(30);
            if (range)
                client.DefaultRequestHeaders.Range = new System.Net.Http.Headers.RangeHeaderValue(start, end);
            var response = await client.GetAsync(url);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new Exception("NotFound");
            if (response.StatusCode != System.Net.HttpStatusCode.OK && response.StatusCode != System.Net.HttpStatusCode.PartialContent)
                throw new Exception("Not 200");
            var pageContents = await response.Content.ReadAsByteArrayAsync();
            return pageContents;
        }
        public async Task<byte[]> LoadNozomiMax(string url = null)
        {
            url = url ?? this.url ?? "https://ltn.hitomi.la/index-all.nozomi";
            if (url.Last() == '/') url = url.Remove(url.Length - 1);
            HttpClient client = new HttpClient();
            var response = await client.GetAsync(url);
            var pageContents = await response.Content.ReadAsByteArrayAsync();
            return pageContents;
        }
        public async Task<byte[]> LoadNozomi(string area, string tag, string language)
        {
            string subtag = string.Join("-", new string[]{tag, language});
            string suburl = string.Join("/", new string[] { domain, compressed_nozomi_prefix, subtag }.Where(x => !x.isNull()));
            string nozomi_address = "//" + suburl + nozomiextension;
            return await LoadNozomi(nozomi_address);
        }
        public async Task<int[]> LoadQuery(string query)
        {
            query = Regex.Replace(query, @"/_/g", " ");
            if (query.IndexOf(":") > -1) {
                var sides = query.Split(':');
                var ns = sides[0];
                var tag = sides[1];
                var area = ns;
                var language = "all";
                if ("female" == ns || "male" == ns) {
                    area = "tag";
                    tag = query;
                } else if ("language" == ns) {
                    area = null;
                    language = tag;
                    tag = "index";
                }
                return ByteArrayToIntArray(await LoadNozomi(area, tag, language));
            }
            
            var key = SHA256.HashArray(query).Skip(0).Take(4).ToArray();
            var field = "galleries";
            Console.WriteLine("Step 1 Complete");
            Node node = await NodeAddress(field, 0);
            Console.WriteLine("Step 2 Complete");
            int[] output = await B_search(field, key, node);
            Console.WriteLine("Step 3 Complete");
            int[] ids = await FromData(output);
            return ids;
        }
        public async Task<Node> NodeAddress(string field = null, int address = 0, string serial = null)
        {
            string galleries_index_version = await IndexVersion("galleriesindex");
            var url = "https://" + domain + "/" + index_dir + "/" + field + '.' + UnixTimeNow() + ".index";
            if (field == "galleries")
            {
                url = "https://" + domain + "/" + galleries_index_dir + "/galleries." + galleries_index_version + ".index";
            }
            byte[] data = await LoadRangeByte(address, address + max_node_size - 1, url);
            return decodeNode(data);
        }
        public Node decodeNode(byte[] data)
        {
            Node node = new Node();
            var pos = 0;

            var number_of_keys = GetBigEndian32(data, pos);
            pos += 4;

            var keys = new List<byte[]>();
            for (int i = 0; i < number_of_keys; i++)
            {
                var key_size = GetBigEndian32(data, pos);
                if (key_size == 0 || key_size > 32)
                {
                    throw new Exception("fatal: !key_size || key_size > 32");
                }
                pos += 4;

                keys.Add(data.Skip(pos).Take(key_size).ToArray());
                pos += key_size;
            }


            var number_of_datas = GetBigEndian32(data, pos);
            pos += 4;

            var datas = new List<int[]>();
            for (var i = 0; i < number_of_datas; i++)
            {
                var offset = GetBigEndian64(data, pos);
                pos += 8;

                var length = GetBigEndian32(data, pos);
                pos += 4;

                datas.Add(new int[] { offset, length });
            }

            var number_of_subnode_addresses = 16 + 1;
            var subnode_addresses = new List<int>();
            for (var i = 0; i < number_of_subnode_addresses; i++)
            {
                var subnode_address = GetBigEndian64(data, pos);
                pos += 8;
                subnode_addresses.Add(subnode_address);
            }

            node.keys = keys;
            node.datas = datas;
            node.subnode_addresses = subnode_addresses;

            return node;
        }
        public async Task<int[]> B_search(string field, byte[] key, Node node)
        {
            Tuple<bool, int> org = locate_key(key, node);
            bool there = org.Item1;
            int where = org.Item2;
            if (there) return node.datas[where];
            Node node2 = await NodeAddress(field, node.subnode_addresses[where]);
            return await B_search(field, key, node2);
        }
        public async Task<int[]> FromData(int[] data)
        {
            string galleries_index_version = await IndexVersion("galleriesindex");
            var url = "https://" + domain + "/" + galleries_index_dir + "/galleries." + galleries_index_version + ".data";
            int offset = data[0], length = data[1];
            if (length > 10000 * 4 + 4) length = 10000 * 4 + 4;
            if (length > 100000000 || length <= 0) {
                throw new Exception("length " + length + " is too long");
            }
            byte[] inbuf = await LoadRangeByte(offset, offset + length - 1, url);
            var galleryids = new List<int>();
            var pos = 0;
            var number_of_galleryids = GetBigEndian32(inbuf, pos);
            pos += 4;

            var expected_length = number_of_galleryids * 4 + 4;

            if (number_of_galleryids > 10000000 || number_of_galleryids <= 0)
            {
                throw new Exception("number_of_galleryids " + number_of_galleryids + " is too long");
            }
            /*else if (inbuf.Length != expected_length)
            {
                throw new Exception("inbuf.byteLength " + inbuf.Length + " !== expected_length " + expected_length);
            }*/
            if (number_of_galleryids > 10000)
                number_of_galleryids = 10000;
            for (var i = 0; i < number_of_galleryids; ++i)
            {
                try
                {
                    if (inbuf.Length - 100 <= pos)
                        Console.WriteLine("t");
                    galleryids.Add(GetBigEndian32(inbuf, pos));
                    pos += 4;
                }
                catch
                {
                    Console.WriteLine(0);
                }
            }
            return galleryids.ToArray();
        }

        public string GetDirFromHFile(Hitomi.HFile file)
        {
            if (file.hasavif)
                return "avif";
            if (file.haswebp)
                return "webp";
            return null;
        }
        public string UrlFromUrlFromHash(Hitomi.HFile file)
        {
            return UrlFromUrl(UrlFromHash(file.name, file.hash, GetDirFromHFile(file)));
        }
        public string ImageUrlFromImage(string name, string hash, bool haswebp, bool no_webp)
        {
            string webp = null;
            if (hash != null && haswebp && !no_webp)
            {
                webp = "webp";
            }

            return UrlFromUrl(UrlFromHash(name, hash, webp));
        }
        public string UrlFromHash(string name, string hash, string dir = null, string ext = null)
        {
            ext = ext ?? dir ?? name.Split('.').Last();
            dir = dir ?? "images";

            return "https://a.hitomi.la/" + dir + "/" + FullPathFromHash(hash) + "." + ext;
        }
        public string FullPathFromHash(string hash)
        {
            if (hash.Length < 3)
            {
                return hash;
            }
            return Regex.Replace(hash, @"^.*(..)(.)$", @"$2/$1/" + hash);
        }
        public string UrlFromUrl(string url, string base1 = null)
        {
            return Regex.Replace(url, @"\/\/..?\.hitomi\.la\/", "//" + SubdomainFromUrl(url, base1) + ".hitomi.la/");
        }
        public string SubdomainFromUrl(string url, string base1 = null)
        {
            var retval = "a";
            if (base1 != null)
            {
                retval = base1;
            }

            var number_of_frontends = 3;
            var b = 16;

            var r = @"\/[0-9a-f]\/([0-9a-f]{2})\/";
            var m = Regex.Match(url, r);
            if (m == null)
            {
                return retval;
            }
            string m1 = m.Groups[1].Value;

            try
            {
                var g = Convert.ToInt32(m1, b);

                if (g < 0x30)
                {
                    number_of_frontends = 2;
                }
                if (g < 0x09)
                {
                    g = 1;
                }
                return SubdomainFromGalleryId(g, number_of_frontends) + retval;
            }
            catch
            {
                return retval;
            }
        }
        public char SubdomainFromGalleryId(int g, int number_of_frontends)
        {
            if (adapose)
                return '0';

            var o = g % number_of_frontends;

            return Convert.ToChar(97 + o);
        }

        public Tuple<bool, int> locate_key(byte[] key, Node node)
        {
            var cmp_result = -1;
            int i;
            for (i = 0; i < node.keys.Count; i++)
            {
                cmp_result = compare_arraybuffers(key, node.keys[i]);
                if (cmp_result <= 0)
                {
                    break;
                }
            }
            return new Tuple<bool, int>(cmp_result == 0, i);

        }
        public int compare_arraybuffers(byte[] dv1, byte[] dv2)
        {
            var top = Math.Min(dv1.Length, dv2.Length);
            for (var i = 0; i < top; i++)
            {
                if (dv1[i] < dv2[i])
                    return -1;
                else if (dv1[i] > dv2[i])
                    return 1;
            }
            return 0;
        }

        public async Task<string> IndexVersion(string name)
        {
            string url = "https://" + domain + "/" + name + "/version?_=" + UnixTimeNow();
            string loaded = await Load(url);
            return loaded;
        }
        public long UnixTimeNow()
        {
            var timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
            return (long)timeSpan.TotalSeconds;
        }

        public int GetBigEndian32(byte[] data, int pos)
        {
            byte[] four = data.Skip(pos).Take(4).Reverse().ToArray();
            return BitConverter.ToInt32(four, 0);
        }
        public int GetBigEndian64(byte[] data, int pos)
        {
            byte[] eight = data.Skip(pos).Take(8).Reverse().ToArray();
            return BitConverter.ToInt32(eight, 0);
        }
        public class Node
        {
            public List<byte[]> keys;
            public List<int[]> datas;
            public List<int> subnode_addresses;
        }
    }
}
