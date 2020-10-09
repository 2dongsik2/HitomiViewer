using HitomiViewer.UserControls;
using HitomiViewerLibrary;
using HitomiViewerLibrary.Loaders;
using HitomiViewerLibrary.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HitomiViewer.Processor.Loaders
{
    public class LoaderDefaults
    {
        public Action<int> search = null;
        public static void Start(int count)
        {
            Global.MainWindow.label.Content = "0/" + count;
            Global.MainWindow.label.Visibility = System.Windows.Visibility.Visible;
            Global.MainWindow.Searching(true);
        }
        public static void End()
        {
            Global.MainWindow.label.Visibility = System.Windows.Visibility.Collapsed;
            Global.MainWindow.Searching(false);
        }

        public class Hiyobis
        {
            public static void Setup(HiyobiLoader loader)
            {
                loader.start = Start;
                loader.update = Update;
                loader.end = End;
            }
            public static void Update(HiyobiGallery h, int index, int max)
            {
                Global.MainWindow.label.Content = $"{index}/{max}";
                Global.MainWindow.MainPanel.Children.Add(new UserControls.Panels.HiyobiPanel(h));
            }
        }

        public class Hitomis
        {
            public static void Setup(HitomiLoader loader)
            {
                loader.start = Start;
                loader.update = Update;
                loader.end = End;
            }
            public static void Update(Hitomi h, int index, int max)
            {
                Global.MainWindow.label.Content = $"{index}/{max}";
                Global.MainWindow.MainPanel.Children.Add(new UserControls.Panels.HitomiPanel(h));
            }
        }

        public class Pixivs
        {
            #region Illust
            public static void Setup(PixivLoaders.Illust loader)
            {
                loader.start = Start;
                loader.update = IllustUpdate;
                loader.end = End;
            }
            public static void IllustUpdate(Pixiv.Illust h, int index, int max)
            {
                Global.MainWindow.label.Content = $"{index}/{max}";
                Global.MainWindow.MainPanel.Children.Add(new UserControls.Panels.PixivIllustPanel(h));
            }
            #endregion
            #region User
            public static void Setup(PixivLoaders.User loader)
            {
                loader.start = Start;
                loader.update = UserUpdate;
                loader.end = End;
            }
            public static void UserUpdate(PixivUser h, int index, int max)
            {
                Global.MainWindow.label.Content = $"{index}/{max}";
                Global.MainWindow.MainPanel.Children.Add(new UserControls.Panels.PixivUserPanel(h));
            }
            #endregion
        }
        public LoaderDefaults SetSearch(Action<int> search)
        {
            this.search = search;
            return this;
        }
        public Action<int> SetPagination(int page)
        {
            return (int pages) =>
            {
                Pagination pagination = new Pagination(page, pages);
                pagination.btnFirs_Clk = (object sender, RoutedEventArgs e) => search(1);
                pagination.btnPrev_Clk = (object sender, RoutedEventArgs e) => search(page - 1);
                pagination.btnNext_Clk = (object sender, RoutedEventArgs e) => search(page + 1);
                pagination.btnLast_Clk = (object sender, RoutedEventArgs e) => search(pages);
                pagination.cbNumberOfRecords_SelectionChanged1 = (object sender, SelectionChangedEventArgs e) => search((int)e.AddedItems[0]);
                Global.MainWindow.MainPanel.Children.Add(pagination);
            };
        }
    }
}
