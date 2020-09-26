using ExtensionMethods;
using HitomiViewer.Processor;
using HitomiViewer.Scripts;
using HitomiViewer.Structs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace HitomiViewer
{
    public class IHitomi
    {
        public class DisplayValue
        {
            public string Display;
            public string Value;
        }
        public class Thumbnail
        {
            public string preview_url;
            public BitmapImage preview_img;
        }
        public class Authors
        {
            private string[] authors;
            public void SetAuthor(string[] authors) => this.authors = authors;
            public void SetAuthor(IEnumerable<string> authors) => this.authors = authors.ToArray();
            public T GetAuthor<T>() where T : struct
            {
                if (typeof(T) == typeof(string))
                    return (T)Convert.ChangeType(string.Join("", authors), typeof(T));
                else if (typeof(T) == typeof(string[]))
                    return (T)Convert.ChangeType(authors, typeof(T));
                else
                    return default;
            }
        }

        public string id { get; set; }
        public string name { get; set; }
        public Thumbnail thumbnail { get; set; }
        public Authors authors = new Authors();
    }
    public class Hitomi : IHitomi
    {
        public string date { get; set; }
        public string japanese_title { get; set; }
        public string language { get; set; }
        public string language_localname { get; set; }
        public string title
        {
            get => name;
            set => name = value;
        }
        public HType type { get; set; }
        public HTag tags { get; set; }
        public HFile files { get; set; }

        public class HType
        {
            public static readonly HType doujinshi = new HType(1, "doujinshi");
            public static readonly HType artistcg = new HType(2, "artistcg");
            public static readonly HType manga = new HType(3, "manga");
            public static readonly HType none = new HType(0, "none");

            public static IEnumerable<HType> Values
            {
                get
                {
                    yield return doujinshi;
                    yield return artistcg;
                    yield return manga;
                    yield return none;
                }
            }

            public static HType Find(string name)
            {
                foreach (HType type in Values)
                    if (type.Name.Equals(name))
                        return type;
                return none;
            }
            public static HType Find(int index)
            {
                foreach (HType type in Values)
                    if (type.Index.Equals(index))
                        return type;
                return none;
            }

            public int    Index { get; private set; }
            public string Name  { get; private set; }

            HType(int index, string name) => (Index, Name) = (index, name);

            public override string ToString() => Name;
        }
        public class HTag
        {
            public string type { get; set; }
            public string tag { get; set; }
            public string url { get; set; }
            public string full
            {
                get => type + ":" + tag;
            }

            public static HTag Parse(JObject obj)
            {
                HTag tag = new HTag();
                if (obj["female"] != null || obj["male"] != null)
                {
                    if (obj.BoolSValue("female") ?? false)
                        tag.type = "female";
                    if (obj.BoolSValue("male") ?? false)
                        tag.type = "male";
                    tag.tag = obj.StringValue("tag");
                }
                else
                {
                    string url = obj.StringValue("url");
                    string type = url.Split('/').Skip(1).First();
                    tag.tag = obj.StringValue("tag");
                    tag.type = type;
                    tag.url = url;
                }
                return tag;
            }
        }
        public class HFile
        {
            public string hash { get; set; }
            public string name { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public bool hasavif { get; set; }
            public bool hasavifsmalltn { get; set; }
            public bool haswebp { get; set; }

            public static HFile Parse(JToken obj)
            {
                return new HFile
                {
                    hash = obj.StringValue("hash"),
                    name = obj.StringValue("name"),
                    width = obj.IntValue("width") ?? 0,
                    height = obj.IntValue("height") ?? 0,
                    hasavif = obj.BoolSValue("hasavif") ?? false,
                    hasavifsmalltn = obj.BoolSValue("hasavifsmalltn") ?? false,
                    haswebp = obj.BoolSValue("haswebp") ?? false
                }
            }
        }
    }
}
