using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HitomiViewer.UserControls
{
    /// <summary>
    /// Pagination.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Pagination : UserControl
    {
        public Action<object, RoutedEventArgs> btnFirs_Clk = (object sender, RoutedEventArgs e) => { };
        public Action<object, RoutedEventArgs> btnPrev_Clk = (object sender, RoutedEventArgs e) => { };
        public Action<object, RoutedEventArgs> btnNext_Clk = (object sender, RoutedEventArgs e) => { };
        public Action<object, RoutedEventArgs> btnLast_Clk = (object sender, RoutedEventArgs e) => { };
        public Action<object, SelectionChangedEventArgs> cbNumberOfRecords_SelectionChanged1 = (object sender, SelectionChangedEventArgs e) => { };

        public Pagination(int page, int pages)
        {
            InitializeComponent();
            if (page <= 1)
            {
                btnFirs.IsEnabled = false;
                btnPrev.IsEnabled = false;
            }
            if (page >= pages)
            {
                btnLast.IsEnabled = false;
                btnNext.IsEnabled = false;
            }
            lblpageInformation.Content = $"{page} of {pages}";
            List<int> items = new List<int>();
            for (int i = 1; i <= pages; i++) items.Add(i);
            cbNumberOfRecords.ItemsSource = items;
            cbNumberOfRecords.SelectedIndex = page - 1;
        }

        private void btnFirs_Click(object sender, RoutedEventArgs e) => btnFirs_Clk(sender, e);
        private void btnPrev_Click(object sender, RoutedEventArgs e) => btnPrev_Clk(sender, e);
        private void btnNext_Click(object sender, RoutedEventArgs e) => btnNext_Clk(sender, e);
        private void btnLast_Click(object sender, RoutedEventArgs e) => btnLast_Clk(sender, e);

        private void cbNumberOfRecords_SelectionChanged(object sender, SelectionChangedEventArgs e) => cbNumberOfRecords_SelectionChanged1(sender, e);
    }
}
