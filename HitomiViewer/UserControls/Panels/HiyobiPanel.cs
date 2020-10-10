using ExtensionMethods;
using HitomiViewer.Processor;
using HitomiViewer.Processor.Loaders;
using HitomiViewer.Scripts;
using HitomiViewer.UserControls.Reader;
using HitomiViewerLibrary;
using HitomiViewerLibrary.Structs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace HitomiViewer.UserControls.Panels
{
    class HiyobiPanel : IHitomiPanel
    {
        private new readonly HiyobiGallery h;

        public HiyobiPanel(HiyobiGallery h, bool large = true, bool file = false, bool blur = false)
        {
            this.large = large;
            this.file = file;
            this.blur = blur;
            this.h = h;
            base.h = h;
            
            Init();
            InitEvent();
        }

        public override async void Init()
        {
            if (h.thumbnail.preview_img == null)
            {
                if (h.thumbnail.preview_url == null)
                    h.thumbnail.preview_img = ImageProcessor.FromResource("NoImage.jpg");
                else
                    h.thumbnail.preview_img = await ImageProcessor.ProcessEncryptAsync(h.thumbnail.preview_url).@catch(null, sourceName: MethodBase.GetCurrentMethod().FullName());
            }
            if (h.files == null)
            {
                h.files = (await new InternetP().HiyobiFiles(int.Parse(h.id))).ToArray();
            }
            thumbNail.Source = h.thumbnail.preview_img;
            thumbBrush.ImageSource = h.thumbnail.preview_img;
            nameLabel.Content = h.name;
            pageLabel.Content = h.files.Length;
            sizeLabel.Content = CF.File.SizeStr(h.FileInfo.size);
            if (h.files.Length > 0)
                sizeperpageLabel.Content = CF.File.SizeStr(h.FileInfo.size / h.files.Length);
            TagsInit();
        }
        private void TagsInit()
        {
            foreach (Hitomi.DisplayValue value in h.tags)
            {
                Hitomi.HTag htag = Hitomi.HTag.Parse(value.Value);
                TagControl tagElem = new TagControl
                {
                    TagName = value.Display
                };
                if (htag.ttype == Hitomi.HTag.TType.female)
                    tagElem.TagColor = new SolidColorBrush(Color.FromRgb(255, 94, 94));
                else if (htag.ttype == Hitomi.HTag.TType.male)
                    tagElem.TagColor = new SolidColorBrush(Color.FromRgb(65, 149, 244));
                else
                    tagElem.TagColor = new SolidColorBrush(Color.FromRgb(153, 153, 153));
                tagPanel.Children.Add(tagElem);
            }
        }
        public override void InitEvent()
        {
            base.InitEvent();
            thumbNail.MouseDown += (object sender, MouseButtonEventArgs e) =>
            {
                HiyobiReader reader = new HiyobiReader(h);
                if (!reader.IsClosed)
                    reader.Show();
            };
        }

        public override void ChangeColor()
        {
            //Plugin.PluginHandler.FireOnHitomiChangeColor(this);
            panel.Background = new SolidColorBrush(Global.background);
            border.Background = new SolidColorBrush(Global.imagecolor);
            InfoPanel.Background = new SolidColorBrush(Global.Menuground);
            bottomPanel.Background = new SolidColorBrush(Global.Menuground);
            authorsPanel.Background = new SolidColorBrush(Global.Menuground);
            nameLabel.Foreground = new SolidColorBrush(Global.fontscolor);
            sizeLabel.Foreground = new SolidColorBrush(Global.fontscolor);
            pageLabel.Foreground = new SolidColorBrush(Global.fontscolor);
            sizeperpageLabel.Foreground = new SolidColorBrush(Global.fontscolor);
            foreach (UIElement elem in authorsPanel.Children)
                (elem as Label).Foreground = new SolidColorBrush(Global.artistsclr);
            (authorsPanel.Children[0] as Label).Foreground = new SolidColorBrush(Global.fontscolor);
            tagPanel.Background = new SolidColorBrush(Global.Menuground);
        }
        public static void ChangeColor(HitomiPanel hpanel)
        {
            DockPanel panel = hpanel.panel as DockPanel;

            Border border = panel.Children[0] as Border;
            DockPanel InfoPanel = panel.Children[1] as DockPanel;

            StackPanel bottomPanel = InfoPanel.Children[1] as StackPanel;
            StackPanel authorsStackPanel = InfoPanel.Children[2] as StackPanel;

            DockPanel authorsPanel = authorsStackPanel.Children[0] as DockPanel;

            Label nameLabel = InfoPanel.Children[0] as Label;

            Label sizeLabel = bottomPanel.Children[0] as Label;
            Label pageLabel = bottomPanel.Children[2] as Label;
            Label sizeperpageLabel = bottomPanel.Children[4] as Label;

            StackPanel tagPanel = InfoPanel.Children[2] as StackPanel;

            panel.Background = new SolidColorBrush(Global.background);
            border.Background = new SolidColorBrush(Global.imagecolor);
            InfoPanel.Background = new SolidColorBrush(Global.Menuground);
            bottomPanel.Background = new SolidColorBrush(Global.Menuground);
            nameLabel.Foreground = new SolidColorBrush(Global.fontscolor);
            sizeLabel.Foreground = new SolidColorBrush(Global.fontscolor);
            pageLabel.Foreground = new SolidColorBrush(Global.fontscolor);
            sizeperpageLabel.Foreground = new SolidColorBrush(Global.fontscolor);
            authorsPanel.Children.Cast<UIElement>().Select(x => (x as Label).Foreground = new SolidColorBrush(Global.artistsclr));
            tagPanel.Background = new SolidColorBrush(Global.Menuground);
        }

        public override void HitomiPanel_Loaded(object sender, RoutedEventArgs e)
        {
            base.HitomiPanel_Loaded(sender, e);
            thumbNail.MouseEnter += (object _, MouseEventArgs __) => thumbNail.ToolTip = GetToolTip(panel.Height);
        }

        public override void DownloadData_Click(object sender, RoutedEventArgs e)
        {
            base.DownloadData_Click(sender, e);
            JObject export = JObject.FromObject(h);
            export["thumbnail"]["preview_img"] = null;
            export.Remove("Values");    
            string filename = CF.File.GetDownloadTitle(CF.File.SaftyFileName(h.name));
            string root = Path.Combine(Global.rootDir, Global.config.download_folder.Get<string>());
            Directory.CreateDirectory(Path.Combine(root, filename));
            File.WriteAllText(Path.Combine(root, filename, "info.json"), export.ToString());
            Process.Start(Path.Combine(root, filename));
        }
        public override void DownloadImage_Click(object sender, RoutedEventArgs e)
        {
            base.DownloadImage_Click(sender, e);
            string filename = CF.File.GetDownloadTitle(CF.File.SaftyFileName(h.name));
            for (int i = 0; i < h.files.Length; i++)
            {
                Hitomi.HFile file = h.files[i];
                WebClient wc = new WebClient();
                wc.Headers.Add("referer", "https://hitomi.la/");
                string folder = Global.config.download_folder.Get<string>();
                if (!File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}/{folder}/{filename}/{i}.jpg"))
                {
                    bool fileen = Global.config.download_file_encrypt.Get<bool>();
                    h.FileInfo.encrypted = fileen;
                    string path = $"{AppDomain.CurrentDomain.BaseDirectory}/{folder}/{filename}/{file.name}.{InternetP.GetDirFromHFileS(file)}.lock";
                    if (fileen)
                        FileEncrypt.DownloadAsync(wc, new Uri(file.url), path + ".lock");
                    else wc.DownloadFileAsync(new Uri(file.url), path);
                }
            }
        }
    }
}
