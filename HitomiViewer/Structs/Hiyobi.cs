using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitomiViewer
{
    public class Hiyobi
    {
        public class HFile : Hitomi.HFile
        {
            public int id { get; set; }
        }
    }

    public class HiyobiGallery : IHitomi
    {
        public HiyobiGallery()
        {
            base.fileType = HFileType.Hiyobi;
        }

        [JsonInfo("category")]
        public int category { get; set; }
        [JsonInfo("type")]
        public int type { get; set; }
        [JsonInfo("uid")]
        public int uid { get; set; }
        [JsonInfo("language")]
        public string language { get; set; }    
        public DisplayValue[] artists { get; set; }
        public DisplayValue[] characters { get; set; }
        public DisplayValue[] groups { get; set; }
        public DisplayValue[] parodys { get; set; }
        public DisplayValue[] tags { get; set; }
        public Hiyobi.HFile[] files { get; set; }
        public string title
        {
            get => name;
            set => name = value;
        }

        public IEnumerable<object> Values
        {
            get
            {
                yield return category;
                yield return type;
                yield return uid;
                yield return language;
                yield return characters;
                yield return groups;
                yield return parodys;
                yield return tags;
            }
        }
    }
}
