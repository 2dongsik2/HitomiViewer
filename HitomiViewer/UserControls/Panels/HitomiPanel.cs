using ExtensionMethods;
using HitomiViewer.Api;
using HitomiViewer.Encryption;
using HitomiViewer.Plugin;
using HitomiViewer.Processor;
using HitomiViewer.Processor.Loaders;
using HitomiViewer.Scripts;
using HitomiViewer.Structs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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
        public HitomiPanel(Hitomi h, bool large = true, bool file = false, bool blur = false)
        {
            this.large = large;
            this.file = file;
            this.blur = blur;
            this.h = h;
            InitializeComponent();
            Init();
            InitEvent();
        }

        public override void InitEvent()
        {
            thumbNail.MouseDown += (object sender, MouseButtonEventArgs e) =>
            {
                Reader reader = new Reader(h);
                if (!reader.IsClosed)
                    reader.Show();
            };
            /*
            if ((h.type != Hitomi.Type.Pixiv && h.type != Hitomi.Type.Folder) || (!afterLoad && h.ugoiraImage == null))
                thumbNail.MouseEnter += (object sender2, MouseEventArgs e2) => thumbNail.ToolTip = GetToolTip(panel.Height);
            else if (h.type == Hitomi.Type.Folder)
            {
                if (ftype != Hitomi.Type.Pixiv || h.ugoiraImage == null)
                    thumbNail.MouseEnter += (object sender2, MouseEventArgs e2) => thumbNail.ToolTip = GetToolTip(panel.Height);
            }
            */
        }
        public override async void Init()
        {
            PluginHandler.FireOnHitomiPanelInit(this);
            if (h.thumbnail.preview_img == null)
            {
                if (h.thumbnail.preview_url == null)
                    h.thumbnail.preview_img = ImageProcessor.FromResource("NoImage.jpg");
                else
                    h.thumbnail.preview_img = await ImageProcessor.ProcessEncryptAsync(h.thumbnail.preview_url);
            }
            panel.Height = 100;
            thumbNail.Source = h.thumbnail.preview_img;
            thumbBrush.ImageSource = h.thumbnail.preview_img;

            authorsPanel.Children.Clear();
            authorsPanel.Children.Add(new Label { Content = "작가 :" });
            tagPanel.Children.Clear();
            if (blur)
                thumbNail.BitmapEffect = new BlurBitmapEffect { Radius = 5, KernelType = KernelType.Gaussian };
            Config config = new Config();
            config.Load();
            if (config.ArrayValue<string>(Settings.except_tags).Any(x => h.tags.Select(y => y.full).Contains(x)))
            {
                if (config.BoolValue(Settings.block_tags) ?? false)
                {
                    Global.MainWindow.MainPanel.Children.Remove(this);
                    return;
                }
                else
                    thumbNail.BitmapEffect = new BlurBitmapEffect { Radius = 5, KernelType = KernelType.Gaussian };
            }

            pageLabel.Content = h.files.Length + "p";

            int GB = 1024 * 1024 * 1024;
            int MB = 1024 * 1024;
            int KB = 1024;
            double FolderByte = h.FolderByte;
            sizeLabel.Content = Math.Round(FolderByte, 2) + "B";
            if (FolderByte > KB)
                sizeLabel.Content = Math.Round(FolderByte / KB, 2) + "KB";
            if (FolderByte > MB)
                sizeLabel.Content = Math.Round(FolderByte / MB, 2) + "MB";
            if (FolderByte > GB)
                sizeLabel.Content = Math.Round(FolderByte / GB, 2) + "GB";

            double SizePerPage = h.SizePerPage;
            sizeperpageLabel.Content = Math.Round(SizePerPage, 2) + "B";
            if (SizePerPage > KB)
                sizeperpageLabel.Content = Math.Round(SizePerPage / KB, 2) + "KB";
            if (SizePerPage > MB)
                sizeperpageLabel.Content = Math.Round(SizePerPage / MB, 2) + "MB";
            if (SizePerPage > GB)
                sizeperpageLabel.Content = Math.Round(SizePerPage / GB, 2) + "GB";


            foreach (Hitomi.HTag tag in h.tags)
            {
                switch (tag.types)
                {
                    case Structs.Tag.Types.female:
                        tag1.TagColor = new SolidColorBrush(Color.FromRgb(255, 94, 94));
                        break;
                    case Structs.Tag.Types.male:
                        tag1.TagColor = new SolidColorBrush(Color.FromRgb(65, 149, 244));
                        break;
                    //case Structs.Tag.Types.tag:
                    default:
                        tag1.TagColor = new SolidColorBrush(Color.FromRgb(153, 153, 153));
                        break;
                }
                tagPanel.Children.Add(tag1);
            }

            if (large)
            {
                panel.Height += 50;
                if (h.authors != null)
                {
                    authorsStackPanel.Visibility = Visibility.Visible;
                    foreach (string artist in h.authors)
                    {
                        if (h.authors.Get<List<string>>().IndexOf(artist) != 0)
                        {
                            Label dot = new Label();
                            dot.Content = ", ";
                            dot.Padding = new Thickness(0, 5, 2.5, 5);
                            authorsPanel.Children.Add(dot);
                        }
                        Label lb = new Label();
                        lb.Content = artist;
                        lb.Foreground = new SolidColorBrush(Global.artistsclr);
                        lb.Cursor = Cursors.Hand;
                        lb.MouseDown += authorLabel_MouseDown;
                        lb.Padding = new Thickness(0, 5, 0, 5);
                        authorsPanel.Children.Add(lb);
                    }
                }
            }

            thumbNail.ToolTip = GetToolTip(panel.Height);
            nameLabel.Width = panel.Width - border.Width;
            nameLabel.Content = h.name;
            ContextSetup();
            ChangeColor();
            PluginHandler.FireOnHitomiPanelDelayInit(this);
        }

        public void ContextSetup()
        {
            Favorite.Visibility = Visibility.Visible;
            FavoriteRemove.Visibility = Visibility.Collapsed;
            Folder_Remove.Visibility = Visibility.Collapsed;
            Folder_Hiyobi_Search.Visibility = Visibility.Collapsed;
            Hiyobi_Download.Visibility = Visibility.Collapsed;
            Hitomi_Download.Visibility = Visibility.Visible;
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
        }

        public override void ChangeColor()
        {
            PluginHandler.FireOnHitomiChangeColor(this);
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
            this.nameLabel.Content = h.name + " (로딩중)";
            if (h.type == Hitomi.Type.Hiyobi)
            {
                h.files = new string[0];
                InternetP parser = new InternetP(index: int.Parse(h.id));
                this.h.files = (await parser.HiyobiFiles()).Select(x => x.url).ToArray();
                this.h.page = h.files.Length;
            }
            if (h.type == Hitomi.Type.Hitomi)
            {
                InternetP parser = new InternetP();
                parser.url = $"https://ltn.hitomi.la/galleries/{h.id}.js";
                JObject info = await parser.HitomiGalleryInfo();
                h.type = Hitomi.Type.Hitomi;
                h.tags = parser.HitomiTags(info);
                h.files = parser.HitomiFiles(info).ToArray();
                h.page = h.files.Length;
                h.Json = info;
                h = await parser.HitomiGalleryData(h);
                if (!(Global.OriginThumb && h.files != null && h.files[0] != null))
                    h.thumbnail.preview_img = await ImageProcessor.ProcessEncryptAsync(h.thumbnail.preview_url.https());
            }
            if (h.type == Hitomi.Type.Pixiv)
            {
                try
                {
                    Pixiv pixiv = Global.Account.Pixiv;
                    JObject obj = await pixiv.ugoiraMetaData(h.id);
                    WebClient wc = new WebClient();
                    wc.Headers.Add("Referer", "https://www.pixiv.net/");
                    byte[] zipbyte = await wc.DownloadDataTaskAsync(obj["ugoira_metadata"]["zip_urls"].StringValue("medium"));
                    h.ugoiraImage = pixiv.UnZip(zipbyte);
                    h.ugoiraImage.original = obj["ugoira_metadata"]["zip_urls"].StringValue("medium");
                    h.ugoiraImage.delays = obj["ugoira_metadata"]["frames"].Select(x => x.IntValue("delay") ?? 0).ToList();
                    h.name += " (우고이라)";
                }
                catch { }
            }
            if (h.ugoiraImage == null)
            {
                thumbNail.ToolTip = GetToolTip(panel.Height);
                thumbNail.MouseEnter += (object sender2, MouseEventArgs e2) => thumbNail.ToolTip = GetToolTip(panel.Height);
            }
            if (Global.OriginThumb &&
                h.files != null &&
                h.files.Length >= 1 &&
                h.files[0] != null &&
                h.type != Hitomi.Type.Pixiv)
            {
                this.nameLabel.Content = h.name + " (썸네일 로딩중)";
                ImageProcessor.ProcessEncryptAsync(h.files[0]).then((BitmapImage image) =>
                {
                    h.thumbnail.preview_img = image;
                    this.nameLabel.Content = h.name;
                    if (h.type != Hitomi.Type.Hiyobi)
                        thumbNail.Source = image;
                });
            }
            else
                this.nameLabel.Content = h.name;
            Init();
            using (Config config = new Config())
            {
                config.Load();
                if (config.ArrayValue<string>(Settings.except_tags).Any(x => h.tags.Select(y => y.full).Contains(x.Replace("_", " "))))
                {
                    if (config.BoolValue(Settings.block_tags) ?? false)
                        MainWindow.MainPanel.Children.Remove(this);
                    else
                        thumbNail.BitmapEffect = new BlurBitmapEffect { Radius = 5, KernelType = KernelType.Gaussian };
                }
            }
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
                h.dir = $"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}/{filename}";
                h.Save($"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}/{filename}/info.json");
                for (int i = 0; i < h.files.Length; i++)
                {
                    string file = h.files[i].url;
                    WebClient wc = new WebClient();
                    wc.Headers.Add("referer", "https://hitomi.la/");
                    if (!File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}/{filename}/{i}.jpg"))
                    {
                        h.encrypted = Global.AutoFileEn;
                        if (Global.AutoFileEn)
                            FileEncrypt.DownloadAsync(wc, new Uri(file), $"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}/{filename}/{i}.jpg.lock");
                        else wc.DownloadFileAsync(new Uri(file), $"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}/{filename}/{i}.jpg");
                    }
                }
                Process.Start($"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}/{filename}");
            });
        }
        public override void Pixiv_Download_Click(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                string filename = File2.GetDownloadTitle(File2.SaftyFileName(h.name));
                if (!Directory.Exists($"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}"))
                    Directory.CreateDirectory($"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}");
                Directory.CreateDirectory($"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}/{filename}");
                h.dir = $"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}/{filename}";
                this.Dispatcher.Invoke(() => h.Save($"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}/{filename}/info.json"));
                for (int i = 0; i < h.files.Length; i++)
                {
                    string file = h.files[i];
                    WebClient wc = new WebClient();
                    wc.Headers.Add("Referer", "https://www.pixiv.net/");
                    wc.DownloadFileAsync(new Uri(h.ugoiraImage.original), $"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}/{filename}/ugoira_metadata.zip");
                    if (!File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}/{filename}/{i}.jpg"))
                    {
                        h.encrypted = Global.AutoFileEn;
                        if (Global.AutoFileEn)
                            FileEncrypt.DownloadAsync(wc, new Uri(file), $"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}/{filename}/{i}.jpg.lock");
                        else wc.DownloadFileAsync(new Uri(file), $"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}/{filename}/{i}.jpg");
                    }
                }
                Process.Start($"{AppDomain.CurrentDomain.BaseDirectory}/{Global.DownloadFolder}/{filename}");
            });
        }
        public override void Folder_Hiyobi_Search_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.MainPanel.Children.Clear();
            MainWindow.label.Visibility = Visibility.Visible;
            MainWindow.label.FontSize = 100;
            InternetP parser = new InternetP();
            parser.keyword = h.name.Replace("｜", "|").Split(' ').ToList();
            parser.index = 1;
            parser.HiyobiSearch(data => new InternetP(data: data).ParseJObject(async jobject =>
            {
                MainWindow.label.Content = 0 + "/" + jobject["list"].ToList().Count;
                foreach (JToken tk in jobject["list"])
                {
                    MainWindow.label.Content = jobject["list"].ToList().IndexOf(tk) + "/" + jobject["list"].ToList().Count;
                    parser = new InternetP();
                    parser.url = $"https://cdn.hiyobi.me/data/json/{tk["id"]}_list.json";
                    JArray imgs = await parser.TryLoadJArray();
                    if (imgs == null) continue;
                    Hiyobi h = new Hiyobi();
                    h.id = tk["id"].ToString();
                    h.name = tk["title"].ToString();
                    h.url = $"https://hiyobi.me/reader/{tk["id"]}";
                    h.thumbnail = $"https://cdn.hiyobi.me/tn/{tk["id"]}.jpg";
                    h.thumb = await ImageProcessor.LoadWebImageAsync($"https://cdn.hiyobi.me/tn/{tk["id"]}.jpg");
                    h.files = imgs.ToList().Select(x => $"https://cdn.hiyobi.me/data/{tk["id"]}/{x["name"]}").ToArray();
                    foreach (JToken tags in tk["tags"])
                    {
                        Tag tag = new Tag();
                        if (tags["value"].ToString().Contains(":"))
                        {
                            tag.types = (Tag.Types)Enum.Parse(typeof(Tag.Types), tags["value"].ToString().Split(':')[0]);
                            tag.name = tags["display"].ToString();
                        }
                        else
                        {
                            tag.types = Structs.Tag.Types.tag;
                            tag.name = tags["display"].ToString();
                        }
                        h.tags.Add(tag);
                    }
                    Global.MainWindow.MainPanel.Children.Add(new HitomiPanel(h));
                    Console.WriteLine($"Completed: https://cdn.hiyobi.me/tn/{tk["id"]}.jpg");
                }
                Global.MainWindow.label.Visibility = Visibility.Hidden;
            }));
        }
        public override void Encrypt_Click(object sender, RoutedEventArgs e)
        {
            string[] files = Directory.GetFiles(h.dir);
            foreach (string file in files)
            {
                if (Path.GetFileName(file) == "info.json") continue;
                if (Path.GetFileName(file) == "info.txt") continue;
                if (Path.GetExtension(file) == ".lock") continue;
                byte[] org = File.ReadAllBytes(file);
                byte[] enc = FileEncrypt.Default(org);
                File.Delete(file);
                File.WriteAllBytes(file + ".lock", enc);
            }
            h.files = h.files.Select(x => x + ".lock").ToArray();
            h.encrypted = true;
            Process.Start(h.dir);
        }
        public override void Decrypt_Click(object sender, RoutedEventArgs e)
        {
            bool err = FileDecrypt.TryFiles(h.dir);
            if (err)
            {
                while (true)
                {
                    InputBox box = new InputBox("비밀번호를 입력해주세요.", "비밀번호가 맞지 않습니다.", "");
                    string password = box.ShowDialog();
                    if (box.canceled) return;
                    if (FileDecrypt.TryFiles(h.dir, SHA256.Hash(password), excepts: new string[] { ".txt", ".json" })) break;
                }
            }
            h.thumbnail.preview_img = ImageProcessor.ProcessEncrypt(File2.GetImages(h.dir).First());
            h.files = h.files.Select(x => Path.Combine(Path.GetDirectoryName(x), Path.GetFileNameWithoutExtension(x))).ToArray();
            h.encrypted = false;
            thumbNail.Source = h.thumbnail.preview_img;
            thumbNail.ToolTip = GetToolTip(panel.Height);
            Process.Start(h.dir);
        }
        public override void Favorite_Click(object sender, RoutedEventArgs e)
        {
            Config cfg = new Config();
            JObject obj = cfg.Load();
            List<string> favs = cfg.ArrayValue<string>("fav").ToList();
            if (!favs.Contains(h.dir))
                favs.Add(h.dir);
            favs = favs.Where(x => Directory.Exists(x) || x.isUrl()).Distinct().ToList();
            obj["fav"] = JToken.FromObject(favs);
            cfg.Save(obj);
            ContextSetup();
        }
        public override void FavoriteRemove_Click(object sender, RoutedEventArgs e)
        {
            Config cfg = new Config();
            JObject obj = cfg.Load();
            List<string> favs = cfg.ArrayValue<string>("fav").ToList();
            if (favs.Contains(h.dir))
                favs.Remove(h.dir);
            favs = favs.Where(x => Directory.Exists(x) || x.isUrl()).Distinct().ToList();
            obj["fav"] = JToken.FromObject(favs);
            cfg.Save(obj);
            ContextSetup();
        }
        private async void authorLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Label lbsender = sender as Label;
            string author = lbsender.Content.ToString().Replace(" ", "_");
            if (h.type == Hitomi.Type.Pixiv || h.type == Hitomi.Type.PixivUgoira)
            {
                MainWindow.PixivUser_Search_Text.Text = author;
                MainWindow.PixivUser_Search_Button_Click(this, null);
            }
            else if (h.type == Hitomi.Type.Hiyobi || h.type == Hitomi.Type.Hitomi)
            {
                bool result = await new InternetP(index: int.Parse(h.id)).isHiyobi();
                MainWindow.MainPanel.Children.Clear();
                if (result)
                {
                    Global.MainWindow.Hiyobi_Search_Text.Text = "artist:" + author;
                    Global.MainWindow.Hiyobi_Search_Button_Click(this, null);
                }
                else
                {
                    Global.MainWindow.Hitomi_Search_Text.Text = "artist:" + author;
                    Global.MainWindow.Hitomi_Search_Button_Click(this, null);
                }
            }
        }
        private async void DownloadData_Click(object sender, RoutedEventArgs e)
        {
            bool hiyobi = ftype == Hitomi.Type.Hiyobi;
            if (ftype == Hitomi.Type.Folder)
            {
                bool result = await new InternetP(index: int.Parse(h.id)).isHiyobi();
                hiyobi = result;
            }
            if (hiyobi)
            {
                Hitomi h2 = await new HiyobiLoader(text: h.id).Parser();
                if (h2 == null || (h2.name == "정보없음" && h2.id == "0"))
                {
                    MessageBox.Show("데이터를 받아오는데 실패했습니다.");
                    return;
                }
                string folder = h.dir;
                if (h.type != Hitomi.Type.Folder)
                    folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Global.DownloadFolder, File2.GetDownloadTitle(File2.SaftyFileName(h2.name)));
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                File.WriteAllText(Path.Combine(folder, "info.json"), JObject.FromObject(h2).ToString());
                MessageBox.Show("데이터를 성공적으로 받았습니다.");
                authorsPanel.Children.Clear();
                authorsPanel.Children.Add(new Label { Content = "작가 :" });
                tagPanel.Children.Clear();
                Init();
            }
            if (!hiyobi)
            {
                InternetP parser = new InternetP();
                parser.index = int.Parse(h.id);
                Hitomi h2 = await parser.HitomiData2();
                string folder = h.dir;
                if (h.type != Hitomi.Type.Folder)
                    folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Global.DownloadFolder, File2.GetDownloadTitle(File2.SaftyFileName(h2.name)));
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                File.WriteAllText(Path.Combine(folder, "info.json"), JObject.FromObject(h2).ToString());
                MessageBox.Show("데이터를 성공적으로 받았습니다.");
                authorsPanel.Children.Clear();
                authorsPanel.Children.Add(new Label { Content = "작가 :" });
                tagPanel.Children.Clear();
                Init();
            }
        }
        private async void DownloadImage_Click(object sender, RoutedEventArgs e)
        {
            bool hiyobi = ftype == Hitomi.Type.Hiyobi;
            if (ftype == Hitomi.Type.Folder)
            {
                bool result = await new InternetP(index: int.Parse(h.id)).isHiyobi();
                hiyobi = result;
            }
            Hitomi h2 = null;
            if (hiyobi)
                h2 = await new HiyobiLoader(text: h.id).Parser();
            if (!hiyobi)
                h2 = await new InternetP(index: int.Parse(h.id)).HitomiData2();
            if (h2 == null) return;
            string folder = h.type == Hitomi.Type.Folder ? h.dir : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Global.DownloadFolder, File2.GetDownloadTitle(File2.SaftyFileName(h2.name)));
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder); h = h2;
            File.WriteAllText(Path.Combine(folder, "info.json"), JObject.FromObject(h2).ToString());
            foreach (string file in Directory.GetFiles(folder))
            {
                if (file.EndsWith(".lock") || file.EndsWith(".jpg"))
                    File.Delete(file);
            }
            for (int i = 0; i < h2.files.Length; i++)
            {
                string file = h2.files[i];
                WebClient wc = new WebClient();
                if (!hiyobi) wc.Headers.Add("referer", "https://hitomi.la/");
                h.encrypted = Global.AutoFileEn;
                if (Global.AutoFileEn)
                    FileEncrypt.DownloadAsync(new Uri(file), $"{folder}/{i}.jpg.lock", !hiyobi);
                else
                    wc.DownloadFileAsync(new Uri(file), $"{folder}/{i}.jpg");
                if (i == 0) wc.DownloadFileCompleted += ImageDownloadCompleted;
            }
            Process.Start(folder);
        }
        public override async void ImageDownloadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            h.thumbnail.preview_url = h.files.First().url;
            h.thumbnail.preview_img = await ImageProcessor.ProcessEncryptAsync(h.thumbnail.preview_url);
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
        public override void thumbNail_MouseEnter(object sender, MouseEventArgs e)
        {
            if (h.ugoiraImage != null)
                Task.Factory.StartNew(() =>
                    Ugoira(this.Dispatcher));
        }
        public override void Ugoira(Dispatcher dispatcher)
        {
            if (this.Visibility != Visibility.Visible)
                return;
            if (!thumbNail.IsMouseOver)
                return;
            //thumbNail.ToolTip = null;
            dispatcher.Invoke(() =>
            {
                if (h.ugoiraImage.index >= h.ugoiraImage.bytesofimages.Count)
                    h.ugoiraImage.index = 0;
                this.thumbNail.Source = ImageProcessor.Bytes2Image2(h.ugoiraImage.bytesofimages[h.ugoiraImage.index]);
            });
            Thread.Sleep(h.ugoiraImage.delays[h.ugoiraImage.index++]);
            Ugoira(dispatcher);
        }

        private async void AtHitomi_Click(object sender, RoutedEventArgs e)
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