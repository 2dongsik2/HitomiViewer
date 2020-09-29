using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitomiViewer.Processor.Parser
{
    public partial class HiyobiParser
    {
        public static HiyobiGallery Parse(JToken obj)
        {
            HiyobiGallery res = obj.ToObject<HiyobiGallery>();
            res.thumbnail = new IHitomi.Thumbnail { preview_url = $"https://cdn.hiyobi.me/tn/{res.id}.jpg" };
            return res;
        }
    }
}
