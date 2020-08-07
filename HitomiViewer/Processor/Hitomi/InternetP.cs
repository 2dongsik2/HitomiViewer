using ExtensionMethods;
using HitomiViewer.Encryption;
using HitomiViewer.Structs;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HitomiViewer.Processor
{
    partial class InternetP
    {
        private string domain = "ltn.hitomi.la";
        private string index_dir = "tagindex";
        private string galleries_index_dir = "galleriesindex";
        private int max_node_size = 464;

        public async Task<Hitomi> HitomiData()
        {
            url = url ?? $"https://ltn.hitomi.la/galleryblock/{index}.html";
            string html = await Load(url);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            Hitomi h = new Hitomi();
            h.dir = $"https://hitomi.la/reader/{index}.html";
            //h.dir = url;
            HtmlNode name = doc.DocumentNode.SelectSingleNode("//h1[@class=\"lillie\"]");
            h.name = name.InnerText;
            HtmlNode image = doc.DocumentNode.SelectSingleNode("//div[@class=\"dj-img1\"]/img");
            image = image ?? doc.DocumentNode.SelectSingleNode("//div[@class=\"cg-img1\"]/img");
            h.thumbpath = image.GetAttributeValue("src", "");
            if (h.thumbpath == "")
                h.thumbpath = image.GetDataAttribute("src").Value;
            //HtmlNode artist = doc.DocumentNode.SelectSingleNode("//div[@class=\"artist-list\"]/ul");
            HtmlNodeCollection artists = doc.DocumentNode.SelectNodes("//div[@class=\"artist-list\"]/ul/li");
            if (artists != null)
            {
                h.authors = artists.Select(x => x.InnerText).ToArray();
                h.author = string.Join(", ", h.authors);
            }
            else
            {
                h.authors = new string[0];
                h.author = "";
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
        public async Task<JObject> HitomiGalleryInfo()
        {
            string html = await Load(url);
            JObject jObject = JObject.Parse(html.Replace("var galleryinfo = ", ""));
            return jObject;
        }
        public async Task<Hitomi> HitomiGalleryData(Hitomi org)
        {
            JObject jObject = await HitomiGalleryInfo();
            List<HitomiFile> files = new List<HitomiFile>();
            foreach (JToken tag1 in jObject["files"])
            {
                files.Add(new HitomiFile
                {
                    hash = tag1["hash"].ToString(),
                    name = tag1["name"].ToString(),
                    hasavif = Convert.ToBoolean(int.Parse(tag1["hasavif"].ToString())),
                    haswebp = Convert.ToBoolean(int.Parse(tag1["haswebp"].ToString()))
                });
            }
            org.language = jObject.StringValue("language");
            org.id = jObject.StringValue("id");
            org.designType = DesignTypeFromString(jObject.StringValue("type"));
            return org;
        }
        public async Task<Hitomi> HitomiData2()
        {
            url = $"https://ltn.hitomi.la/galleryblock/{index}.html";
            Hitomi h = await HitomiData();
            url = $"https://ltn.hitomi.la/galleries/{index}.js";
            JObject info = await HitomiGalleryInfo();
            h.type = Hitomi.Type.Hitomi;
            h.tags = HitomiTags(info);
            h.files = HitomiFiles(info).ToArray();
            h.page = h.files.Length;
            h.thumb = ImageProcessor.LoadWebImage("https:" + h.thumbpath);
            h.Json = info;
            return await HitomiGalleryData(h);
        }
        public List<Tag> HitomiTags(JObject jObject)
        {
            List<Tag> tags = new List<Tag>();
            foreach (JToken tag1 in jObject["tags"])
            {
                Tag tag = new Tag();
                tag.types = Tag.Types.tag;
                if (tag1.SelectToken("female") != null && tag1["female"].ToString() == "1")
                    tag.types = Tag.Types.female;
                if (tag1.SelectToken("male") != null && tag1["male"].ToString() == "1")
                    tag.types = Tag.Types.male;
                tag.name = tag1["tag"].ToString();
                tags.Add(tag);
            }
            return tags;
        }
        public List<string> HitomiFiles(JObject jObject)
        {
            List<string> files = new List<string>();
            foreach (JToken tag1 in jObject["files"])
            {
                files.Add($"https://aa.hitomi.la/webp/{HitomiFullPath(tag1["hash"].ToString())}.webp");
            }
            return files;
        }
        public async Task<byte[]> LoadNozomi(string url = null)
        {
            url = url ?? this.url ?? "https://ltn.hitomi.la/index-all.nozomi";
            if (url.Last() == '/') url = url.Remove(url.Length - 1);
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Range = new System.Net.Http.Headers.RangeHeaderValue(index * 4, (index + count) * 4 - 1);
            var response = await client.GetAsync(url);
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
            string nozomiextension = ".nozomi";
            string domain = "ltn.hitomi.la";
            string compressed_nozomi_prefix = "";
            string subtag = string.Join("-", new string[]{tag, language});
            var nozomi_address = "//" + string.Join("/", new string[] { domain, compressed_nozomi_prefix, subtag }) + nozomiextension;
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
            //if (BitConverter.IsLittleEndian) data = data.Reverse().ToArray();

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
            for (var i = 0; i<number_of_subnode_addresses; i++) {
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
