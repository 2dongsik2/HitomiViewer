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
using System.Windows.Shapes;

namespace HitomiViewer.UserControls
{
    /// <summary>
    /// Login.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Login : Window
    {
        public string nickname;
        public string password;
        public Login()
        {
            InitializeComponent();
            Nickname.Focus();
        }

        private void submit_Click(object sender, RoutedEventArgs e)
        {
            this.nickname = Nickname.Text;
            this.password = Password.Password;
            this.DialogResult = true;
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.nickname = Nickname.Text;
            this.password = Password.Password;
            this.DialogResult = false;
        }
    }
}
