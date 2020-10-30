using ExtensionMethods;
using HitomiViewer.Encryption;
using HitomiViewer.Processor;
using HitomiViewer.Processor.Cache;
using HitomiViewer.Processor.Loaders;
using HitomiViewer.Plugin;
using HitomiViewer.Scripts;
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
using HitomiViewerLibrary.Loaders;
using HitomiViewerLibrary;
using HitomiViewerLibrary.Structs;
using HitomiViewer.UserControls.Panels;

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
            new Logger();
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(ResolveAssembly);
            PluginHandler.LoadPlugins();
            Global.dispatcher = Dispatcher;
            new LoginClass().Run();
            //new Config().GetConfig().Save();
            new Cache().Test();
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
            if (Global.config.download_folder.Get<string>() != "hitomi_downloaded")
                LoadHitomi(CF.File.GetDirectories(root: "", path, rootDir + Global.config.download_folder.Get<string>()));
            else
                LoadHitomi(Directory.GetDirectories(path));
        }
        public void LoadHitomi(string[] files)
        {
            label.Visibility = Visibility.Visible;
            FileLoader loader = new FileLoader();
            loader.Default();
            loader.Parser(files);
            label.Visibility = Visibility.Hidden;
            var pagination = new LoaderDefaults().SetSearch((int ind) =>
            {
                MainPanel.Children.Clear();
                LabelSetup();
                Page_Index.SelectedIndex = ind - 1; //Page_Index_SelectionChanged 이벤트 호출
            }).SetPagination(Page);
            int pages = (int)Math.Ceiling(files.Length / ((double)Page_itemCount));
            pagination(pages);
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
                if ((hitomiPanel as IHitomiPanel) != null)
                    (hitomiPanel as IHitomiPanel).ChangeColor();
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

        #region Events
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            new ConfigFile().GetConfig().Save();
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
            LoadHitomi(path);
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
            LoadHitomi(path);
        }
        private void SearchMode2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadHitomi(path);
        }
        private void Page_Index_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadHitomi(path);
        }
        private void Page_ItemCount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadHitomi(path);
        }
        private void LoadBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadHitomi(path);
        }
        private void File_Search_Button_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Children.Clear();
            string SearchText = Search_Text.Text;
            List<string> dirs = new string[] { path }.ToList();
            string configPath = rootDir + Global.config.download_folder.Get<string>();
            if (!dirs.Contains(configPath))
                dirs.Add(configPath);
            string[] files = CF.File.GetDirectories(root: "", dirs.ToArray()).Where(x => x.RemoveSpace().Contains(SearchText.RemoveSpace())).ToArray();
            LoadHitomi(files);
        }
        private void Search_Text_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) Task.Factory.StartNew(() =>
            {
                Thread.Sleep(500);
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => File_Search_Button_Click(null, null)));
            });
        }
        private void OpenSetting_Click(object sender, RoutedEventArgs e)
        {
            new Settings().ShowDialog();
        }
        private void FavoriteBtn_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Encrypt_Click(object sender, RoutedEventArgs e)
        {
            
        }
        private void Decrypt_Click(object sender, RoutedEventArgs e)
        {
            
        }
        private void ExportNumber_Click(object sender, RoutedEventArgs e)
        {
            
        }
        private void ImportNumber_Click(object sender, RoutedEventArgs e)
        {
            
        }
        private void CacheDownload_Click(object sender, RoutedEventArgs e)
        {
            
        }
        #endregion

        #region Hiyobi
        public void HiyobiMain(int index)
        {
            InternetP parser = new InternetP(url: "https://api.hiyobi.me/list/" + index);
            HiyobiLoader hiyobi = new HiyobiLoader();
            hiyobi.Default();
            LoaderDefaults.Hiyobis.Setup(hiyobi);
            hiyobi.pagination = new LoaderDefaults().SetSearch((int i) =>
            {
                MainPanel.Children.Clear();
                LabelSetup();
                HiyobiMain(i);
            }).SetPagination(index);
            parser.LoadJObject(hiyobi.Parser); //err catch
        }
        public void HiyobiSearch(List<string> keyword, int index)
        {
            InternetP parser = new InternetP();
            HiyobiLoader hiyobi = new HiyobiLoader();
            hiyobi.Default();
            LoaderDefaults.Hiyobis.Setup(hiyobi);
            hiyobi.pagination = new LoaderDefaults().SetSearch((int i) =>
            {
                MainPanel.Children.Clear();
                LabelSetup();
                HiyobiSearch(keyword, i);
            }).SetPagination(index);
            parser.HiyobiSearch<JObject>(index, keyword).TaskCallback(hiyobi.Parser);
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
            LoaderDefaults.Hitomis.Setup(loader);
            loader.pagination = new LoaderDefaults().SetSearch((int i) =>
            {
                MainPanel.Children.Clear();
                LabelSetup();
                HitomiMain(i);
            }).SetPagination(index);
            loader.Parser();
        }
        public void HitomiSearch(string[] tags, int index)
        {
            SearchLoader loader = new SearchLoader();
            loader.tags = tags;
            loader.itemCount = (int)Page_itemCount;
            loader.index = index;
            loader.pagination = new LoaderDefaults().SetSearch((int i) =>
            {
                MainPanel.Children.Clear();
                LabelSetup();
                HitomiMain(i);
            }).SetPagination(index);
            loader.DefaultChain().HitomiSearch();
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
                await pixiv.Auth(login.username, login.password, true);
                if (!pixiv.authStatus)
                {
                    MessageBox.Show("로그인에 실패했습니다.");
                    return false;
                }
                else
                    Global.Account.Pixiv = pixiv;
            }
            return result;
        }
        private async void FollowIllust()
        {
            if (Global.Account.Pixiv == null)
                if (await Login() == false)
                    return;
            JObject data = await Global.Account.Pixiv.illustFollow();
            PixivLoaders.Illust loader = new PixivLoaders.Illust();
            loader.Default();
            LoaderDefaults.Pixivs.Setup(loader);
            loader.Parser(data);
        }
        private async void RecommendIllust()
        {
            if (Global.Account.Pixiv == null)
                if (await Login() == false)
                    return;
            JObject data = await Global.Account.Pixiv.illustRecommended();
            PixivLoaders.Illust loader = new PixivLoaders.Illust();
            loader.Default();
            LoaderDefaults.Pixivs.Setup(loader);
            loader.Parser(data);
        }
        private async void UserSearch()
        {
            if (Global.Account.Pixiv == null)
                if (await Login() == false)
                    return;
            JObject data = await Global.Account.Pixiv.searchUser(PixivUser_Search_Text.Text);
            PixivLoaders.User loader = new PixivLoaders.User();
            loader.Default();
            LoaderDefaults.Pixivs.Setup(loader);
            loader.Parser(data);
        }
        private async void IllustSearch()
        {
            JObject data = await Global.Account.Pixiv.searchIllust(PixivIllust_Search_Text.Text);
            PixivLoaders.Illust loader = new PixivLoaders.Illust();
            loader.Default();
            LoaderDefaults.Pixivs.Setup(loader);
            loader.Parser(data);
        }

        #region KeyDown
        private void PixivUser_Search_Text_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) Task.Factory.StartNew(() =>
            {
                Thread.Sleep(500);
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => PixivUser_Search_Button_Click(null, null)));
            });
        }
        private void PixivIllust_Search_Text_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) Task.Factory.StartNew(() =>
            {
                Thread.Sleep(500);
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => PixivIllust_Search_Button_Click(null, null)));
            });
        }
        #endregion
        #region Click
        private async void PixivFollowIllust_Click(object sender, RoutedEventArgs e)
        {
            if (Global.Account.Pixiv == null)
                if (await Login() == false)
                    return;
            MainPanel.Children.Clear();
            FollowIllust();
        }
        private async void PixivRecommendIllust_Click(object sender, RoutedEventArgs e)
        {
            if (Global.Account.Pixiv == null)
                if (await Login() == false)
                    return;
            MainPanel.Children.Clear();
            RecommendIllust();
        }
        public async void PixivUser_Search_Button_Click(object sender, RoutedEventArgs e)
        {
            if (Global.Account.Pixiv == null)
                if (await Login() == false)
                    return;
            MainPanel.Children.Clear();
            UserSearch();
        }
        public async void PixivIllust_Search_Button_Click(object sender, RoutedEventArgs e)
        {
            if (Global.Account.Pixiv == null)
                if (await Login() == false)
                    return;
            MainPanel.Children.Clear();
            IllustSearch();
        }
        #endregion

        #endregion
    }
}
