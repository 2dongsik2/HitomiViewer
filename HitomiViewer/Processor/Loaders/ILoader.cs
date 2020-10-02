using HitomiViewer.UserControls;
using System;
using System.Windows;
using System.Windows.Controls;

namespace HitomiViewer.Processor.Loaders
{
    class ILoader
    {
        public int index = 0;
        public int count = 0;
        public Action<int> start = null;
        public Action<int> search = null;
        public Action<int> pagination = null;
        public Action end = null;

        public virtual void Default()
        {
            this.start = (int count) =>
            {
                Global.MainWindow.label.Content = "0/" + count;
                Global.MainWindow.label.Visibility = System.Windows.Visibility.Visible;
                Global.MainWindow.Searching(true);
            };
            this.end = () =>
            {
                Global.MainWindow.label.Visibility = System.Windows.Visibility.Collapsed;
                Global.MainWindow.Searching(false);
            };
        }
        public virtual void Parser() { }
        public virtual void Search() { }
        public ILoader SetSearch(Action<int> search)
        {
            this.search = search;
            return this;
        }
        public virtual Action<int> SetPagination(int page)
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
