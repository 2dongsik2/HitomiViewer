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
            return obj.ToObject<HiyobiGallery>();
        }
    }
}
