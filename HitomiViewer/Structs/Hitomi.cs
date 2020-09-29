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
    public static class Extensions
    {
        [Obsolete]
        public static T JsonParseFromName<T>(this T t, JToken obj)
        {
            var items = typeof(T).GetProperties()
                .ToList();
            foreach (var item in items)
            {
                JsonInfo attr = item.GetCustomAttributes(true).Length >= 1 ? (JsonInfo)item.GetCustomAttributes(true).Where(x => x is JsonInfo).First() : null;
                if (attr != null && attr.ignore) continue;
                item.SetValue(t, obj[item.Name].ToObject(item.PropertyType));
            }
            return t;
        }
        [Obsolete]
        public static T JsonParseFromAttr<T>(this T t, JToken obj) where T : IHitomi
        {
            var items = typeof(HiyobiGallery).GetProperties()
                .Where(x => x.GetCustomAttributes(true)
                .Where(y => y.GetType() == typeof(JsonInfo)).Any())
                .ToList();
            foreach (var item in items)
            {
                JsonInfo attr = (JsonInfo)item.GetCustomAttributes(true).Where(x => x is JsonInfo).First();
                if (attr.ignore) continue;
                item.SetValue(t, obj[attr.path].ToObject(item.PropertyType));
            }
            return t;
        }
        public static void Save<T>(this T t, string path) where T : IHitomi => File.WriteAllText(path, JObject.FromObject(t).ToString());
    }
    public class JsonInfo : Attribute
    {
        public string path;
        public bool ignore = false;
        public Type Type = null;

        public JsonInfo(string path = null)
        {
            if (path != null) this.path = path;
        }
    }
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
            private BitmapImage private_preview_img;
            public BitmapImage preview_img
            {
                get => private_preview_img;
                set => private_preview_img = value;
            }
        }
        public class Authors
        {
            private string[] authors;
            public void Set(string[] authors) => this.authors = authors;
            public void Set(IEnumerable<string> authors) => this.authors = authors.ToArray();
            public T Get<T>()
            {
                if (typeof(T) == typeof(string))
                    return (T)Convert.ChangeType(string.Join("", authors), typeof(T));
                else if (typeof(T) == typeof(string[]))
                    return (T)Convert.ChangeType(authors, typeof(T));
                else if (typeof(T) == typeof(List<string>))
                    return (T)Convert.ChangeType(authors.ToList(), typeof(T));
                else
                    return (T)Convert.ChangeType(authors, typeof(T));
            }
            public IEnumerator<string> GetEnumerator()
            {
                for (int i = 0; i < authors.Length; i++)
                    yield return authors[i];
            }
        }
        public class InFile
        {
            public string dir { get; set; }
            public bool encrypted { get; set; }
        }

        public string id { get; set; }
        public string url { get; set; }
        public string name { get; set; }
        public Thumbnail thumbnail = new Thumbnail();
        public Authors authors = new Authors();
        [JsonInfo(ignore = true)]
        public InFile inFile = null;
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
        public HTag[] tags { get; set; }
        public HFile[] files { get; set; }

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
            public TType ttype = TType.tag;
            public string type { get; set; }
            public string tag { get; set; }
            public string url { get; set; }
            public string full
            {
                get => type + ":" + tag;
            }

            public static HTag Parse(JToken obj)
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
            public class TType
            {
                public static readonly TType tag = new TType(0, "tag");
                public static readonly TType female = new TType(1, "female");
                public static readonly TType male = new TType(2, "male");
                public static readonly TType language = new TType(3, "language");
                public static readonly TType none = tag;

                public static IEnumerable<TType> Values
                {
                    get
                    {
                        yield return tag;
                        yield return female;
                        yield return male;
                        yield return language;
                    }
                }

                public static TType Find(string name)
                {
                    foreach (TType type in Values)
                        if (type.Name.Equals(name))
                            return type;
                    return none;
                }
                public static TType Find(int index)
                {
                    foreach (TType type in Values)
                        if (type.Index.Equals(index))
                            return type;
                    return none;
                }

                public int Index { get; private set; }
                public string Name { get; private set; }

                TType(int index, string name) => (Index, Name) = (index, name);

                public override string ToString() => Name;
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
            [JsonInfo(ignore = true)]
            public string url
            {
                get => new InternetP().UrlFromUrlFromHash(this);
            }
        }
    }
}
