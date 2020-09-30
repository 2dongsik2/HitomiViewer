using HitomiViewer.Processor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HitomiViewer.UserControls.Reader
{
    class HiyobiReader : IReader
    {
        private HiyobiGallery hiyobi;

        public HiyobiReader(HiyobiGallery hiyobi)
        {
            base.Background = new SolidColorBrush(Global.background);
            this.hiyobi = hiyobi;
            base.window = Global.MainWindow;
            base.page = 0;
            base.images = new BitmapImage[hiyobi.files.Length];
            InitializeComponent();
            Init();
        }

        protected override void Init()
        {
            base.Init();
            this.window.Readers.Add(this);
            this.Loaded += (object sender, RoutedEventArgs e) => this.Focus();
            this.Closing += (object sender, System.ComponentModel.CancelEventArgs e) =>
            {
                window.Readers.Remove(this);
            };
            this.image.Source = hiyobi.thumbnail.preview_img;
            this.Title = hiyobi.name;
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            if (hiyobi.files == null || hiyobi.files.Length <= 0)
            {
                MessageBox.Show("이미지를 불러올 수 없습니다.");
                Close();
            }
            new TaskFactory().StartNew(() => {
                while (hiyobi.files == null || hiyobi.files.Length <= 0) { }
                if (hiyobi.thumbnail.preview_img == null) this.image.Source = ImageProcessor.ProcessEncrypt(hiyobi.files[0].url);
                System.Threading.Thread.Sleep(100);
                this.Dispatcher.Invoke(() =>
                {
                    this.Activate();
                    this.WindowStyle = WindowStyle.None;
                    this.WindowState = WindowState.Maximized;
                });
                System.Threading.Thread.Sleep(500);
                this.Dispatcher.Invoke(() =>
                {
                    this.Activate();
                    this.WindowStyle = WindowStyle.None;
                    this.WindowState = WindowState.Maximized;
                });
            });
        }
        protected override async void SetImage(Hitomi.HFile file)
        {
            base.SetImage(file);
            int copypage = page;
            if (images == null || images.Length < hiyobi.files.Length)
                images = new BitmapImage[hiyobi.files.Length];
            if (images[copypage] == null)
            {
                BitmapImage image = await ImageProcessor.ProcessEncryptAsync(file.url);
                image.Freeze();
                if (images.Length == hiyobi.files.Length)
                    images[copypage] = image;
            }
            if (copypage == page && images.Length == hiyobi.files.Length)
            {
                if (file.url.EndsWith(".gif"))
                    WpfAnimatedGif.ImageBehavior.SetAnimatedSource(image, images[page]);
                else
                {
                    WpfAnimatedGif.ImageBehavior.SetAnimatedSource(image, null);
                    image.Source = images[page];
                }
                ClearMemory();
            }
        }
        protected override async void PreLoadAll(int start)
        {
            base.PreLoadAll(start);
            this.Title = hiyobi.name + " 0/" + (hiyobi.files.Length - 1);
            for (int i = start; i < hiyobi.files.Length; i++)
            {
                this.Title = hiyobi.name + " " + i + "/" + (hiyobi.files.Length - 1);
                if (images == null || images.Length < hiyobi.files.Length)
                    images = new BitmapImage[hiyobi.files.Length];
                if (images[i] == null)
                {
                    BitmapImage image;
                    image = await ImageProcessor.ProcessEncryptAsync(hiyobi.files[i].url);
                    image.Freeze();
                    images[i] = image;
                }
            }
        }

        protected override void Image_KeyDown(object sender, KeyEventArgs e)
        {
            base.Image_KeyDown(sender, e);
            if (e.Key == Key.Right)
            {
                if (page < hiyobi.files.Length - 1)
                    page++;
            }
            else if (e.Key == Key.Left)
            {
                if (page > 0)
                    page--;
            }
            if (e.Key == Key.Right || e.Key == Key.Left)
            {
                PreLoad();
                SetImage(hiyobi.files[page]);
            }
            if (e.Key == Key.F11)
            {
                if (base.WindowStyle == WindowStyle.None && base.WindowState == WindowState.Maximized)
                {
                    base.WindowStyle = WindowStyle.SingleBorderWindow;
                    base.WindowState = WindowState.Normal;
                }
                else if (base.WindowStyle == WindowStyle.SingleBorderWindow && base.WindowState == WindowState.Normal)
                {
                    base.WindowStyle = WindowStyle.None;
                    base.WindowState = WindowState.Maximized;
                }
                else if (base.WindowStyle == WindowStyle.SingleBorderWindow && base.WindowState == WindowState.Maximized)
                {
                    base.WindowStyle = WindowStyle.None;
                    base.WindowState = WindowState.Normal;
                    base.WindowState = WindowState.Maximized;
                }
            }
            else if (e.Key == Key.Escape)
            {
                if (base.WindowStyle == WindowStyle.None && base.WindowState == WindowState.Maximized)
                {
                    base.WindowStyle = WindowStyle.SingleBorderWindow;
                    base.WindowState = WindowState.Normal;
                }
            }
            else if (e.Key == Key.Enter)
            {

            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.C)
            {
                Clipboard.SetImage((BitmapSource)this.image.Source);
            }
        }
        protected override void Image_MouseDown(object sender, MouseEventArgs e)
        {
            base.Image_MouseDown(sender, e);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (page < hiyobi.files.Length - 1)
                    page++;
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                if (page > 0)
                    page--;
            }
            if (e.RightButton == MouseButtonState.Pressed || e.LeftButton == MouseButtonState.Pressed)
            {
                PreLoad();
                if (hiyobi.files == null || hiyobi.files.Length <= 0)
                    SetImage(new Uri("/Resources/loading2.png", UriKind.Relative));
                else
                    SetImage(hiyobi.files[page]);
            }
        }
    }
}
