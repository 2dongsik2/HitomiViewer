using ExtensionMethods;
using HitomiViewer.Api;
using HitomiViewer.Encryption;
using HitomiViewer.Plugin;
using HitomiViewer.Processor;
using HitomiViewer.Processor.Loaders;
using HitomiViewer.Scripts;
using HitomiViewer.Structs;
using HitomiViewer.UserControls.Reader;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Path = System.IO.Path;

#pragma warning disable CS0618 // 형식 또는 멤버는 사용되지 않습니다.
namespace HitomiViewer.UserControls
{
    /// <summary>
    /// HitomiPanel.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class HitomiPanel : IHitomiPanel
    {
        public new Hitomi h;

        public HitomiPanel(Hitomi h, bool large = true, bool file = false, bool blur = false)
        {
            this.large = large;
            this.file = file;
            this.blur = blur;
            this.h = h;
            base.h = h;
            InitializeComponent();
            Init();
            InitEvent();
        }

        public override void InitEvent()
        {
            thumbNail.MouseDown += (object sender, MouseButtonEventArgs e) =>
            {
                HitomiReader reader = new HitomiReader(h);
                if (!reader.IsClosed)
                    reader.Show();
            };
        }
        public override async void Init()
        {
            if (h.thumbnail.preview_img == null)
                h.thumbnail.preview_img = await ImageProcessor.LoadWebImageAsync(h.thumbnail.preview_url.https());
            thumbNail.Source = h.thumbnail.preview_img;
            thumbBrush.ImageSource = h.thumbnail.preview_img;
            nameLabel.Content = h.name;
            pageLabel.Content = h.files.Length;
            sizeLabel.Content = File2.SizeStr(h.FileInfo.size);
            if (h.files.Length > 0)
                sizeperpageLabel.Content = File2.SizeStr(h.FileInfo.size / h.files.Length);
        }

        public override void ContextSetup()
        {
            Folder_Remove.Visibility = Visibility.Collapsed;
            Folder_Hiyobi_Search.Visibility = Visibility.Collapsed;
            Hiyobi_Download.Visibility = Visibility.Collapsed;
            Pixiv_Download.Visibility = Visibility.Collapsed;
            AtHitomi.Visibility = Visibility.Collapsed;
            Encrypt.Visibility = Visibility.Collapsed;
            Decrypt.Visibility = Visibility.Collapsed;
            CopyNumber.Visibility = Visibility.Visible;
            if (file)
            {
                Folder_Remove.Visibility = Visibility.Visible;
                if (Global.Password != null)
                {
                    Encrypt.Visibility = Visibility.Visible;
                    Decrypt.Visibility = Visibility.Visible;
                }
            }
            if (h.id == null || h.id == "")
            {
                DownloadData.Visibility = Visibility.Collapsed;
                DownloadImage.Visibility = Visibility.Collapsed;
                CopyNumber.Visibility = Visibility.Collapsed;
            }
            Config cfg = new Config();
            JObject obj = cfg.Load();
            List<string> favs = cfg.ArrayValue<string>(Settings.favorites).ToList();
            if (favs.Contains(h.id))
            {
                Favorite.Visibility = Visibility.Collapsed;
                FavoriteRemove.Visibility = Visibility.Visible;
            }
            else
            {
                Favorite.Visibility = Visibility.Visible;
                FavoriteRemove.Visibility = Visibility.Collapsed;
            }
        }

        public override void ChangeColor()
        {
            panel.Background = new SolidColorBrush(Global.background);
            border.Background = new SolidColorBrush(Global.imagecolor);
            InfoPanel.Background = new SolidColorBrush(Global.Menuground);
            bottomPanel.Background = new SolidColorBrush(Global.Menuground);
            authorsPanel.Background = new SolidColorBrush(Global.Menuground);
            nameLabel.Foreground = new SolidColorBrush(Global.fontscolor);
            sizeLabel.Foreground = new SolidColorBrush(Global.fontscolor);
            pageLabel.Foreground = new SolidColorBrush(Global.fontscolor);
            sizeperpageLabel.Foreground = new SolidColorBrush(Global.fontscolor);
            //authorsPanel.Children.Cast<UIElement>().Select(x => (x as Label).Foreground = new SolidColorBrush(Global.artistsclr));
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

        public override async void HitomiPanel_Loaded(object sender, RoutedEventArgs e)
        {
            thumbNail.MouseEnter += (object sender2, MouseEventArgs e2) => thumbNail.ToolTip = GetToolTip(panel.Height);
            this.nameLabel.Content = h.name + " (로딩중)";
            InternetP parser = new InternetP();
            h = await parser.HitomiData2(h, int.Parse(h.id));
            if (Global.OriginThumb && h.files != null && h.files.Length >= 1 && h.files[0] != null)
            {
                this.nameLabel.Content = h.name + " (썸네일 로딩중)";
                ImageProcessor.ProcessEncryptAsync(h.files[0].url).then((BitmapImage image) =>
                {
                    h.thumbnail.preview_img = image;
                    thumbNail.Source = image;
                    thumbBrush.ImageSource = image;
                    this.nameLabel.Content = h.name;
                }, null, sourceName: MethodBase.GetCurrentMethod().FullName());
            }
            else
                this.nameLabel.Content = h.name;
            this.pageLabel.Content = h.files.Length;
            Init();
        }

        public override void Folder_Remove_Click(object sender, RoutedEventArgs e)
        {
            if (!file) return;
            //Global.MainWindow.MainPanel.Children.Remove(this);
            //Directory.Delete(h.dir, true);
        }
        public override void Folder_Open_Click(object sender, RoutedEventArgs e)
        {
            if (!file) return;
            //Process.Start(h.dir);
        }
        public override void CopyNumber_Click(object sender, RoutedEventArgs e) => Clipboard.SetText(h.id);
        public override void Hitomi_Download_Click(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                string filename = File2.GetDownloadTitle(File2.SaftyFileName(h.name));
                if (!Directory.Exists($"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}"))
                    Directory.CreateDirectory($"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}");
                Directory.CreateDirectory($"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}/{filename}");
                h.FileInfo.dir = $"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}/{filename}";
                h.Save($"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}/{filename}/info.json");
                for (int i = 0; i < h.files.Length; i++)
                {
                    string file = h.files[i].url;
                    WebClient wc = new WebClient();
                    wc.Headers.Add("referer", "https://hitomi.la/");
                    if (!File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}/{filename}/{i}.jpg"))
                    {
                        h.FileInfo.encrypted = Global.AutoFileEn;
                        if (Global.AutoFileEn)
                            FileEncrypt.DownloadAsync(wc, new Uri(file), $"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}/{filename}/{i}.jpg.lock");
                        else wc.DownloadFileAsync(new Uri(file), $"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}/{filename}/{i}.jpg");
                    }
                }
                Process.Start($"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}/{filename}");
            });
        }
        public override void Encrypt_Click(object sender, RoutedEventArgs e)
        {
            
        }
        public override void Decrypt_Click(object sender, RoutedEventArgs e)
        {

        }
        public override void Favorite_Click(object sender, RoutedEventArgs e)
        {
            
        }
        public override void FavoriteRemove_Click(object sender, RoutedEventArgs e)
        {
            
        }
        public override void authorLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }
        public override void DownloadData_Click(object sender, RoutedEventArgs e)
        {
            
        }
        public override void DownloadImage_Click(object sender, RoutedEventArgs e)
        {
            
        }
        public override async void ImageDownloadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            h.thumbnail.preview_url = h.files.First().url;
            h.thumbnail.preview_img = await ImageProcessor.ProcessEncryptAsync(h.thumbnail.preview_url).@catch(null, sourceName: MethodBase.GetCurrentMethod().FullName());
            Init();
        }
        public override void tagScroll_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scroll = Global.MainWindow.MainScroll;
            double offset = 0.45;
            if (e.Delta > 0)
                scroll.ScrollToVerticalOffset(scroll.VerticalOffset - (offset * e.Delta));
            else
                scroll.ScrollToVerticalOffset(scroll.VerticalOffset - (offset * e.Delta));
            e.Handled = true;
        }

        public override async void AtHitomi_Click(object sender, RoutedEventArgs e)
        {
            //HiyobiLoader hiyobi = new HiyobiLoader();
            //hiyobi.Default();
            //await hiyobi.Parser(hiyobiList.ToArray());
            HitomiLoader hitomi = new HitomiLoader();
            hitomi.Default();
            await hitomi.Parser(new int[] { int.Parse(h.id) });
        }
    }
}
#pragma warning restore CS0618 // 형식 또는 멤버는 사용되지 않습니다.