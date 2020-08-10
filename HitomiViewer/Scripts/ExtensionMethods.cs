﻿using System;
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

namespace ExtensionMethods
{
    public static partial class MyExtensions
    {
        #region SORT
        [System.Runtime.InteropServices.DllImport("Shlwapi.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        public static extern int StrCmpLogicalW(string psz1, string psz2);
        public static IEnumerable<string> CustomSort(this IEnumerable<string> list)
        {
            int maxLen = list.Select(s => s.Length).Max();

            return list.Select(s => new
            {
                OrgStr = s,
                SortStr = System.Text.RegularExpressions.Regex.Replace(s, @"(\d+)|(\D+)", m => m.Value.PadLeft(maxLen, char.IsDigit(m.Value[0]) ? ' ' : '\xffff'))
            })
            .OrderBy(x => x.SortStr)
            .Select(x => x.OrgStr);
        }
        public static IEnumerable<string> FileInfoSort(this IEnumerable<string> list)
        {
            return list.Select(fn => new FileInfo(fn)).OrderBy(f => f.Name).Select(f => f.FullName);
        }
        public static void HitomiPanelSort(this StackPanel MainPanel)
        {
            HitomiPanel[] childs = MainPanel.Children.Cast<HitomiPanel>().ToArray();
            MainPanel.Children.Clear();
            Dictionary<string, HitomiPanel> panelKey = new Dictionary<string, HitomiPanel>();
            foreach (HitomiPanel child in childs)
            {
                string name = (((child.panel as DockPanel).Children[1] as DockPanel).Children[0] as Label).Content as string;
                panelKey.Add(name, child);
            }
            string[] names = panelKey.Select(k => Path.Combine(Global.MainWindow.path, k.Key)).IEESort();
            for (int i = 0; i < names.Length; i++)
            {
                string name = names[i].Split(Path.DirectorySeparatorChar).Last();
                Console.WriteLine(name);
                Global.MainWindow.label.Content = i + "/" + names.Length;
                MainPanel.Children.Add(panelKey[name]);
                panelKey[name].thumbNail.Source = panelKey[name].thumbNail.Source;
            }
        }
        public static Hitomi[] HitomiSort(this Hitomi[] hlist)
        {
            Dictionary<string, Hitomi> hitomiKey = new Dictionary<string, Hitomi>();
            foreach (Hitomi h in hlist)
            {
                string name = h.dir;
                hitomiKey.Add(name, h);
            }
            string[] names = hitomiKey.Select(h => h.Key).IEESort();
            List<Hitomi> hitomis = new List<Hitomi>();
            for (int i = 0; i < names.Length; i++)
            {
                string name = names[i];
                hitomis.Add(hitomiKey[name]);
            }
            return hitomis.ToArray();
        }
        public static string[] ESort(this string[] list)
        {
            return list.Select(f => new FileInfo(f)).ToArray().ExplorerSort().Select(f => f.FullName).ToArray();
        }
        public static string[] IEESort(this IEnumerable<string> list)
        {
            return list.Select(f => new FileInfo(f)).ToArray().ExplorerSort().Select(f => f.FullName).ToArray();
        }
        public static FileInfo[] ExplorerSort(this FileInfo[] list)
        {
            Array.Sort(list, delegate (FileInfo x, FileInfo y) { return StrCmpLogicalW(x.Name, y.Name); });
            return list;
        }
        #endregion
        #region Array
        public static bool ItemsEqual<TSource>(this TSource[] array1, TSource[] array2)
        {
            if (array1 == null && array2 == null)
                return true;
            if (array1 == null || array2 == null)
                return false;
            return array1.Count() == array2.Count() && !array1.Except(array2).Any();
        }
        #endregion

        public static Hitomi Copy(this Hitomi hitomi)
        {
            return Hitomi.Copy(hitomi);
        }
        public static string RemoveSpace(this string s) => s.Replace(" ", string.Empty);
        public static bool isUrl(this string s)
        {
            Uri uriResult;
            bool result = Uri.TryCreate(s, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            return result;
        }
        public static bool isNull(this string s) => s == null;

        #region JSON
        public static string StringValue(this JObject config, string path)
        {
            if (config == null) return null;
            if (!config.ContainsKey(path)) return null;
            return config[path].ToString();
        }
        public static int? IntValue(this JObject config, string path)
        {
            int res;
            if (config == null) return null;
            if (!config.ContainsKey(path)) return null;
            if (!int.TryParse(config[path].ToString(), out res)) return null;
            return res;
        }
        public static double? DoubleValue(this JObject config, string path)
        {
            double res;
            if (config == null) return null;
            if (!config.ContainsKey(path)) return null;
            if (double.TryParse(config[path].ToString(), out res)) return null;
            return res;
        }
        public static bool? BoolValue(this JObject config, string path)
        {
            if (config == null) return null;
            if (!config.ContainsKey(path)) return null;
            return bool.Parse(config[path].ToString());
        }
        public static IList<T> ArrayValue<T>(this JObject config, string path) where T : class
        {
            if (config == null) return new List<T>();
            if (!config.ContainsKey(path)) return new List<T>();
            return config[path].ToObject<List<T>>();
        }

        public static string StringValue(this JToken config, string path)
        {
            if (config == null) return null;
            if (config[path] == null) return null;
            return config[path].ToString();
        }
        public static int? IntValue(this JToken config, string path)
        {
            int res;
            if (config == null) return null;
            if (config[path] == null) return null;
            if (!int.TryParse(config[path].ToString(), out res)) return null;
            return res;
        }
        public static double? DoubleValue(this JToken config, string path)
        {
            double res;
            if (config == null) return null;
            if (config[path] == null) return null;
            if (double.TryParse(config[path].ToString(), out res)) return null;
            return res;
        }
        public static bool? BoolValue(this JToken config, string path)
        {
            if (config == null) return null;
            if (config[path] == null) return null;
            return bool.Parse(config[path].ToString());
        }
        public static IList<T> ArrayValue<T>(this JToken config, string path) where T : class
        {
            if (config == null) return new List<T>();
            if (config[path] == null) return new List<T>();
            return config[path].ToObject<List<T>>();
        }
        #endregion

        public static async void TaskCallback<T>(this Task<T> Task, Action<T> callback) where T : class => callback(await Task);
        public static async void then<T>(this Task<T> Task, Action<T> callback) where T : class => callback(await Task);
        public static bool ToBool(this int i) => Convert.ToBoolean(i);
        public static void RemoveAllEvents(this EventHandler events)
        {
            foreach (EventHandler eh in events.GetInvocationList())
            {
                events -= eh;
            }
        }

        public static IEnumerable<string> StartsContains(this IEnumerable<string> ie, string s)
        {
            IEnumerable<string> res = ie.Where(x => x.StartsWith(s));
            return res.Concat(ie.Where(x => x.Contains(s))).Distinct();
        }
    }
}
