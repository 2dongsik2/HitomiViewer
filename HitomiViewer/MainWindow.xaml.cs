﻿using ExtensionMethods;
using HitomiViewer.Api;
using HitomiViewer.Encryption;
using HitomiViewer.Processor;
using HitomiViewer.Processor.Cache;
using HitomiViewer.Processor.Loaders;
using HitomiViewer.Plugin;
using HitomiViewer.Scripts;
using HitomiViewer.Structs;
using HitomiViewer.UserControls;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Path = System.IO.Path;

namespace HitomiViewer
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public enum FolderSorts
        {
            Name,
            Creation,
            LastWrite,
            Size,
            Pages,
            SizePerPage
        }

        public enum SearchType
        {
            normal,
            reversal
        }

        public static readonly string rootDir = AppDomain.CurrentDomain.BaseDirectory;
        public string path = string.Empty;
        public uint Page_itemCount => uint.Parse(((ComboBoxItem)Page_ItemCount.SelectedItem).Content.ToString());
        public int Page => Page_Index.SelectedIndex + 1;
        public SearchType searchType
        {
            get => SearchMode2.SelectedIndex switch
            {
                0 => SearchType.normal,
                1 => SearchType.reversal,
                _ => SearchType.normal,
            };
        }
        public List<IReader> Readers = new List<IReader>();
        public MainWindow()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(ResolveAssembly);
            PluginHandler.LoadPlugins();
            Global.dispatcher = Dispatcher;
            //new LoginClass().Run();
            //new Config().GetConfig().Save();
            CheckUpdate.Auto();
            HiyobiTags.LoadTags();
            Global.Setup();
            Account.Load();
            InitializeComponent();
            PluginHandler.FireOnInit(this);
            Init();
            PluginHandler.FireOnDelayInit(this);
            InitEvents();
        }

        public bool Argument()
        {
            bool relative = false;
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg == "/p" && args.Length - 1 > i)
                {
                    if (relative) path = Path.Combine(rootDir, args[i + 1]);
                    else path = args[i + 1];
                }
                if (arg == "/r") relative = true;
            }
            return relative;
        }
        public void Init()
        {
            this.MinWidth = 300;
            Global.MainWindow = this;
            bool relative = Argument();
            
            if (path == string.Empty) path = Path.Combine(rootDir, "hitomi_downloaded");
            if (relative) path = Path.Combine(rootDir, path);
            if (!Directory.Exists(path))
            {
                Console.WriteLine("Invaild Path");
                path = Path.Combine(rootDir, "hitomi_downloaded");
            }
            SearchMode1.SelectedIndex = 0;
            SetFolderSort(FolderSorts.Name);
            if (Global.Password == null)
            {
                Encrypt.Visibility = Visibility.Collapsed;
                Decrypt.Visibility = Visibility.Collapsed;
            }
        }
        public void InitEvents()
        {
            this.Loaded += MainWindow_Loaded;
        }
        public void DelayRegistEvents()
        {
            SearchMode1.SelectionChanged += SearchMode1_SelectionChanged;
            SearchMode2.SelectionChanged += SearchMode2_SelectionChanged;
            Page_Index.SelectionChanged += Page_Index_SelectionChanged;
            Page_ItemCount.SelectionChanged += Page_ItemCount_SelectionChanged;
        }
        private static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            Assembly thisAssembly = Assembly.GetExecutingAssembly();
            var name = args.Name.Substring(0, args.Name.IndexOf(',')) + ".dll";

            var resources = thisAssembly.GetManifestResourceNames().Where(s => s.EndsWith(name));
            if (resources.Count() > 0)
            {
                string resourceName = resources.First();
                using (Stream stream = thisAssembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        byte[] assembly = new byte[stream.Length];
                        stream.Read(assembly, 0, assembly.Length);
                        Console.WriteLine("Dll file load : " + resourceName);
                        return Assembly.Load(assembly);
                    }
                }
            }
            return null;
        }

        public void LoadHitomi(string path)
        {
            if (Global.DownloadFolder != "hitomi_downloaded")
                LoadHitomi(File2.GetDirectories(root: "", path, rootDir + Global.DownloadFolder));
            else
                LoadHitomi(Directory.GetDirectories(path));
        }
        public void LoadHitomi(string[] files)
        {
            /*
             Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => label.Visibility = Visibility.Hidden));
            if (files.Length <= 0)
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => label.Visibility = Visibility.Hidden));
                return;
            }
            string[] Folders = FolderSort(files);
            int i = 0;
            int SelectedPage = 1;
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                this.Background = new SolidColorBrush(Global.background);
                MainPanel.Children.Clear();
                if (SearchMode2.SelectedIndex == 1)
                    Folders = Folders.Reverse().ToArray();
                SelectedPage = Page_Index.SelectedIndex + 1;
                this.Title = string.Format("MainWindow - {0}페이지", SelectedPage);
            }));
            foreach (string folder in Folders.Where(x => Array.IndexOf(Folders, x) + 1 <= Page_itemCount * SelectedPage && Array.IndexOf(Folders, x) + 1 > (SelectedPage - 1) * Page_itemCount))
            {
                i++;
                Console.WriteLine("{0}: {1}", i, folder);
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".lock" };
                string[] innerFiles = Directory.GetFiles(folder).Where(file => allowedExtensions.Any(file.ToLower().EndsWith)).ToArray().ESort();
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    Hitomi h = new Hitomi
                    {
                        name = folder.Split(Path.DirectorySeparatorChar).Last(),
                        dir = folder,
                        page = innerFiles.Length,
                        files = innerFiles,
                        type = Hitomi.Type.Folder,
                        FolderByte = File2.GetFilesByte(innerFiles),
                        SizePerPage = File2.GetSizePerPage(folder, allowedExtensions)
                    };
                    if (innerFiles.Length <= 0)
                    {
                        h.thumb = ImageProcessor.FromResource("NoImage.jpg");
                        h.thumbpath = "";
                    }
                    else
                    {
                        h.thumb = ImageProcessor.ProcessEncrypt(innerFiles.First());
                        h.thumbpath = innerFiles.First();
                    }
                    if (h.thumb == null) return;
                    label.FontSize = 100;
                    label.Content = i + "/" + Page_itemCount;
                    MainPanel.Children.Add(new HitomiPanel(h, this, true));
                    Console.WriteLine("Completed: {0}", folder);
                }));
            }*/
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                label.Visibility = Visibility.Hidden;
                var pagination = new ILoader().SetSearch((int ind) =>
                {
                    MainPanel.Children.Clear();
                    LabelSetup();
                    Page_Index.SelectedIndex = ind - 1; //Page_Index_SelectionChanged 이벤트 호출
                }).SetPagination(Page);
                int pages = (int)Math.Ceiling(files.Length / ((double)Page_itemCount));
                pagination(pages);
            }));
        }

        public int GetPage() => (int) new CountBox("페이지", "원하는 페이지 수", 1).ShowDialog();
        public int GetPage(ref bool success)
        {
            CountBox countBox = new CountBox("페이지", "원하는 페이지 수", 1);
            int val = (int)countBox.ShowDialog();
            success = countBox.success;
            return val;
        }
        public void LabelSetup()
        {
            label.FlowDirection = FlowDirection.RightToLeft;
            label.Visibility = Visibility.Visible;
            label.FontSize = 100;
            label.Content = "로딩중";
            label.Margin = new Thickness(352 - label.Content.ToString().Length * 11, 240, 0, 0);
        }
        public void Searching(bool tf)
        {
            MainMenu.IsEnabled = !tf;
        }

        private void SetColor()
        {
            this.Background = new SolidColorBrush(Global.background);
            MainMenuBackground.Color = Global.Menuground;
            foreach (MenuItem menuItem in MainMenu.Items)
            {
                menuItem.Background = new SolidColorBrush(Global.MenuItmclr);
                menuItem.Foreground = new SolidColorBrush(Global.fontscolor);
                foreach (MenuItem item in menuItem.Items)
                    item.Foreground = new SolidColorBrush(Colors.Black);
            }
            foreach (IReader reader in Readers)
                reader.ChangeMode();
            foreach (UIElement hitomiPanel in MainPanel.Children)
            {
                if ((hitomiPanel as HitomiPanel) != null)
                    (hitomiPanel as HitomiPanel).ChangeColor();
            }
        }
        private void SetFolderSort(FolderSorts sorts)
        {
            switch (sorts)
            {
                case FolderSorts.Name:
                    Global.FolderSort = (string[] arr) =>
                    {
                        Dictionary<string, string> Match = new Dictionary<string, string>();
                        for (int i = 0; i < arr.Length; i++)
                        {
                            string item = arr[i];
                            if (File.Exists(Path.Combine(item, "info.json")))
                            {
                                string org = File.ReadAllText(Path.Combine(item, "info.json"));
                                if (!string.IsNullOrWhiteSpace(org))
                                {
                                    JObject jobject = JObject.Parse(org);
                                    string s = jobject.StringValue("name");
                                    if (s != null)
                                    {
                                        arr[i] = s;
                                        Match.Add(s, item);
                                    }
                                    else
                                        Match.Add(Path.GetFileName(item), item);
                                }
                                else
                                    Match.Add(Path.GetFileName(item), item);
                            }
                            else
                                Match.Add(Path.GetFileName(item), item);
                        }
                        return arr.StringSort().Select(x => Match.Keys.Contains(x) ? Match[x] : x).ToArray();
                    };
                    break;
                case FolderSorts.Creation:
                    Global.FolderSort = (string[] arr) =>
                    {
                        var arr2 = arr.Select(f => new FileInfo(f)).ToArray();
                        Array.Sort(arr2, delegate (FileInfo x, FileInfo y) { return DateTime.Compare(x.CreationTime, y.CreationTime); });
                        return arr2.Select(f => f.FullName).ToArray();
                    };
                    break;
                case FolderSorts.LastWrite:
                    Global.FolderSort = (string[] arr) =>
                    {
                        var arr2 = arr.Select(f => new FileInfo(f)).ToArray();
                        Array.Sort(arr2, delegate (FileInfo x, FileInfo y) { return DateTime.Compare(x.LastWriteTime, y.LastWriteTime); });
                        return arr2.Select(f => f.FullName).ToArray();
                    };
                    break;
                case FolderSorts.Size:
                    Global.FolderSort = (string[] arr) =>
                    {
                        var arr2 = arr.Select(f => new DirectoryInfo(f)).ToArray();
                        Array.Sort(arr2, delegate (DirectoryInfo x, DirectoryInfo y)
                        {
                            long xlen = x.EnumerateFiles().Sum(f => f.Length);
                            long ylen = y.EnumerateFiles().Sum(f => f.Length);
                            if (xlen == ylen) return 0;
                            if (xlen > ylen) return 1;
                            if (xlen < ylen) return -1;
                            return 0;
                        });
                        return arr2.Select(f => f.FullName).ToArray();
                    };
                    break;
                case FolderSorts.Pages:
                    Global.FolderSort = (string[] arr) =>
                    {
                        var arr2 = arr.ToArray();
                        Array.Sort(arr2, delegate (string x, string y)
                        {
                            long xlen = Directory.GetFiles(x).Length;
                            long ylen = Directory.GetFiles(y).Length;
                            if (xlen == ylen) return 0;
                            if (xlen > ylen) return 1;
                            if (xlen < ylen) return -1;
                            return 0;
                        });
                        return arr2.ToArray();
                    };
                    break;
                case FolderSorts.SizePerPage:
                    Global.FolderSort = (string[] arr) =>
                    {
                        var arr2 = arr.Select(f => new DirectoryInfo(f)).ToArray();
                        Array.Sort(arr2, delegate (DirectoryInfo x, DirectoryInfo y)
                        {
                            long xlen = x.EnumerateFiles().Sum(f => f.Length) / x.GetFiles().Length;
                            long ylen = y.EnumerateFiles().Sum(f => f.Length) / y.GetFiles().Length;
                            if (xlen == ylen) return 0;
                            if (xlen > ylen) return 1;
                            if (xlen < ylen) return -1;
                            return 0;
                        });
                        return arr2.Select(f => f.FullName).ToArray();
                    };
                    break;
            }
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            new Config().GetConfig().Save();
            LabelSetup();
            this.Background = new SolidColorBrush(Global.background);
            MainPanel.Children.Clear();
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            int pages = Directory.GetDirectories(path).Length / 25 + 1;
            for (int i = 0; i < pages; i++)
            {
                Page_Index.Items.Add(i + 1);
            }
            Page_Index.SelectedIndex = 0;
            Page_ItemCount.SelectedIndex = 3;
            SearchMode2.SelectedIndex = 0;
            DelayRegistEvents();
            new TaskFactory().StartNew(() => LoadHitomi(path));
        }
        private void MenuItem_Checked(object sender, RoutedEventArgs e)
        {
            Global.background = Colors.Black;
            Global.imagecolor = Color.FromRgb(116, 116, 116);
            Global.Menuground = Color.FromRgb(33, 33, 33);
            Global.MenuItmclr = Color.FromRgb(76, 76, 76);
            Global.panelcolor = Color.FromRgb(76, 76, 76);
            Global.fontscolor = Colors.White;
            Global.outlineclr = Colors.White;
            Global.artistsclr = Colors.SkyBlue;
            SetColor();
        }
        private void DarkMode_Unchecked(object sender, RoutedEventArgs e)
        {
            Global.background = Colors.White;
            Global.imagecolor = Colors.LightGray;
            Global.Menuground = Color.FromRgb(240, 240, 240);
            Global.MenuItmclr = Colors.White;
            Global.panelcolor = Colors.White;
            Global.fontscolor = Colors.Black;
            Global.outlineclr = Colors.Black;
            Global.artistsclr = Colors.Blue;
            SetColor();
        }
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F11)
            {
                //Normal
                if (WindowStyle == WindowStyle.None && WindowState == WindowState.Maximized)
                {
                    this.WindowStyle = WindowStyle.SingleBorderWindow;
                    this.WindowState = WindowState.Normal;
                }
                else if (WindowStyle == WindowStyle.SingleBorderWindow && WindowState == WindowState.Normal)
                {
                    this.WindowStyle = WindowStyle.None;
                    this.WindowState = WindowState.Maximized;
                }
                //Maximized
                else if (WindowStyle == WindowStyle.SingleBorderWindow && WindowState == WindowState.Maximized)
                {
                    this.WindowStyle = WindowStyle.None;
                    this.WindowState = WindowState.Normal;
                    this.WindowState = WindowState.Maximized;
                }
            }
            else if (e.Key == Key.Escape)
            {
                if (WindowStyle == WindowStyle.None && WindowState == WindowState.Maximized)
                {
                    this.WindowStyle = WindowStyle.SingleBorderWindow;
                    this.WindowState = WindowState.Normal;
                }
            }
        }
        private void SearchMode1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FolderSorts SortTypes = FolderSorts.Name;
            switch (SearchMode1.SelectedIndex)
            {
                case 0:
                    SortTypes = FolderSorts.Name;
                    break;
                case 1:
                    SortTypes = FolderSorts.Creation;
                    break;
                case 2:
                    SortTypes = FolderSorts.LastWrite;
                    break;
                case 3:
                    SortTypes = FolderSorts.Size;
                    break;
                case 4:
                    SortTypes = FolderSorts.Pages;
                    break;
                case 5:
                    SortTypes = FolderSorts.SizePerPage;
                    break;
            }
            SetFolderSort(SortTypes);
            new TaskFactory().StartNew(() => LoadHitomi(path));
        }
        private void SearchMode2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            new TaskFactory().StartNew(() => LoadHitomi(path));
        }
        private void Page_Index_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            new TaskFactory().StartNew(() => LoadHitomi(path));
        }
        private void Page_ItemCount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            new TaskFactory().StartNew(() => LoadHitomi(path));
        }
        private void LoadBtn_Click(object sender, RoutedEventArgs e)
        {
            new TaskFactory().StartNew(() => LoadHitomi(path));
        }
        private async void Search_Button_Click(object sender, RoutedEventArgs e)
        {
            string SearchText = Search_Text.Text;
            int number = 0;
            if (int.TryParse(SearchText, out number))
            {
                MainPanel.Children.Clear();
                InternetP parser = new InternetP(index: number);
                Tuple<bool, HiyobiGallery> data = await parser.isHiyobiData();
                bool result = data.Item1;
                HiyobiGallery h = data.Item2;
                if (!result)
                    MainPanel.Children.Add(new HitomiPanel(await parser.HitomiData(), true));
            }
            else File_Search_Button_Click(sender, e);
        }
        private void File_Search_Button_Click(object sender, RoutedEventArgs e)
        {
            string SearchText = Search_Text.Text;
            string[] files = File2.GetDirectories(root: "", path, rootDir + Global.DownloadFolder).Where(x => x.RemoveSpace().Contains(SearchText.RemoveSpace())).ToArray();
            new TaskFactory().StartNew(() => LoadHitomi(files));
        }
        private void Search_Text_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string SearchText = Search_Text.Text;
                string[] files = File2.GetDirectories(root: "", path, rootDir + Global.DownloadFolder).Where(x => x.RemoveSpace().Contains(SearchText.RemoveSpace())).ToArray();
                new TaskFactory().StartNew(() => LoadHitomi(files));
            }
        }
        private void OpenSetting_Click(object sender, RoutedEventArgs e)
        {
            new Settings().Show();
        }
        private async void FavoriteBtn_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Children.Clear();
            label.Visibility = Visibility.Visible;
            label.FontSize = 100;
            Config cfg = new Config();
            cfg.Load();
            List<string> favs = cfg.ArrayValue<string>(Settings.favorites).ToList();
            favs = favs.Where(x => Directory.Exists(x) || x.isUrl()).Distinct().ToList();
            InternetP parser = new InternetP();
            parser.start = (int count) => label.Content = "0/" + count;
            parser.update = (Hitomi h, int index, int max) =>
            {
                label.Content = $"{index}/{max}";
                MainPanel.Children.Add(new HitomiPanel(h));
            };
            parser.end = () => label.Visibility = Visibility.Collapsed;
            await parser.LoadCompre(favs);
            label.Visibility = Visibility.Collapsed;
        }
        private void Encrypt_Click(object sender, RoutedEventArgs e)
        {
            foreach (string item in File2.GetDirectories(root: "", path, rootDir + Global.DownloadFolder))
            {
                if (File.Exists($"{item}/info.json"))
                {
                    JObject j = JObject.Parse(File.ReadAllText($"{item}/info.json"));
                    j["encrypted"] = true;
                    File.WriteAllText($"{item}/info.json", j.ToString());
                }
                string[] files = Directory.GetFiles(item);
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
            }
            MessageBox.Show("전체 암호화 완료");
        }
        private void Decrypt_Click(object sender, RoutedEventArgs e)
        {
            foreach (string item in File2.GetDirectories(root: "", path, rootDir + Global.DownloadFolder))
            {
                if (File.Exists($"{path}/info.json"))
                {
                    JObject j = JObject.Parse(File.ReadAllText($"{path}/info.json"));
                    j["encrypted"] = false;
                    File.WriteAllText($"{path}/info.json", j.ToString());
                }
                string[] files = Directory.GetFiles(item);
                foreach (string file in files)
                {
                    try
                    {
                        byte[] org = File.ReadAllBytes(file);
                        byte[] enc = FileDecrypt.Default(org);
                        File.Delete(file);
                        File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file)), enc);
                    }
                    catch { }
                }
            }
            MessageBox.Show("전체 복호화 완료");
        }
        private async void ExportNumber_Click(object sender, RoutedEventArgs e)
        {
            /*
            JArray export = new JArray();
            foreach (string item in Directory.GetDirectories(path))
            {
                if (File.Exists($"{item}/info.json"))
                {
                    JObject j = JObject.Parse(File.ReadAllText($"{item}/info.json"));
                    JObject obj = new JObject();
                    obj["id"] = j["id"];
                    obj["type"] = int.Parse(j["type"].ToString());
                    export.Add(obj);
                }
                else if (File.Exists($"{item}/info.txt"))
                {
                    JObject obj = new JObject();
                    HitomiInfoOrg info = InfoLoader.parseTXT(File.ReadAllText($"{item}/info.txt"));
                    obj["id"] = info.Number;
                    bool result = await new InternetP(index: int.Parse(info.Number)).isHiyobi();
                    if (result) obj["type"] = (int)Hitomi.Type.Hiyobi;
                    else obj["type"] = (int)Hitomi.Type.Hitomi;
                    export.Add(obj);
                }
            }
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "번호 파일|*.json";
            if (!sfd.ShowDialog() ?? false) return;
            File.WriteAllText(sfd.FileName, export.ToString());
            */
        }
        private async void ImportNumber_Click(object sender, RoutedEventArgs e)
        {
            /*
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "번호 파일|*.json";
            if (!ofd.ShowDialog() ?? false) return;
            string s = ofd.FileName;
            try
            {
                MainPanel.Children.Clear();
                JArray arr = JArray.Parse(File.ReadAllText(s));
                List<string> hiyobiList = new List<string>();
                List<int> hitomiList = new List<int>();
                foreach (JToken item in arr)
                {
                    string id = item["id"].ToString();
                    Hitomi.Type type = (Hitomi.Type)int.Parse(item["type"].ToString());
                    if (type == Hitomi.Type.Hiyobi)
                        hiyobiList.Add(id);
                    else if (type == Hitomi.Type.Hitomi)
                        hitomiList.Add(int.Parse(id));
                }
                HiyobiLoader hiyobi = new HiyobiLoader();
                hiyobi.Default();
                await hiyobi.Parser(hiyobiList.ToArray());
                HitomiLoader hitomi = new HitomiLoader();
                hitomi.Default();
                await hitomi.Parser(hitomiList.ToArray());
            }
            catch
            {
                MessageBox.Show("잘못된 형식 입니다.");
            }
            */
        }
        private void CacheDownload_Click(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(new Cache().TagCache);
        }

        #region Hiyobi
        public void HiyobiMain(int index)
        {
            InternetP parser = new InternetP(url: "https://api.hiyobi.me/list/" + index);
            HiyobiLoader hiyobi = new HiyobiLoader();
            hiyobi.Default();
            hiyobi.pagination = hiyobi.SetSearch((int i) =>
            {
                MainPanel.Children.Clear();
                LabelSetup();
                HiyobiMain(i);
            }).SetPagination(index);
            parser.LoadJObject(hiyobi.Parser);
        }
        public void HiyobiSearch(List<string> keyword, int index)
        {
            
        }
        private void MenuHiyobi_Click(object sender, RoutedEventArgs e)
        {
            bool success = false;
            int page = GetPage(ref success);
            if (success)
            {
                MainPanel.Children.Clear();
                LabelSetup();
                HiyobiMain(page);
            }
        }
        private void Hiyobi_Search_Text_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) Task.Factory.StartNew(() =>
            {
                Thread.Sleep(500);
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => Hiyobi_Search_Button_Click(null, null)));
            });
        }
        public void Hiyobi_Search_Button_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Children.Clear();
            LabelSetup();
            HiyobiSearch(keyword: Hiyobi_Search_Text.Text.Split(' ').ToList(), index: GetPage());
        }
        #endregion

        #region Hitomi
        public void HitomiMain(int index)
        {
            HitomiLoader loader = new HitomiLoader();
            loader.index = index;
            loader.count = (int)Page_itemCount;
            loader.Default();
            loader.Parser();
        }
        public void HitomiSearch(string[] tags, int index)
        {
            SearchLoader loader = new SearchLoader();
            loader.tags = tags;
            loader.itemCount = (int)Page_itemCount;
            loader.index = index;
            loader.Default()
                  .HitomiSearch();
        }
        private void MenuHitomi_Click(object sender, RoutedEventArgs e)
        {
            bool success = false;
            int page = GetPage(ref success);
            if (success)
            {
                MainPanel.Children.Clear();
                LabelSetup();
                HitomiMain(page);
            }
        }
        private void Hitomi_Search_Text_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) Task.Factory.StartNew(() =>
            {
                Thread.Sleep(500);
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => Hitomi_Search_Button_Click(null, null)));
            });
        }
        public void Hitomi_Search_Button_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Children.Clear();
            LabelSetup();
            HitomiSearch(Hitomi_Search_Text.Text.Split(' '), GetPage());
        }
        #endregion

        #region Pixiv
        private async Task<bool> Login()
        {
            Login login = new Login();
            bool result = login.ShowDialog() ?? false;
            if (result)
            {
                if (login.remember)
                    Account.Save("pixiv", login.username, login.password);
                Pixiv pixiv = new Pixiv();
                Global.Account.Pixiv = await pixiv.AuthChain(login.username, login.password, true);
            }
            return result;
        }
        private async void PixivFollow_Click(object sender, RoutedEventArgs e)
        {
            /*
            if (Global.Account.Pixiv == null)
                if (await Login() == false)
                    return;
            MainPanel.Children.Clear();
            JObject data = await Global.Account.Pixiv.illustFollow();
            PixivLoader loader = new PixivLoader();
            loader.Default()
                .Parser(data);
            */
        }
        private async void PixivRecommend_Click(object sender, RoutedEventArgs e)
        {
            /*
            if (Global.Account.Pixiv == null)
                if (await Login() == false)
                    return;
            MainPanel.Children.Clear();
            JObject data = await Global.Account.Pixiv.illustRecommended();
            PixivLoader loader = new PixivLoader();
            loader.Default()
                .Parser(data);
            */
        }
        private void PixivUser_Search_Text_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) Task.Factory.StartNew(() =>
            {
                Thread.Sleep(500);
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => PixivUser_Search_Button_Click(null, null)));
            });
        }
        public async void PixivUser_Search_Button_Click(object sender, RoutedEventArgs e)
        {
            /*
            if (Global.Account.Pixiv == null)
                if (await Login() == false)
                    return;
            MainPanel.Children.Clear();
            JObject data = await Global.Account.Pixiv.searchUser(PixivUser_Search_Text.Text);
            PixivLoader loader = new PixivLoader();
            loader.UserDefault().UserParser(data);
            */
        }
        private void PixivIllust_Search_Text_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) Task.Factory.StartNew(() =>
            {
                Thread.Sleep(500);
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => PixivIllust_Search_Button_Click(null, null)));
            });
        }
        public async void PixivIllust_Search_Button_Click(object sender, RoutedEventArgs e)
        {
            /*
            if (Global.Account.Pixiv == null)
                if (await Login() == false)
                    return;
            MainPanel.Children.Clear();
            JObject data = await Global.Account.Pixiv.searchIllust(PixivIllust_Search_Text.Text);
            PixivLoader loader = new PixivLoader();
            loader.FastDefault().FastParser(data);
            */
        }
        #endregion
    }
}
