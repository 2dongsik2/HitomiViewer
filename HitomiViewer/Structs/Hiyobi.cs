using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitomiViewer
{
    class Hiyobi : IHitomi
    {
        public int category { get; set; }
        public int type { get; set; }
        public int uid { get; set; }
        public DisplayValue[] characters { get; set; }
        public DisplayValue[] groups { get; set; }
        public DisplayValue[] parodys { get; set; }
        public DisplayValue[] tags { get; set; }
        public string title
        {
            get => name;
            set => name = value;
        }
    }
}
