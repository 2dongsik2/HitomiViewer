using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HitomiViewer;
using System.Windows.Interop;
using System.Windows;
using Bitmap = System.Drawing.Bitmap;
using System.Windows.Controls;
using System.Windows.Threading;
using HitomiViewer.UserControls;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Web;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using HitomiViewer.Processor;

namespace ExtensionMethods
{
    public static partial class Extensions
    {
        public static T ToObjectExceptNull<T>(this JObject obj) where T : class
        {
            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer
            {
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
            };
            return obj.ToObject<T>(serializer);
        }
    }
}
