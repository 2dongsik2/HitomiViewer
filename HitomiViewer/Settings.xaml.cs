using ExtensionMethods;
using HitomiViewer.Encryption;
using HitomiViewer.Processor;
using HitomiViewer.Scripts;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
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
using Path = System.IO.Path;

namespace HitomiViewer
{
    /// <summary>
    /// Settings.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Settings : Window
    {
        private readonly string[] oldconfig = new string[] { "pw", "fe", "autofe", "et", "rt", "df", "fav" };
        private readonly string[] newconfig = new string[] { password, file_encrypt, download_file_encrypt, encrypt_title, random_title, download_folder, favorites };

        public const string password = "password";
        public const string file_encrypt = "file-encrypt";
        public const string download_file_encrypt = "download-file-encrypt";
        public const string encrypt_title = "encrypt-title";
        public const string random_title = "random-title";
        public const string download_folder = "download-file";
        public const string except_tags = "except-tags";
        public const string favorites = "favorites";
        public const string block_tags = "block_tags";
        public const string cache_search = "cachesearch";
        public const string origin_thumb = "originalthumbnail";

        private List<string> ExceptTagList = new List<string>();

        public Settings()
        {
            InitializeComponent();
            ConfigFile cfg = new ConfigFile();
            ConfigFileData config = cfg.Load();
            InitFolderName(config);
            InitPassword(config);
            InitEncrypt(config);
            InitTitle(config);
            InitTags(config);
        }

        private void InitFolderName(ConfigFileData config)
        {
            FolderName.Content = config.download_folder.Get<string>();
        }
        private void InitPassword(ConfigFileData config)
        {
            if (config.password.Get<string>() != null)
                Password.IsChecked = true;
        }
        private void InitEncrypt(ConfigFileData config)
        {
            FileEncrypt.IsChecked = config.file_encrypt.Get<bool>();
            AutoEncryption.IsChecked = config.download_file_encrypt.Get<bool>();
        }
        private void InitTitle(ConfigFileData config)
        {
            EncryptTitle.IsChecked = config.encrypt_title.Get<bool>();
            RandomTitle.IsChecked = config.random_title.Get<bool>();
        }
        private void InitTags(ConfigFileData config)
        {

            BlockTags.IsChecked = config.block_tags.Get<bool>();
            List<string> tags = config.except_tags.Get<List<string>>();
            if (tags != null)
                foreach (string tag in tags) ExceptTagList.Add(tag);
            TagList2ListBox();
        }

        private void TagList2ListBox()
        {
            ExceptTags.Items.Clear();
            foreach (string tag in ExceptTagList)
            {
                StackPanel stack = new StackPanel();
                stack.Children.Add(new Label
                {
                    MinWidth = 176,
                    Content = tag
                });
                Button btn = new Button { Content = "x" };
                btn.Click += (object sender, RoutedEventArgs e) =>
                {
                    ExceptTags.Items.Remove(stack);
                    ExceptTagList.Remove(tag);
                };
                stack.Children.Add(btn);
                ExceptTags.Items.Add(stack);
            }
        }

        private void Password_Checked(object sender, RoutedEventArgs e) => FileEncrypt.IsEnabled = true;
        private void Password_Unchecked(object sender, RoutedEventArgs e) => FileEncrypt.IsEnabled = false;
        private void FileEncrypt_Checked(object sender, RoutedEventArgs e) => AutoEncryption.IsEnabled = true;
        private void FileEncrypt_Unchecked(object sender, RoutedEventArgs e) => AutoEncryption.IsEnabled = false;
        private void RandomTitle_Checked(object sender, RoutedEventArgs e) => EncryptTitle.IsEnabled = false;
        private void RandomTitle_Unchecked(object sender, RoutedEventArgs e) => EncryptTitle.IsEnabled = true;
        private void EncryptTitle_Checked(object sender, RoutedEventArgs e) => RandomTitle.IsEnabled = false;
        private void EncryptTitle_Unchecked(object sender, RoutedEventArgs e) => RandomTitle.IsEnabled = true;
        private void RandomDownloadFolder_Click(object sender, RoutedEventArgs e)
        {

        }
        private void ChangeDownloadFolder_Click(object sender, RoutedEventArgs e)
        {

        }
        private void ExceptTagsBtn_Click(object sender, RoutedEventArgs e)
        {

        }
        private void ExcecptTagsText_KeyDown(object sender, KeyEventArgs e)
        {

        }
        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ConfigFileData data = Global.config;
            
        }
    }
}
