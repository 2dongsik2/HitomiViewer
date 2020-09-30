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
    class HitomiReader : IReader
    {
        private Hitomi hitomi;

        public HitomiReader(Hitomi hitomi)
        {
            this.Background = new SolidColorBrush(Global.background);
            this.hitomi = hitomi;
            this.window = Global.MainWindow;
            this.page = 0;
            this.images = new BitmapImage[hitomi.files.Length];
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
            this.image.Source = hitomi.thumbnail.preview_img;
            this.Title = hitomi.name;
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            if (hitomi.files == null || hitomi.files.Length <= 0)
            {
                MessageBox.Show("이미지를 불러올 수 없습니다.");
                Close();
            }
            new TaskFactory().StartNew(() => {
                while (hitomi.files == null || hitomi.files.Length <= 0) { }
                if (hitomi.thumbnail.preview_img == null) this.image.Source = ImageProcessor.ProcessEncrypt(hitomi.files[0].url);
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
            if (images == null || images.Length < hitomi.files.Length)
                images = new BitmapImage[hitomi.files.Length];
            if (images[copypage] == null)
            {
                BitmapImage image = await ImageProcessor.ProcessEncryptAsync(file.url);
                image.Freeze();
                if (images.Length == hitomi.files.Length)
                    images[copypage] = image;
            }
            if (copypage == page && images.Length == hitomi.files.Length)
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

        protected override void Image_KeyDown(object sender, KeyEventArgs e)
        {
            base.Image_KeyDown(sender, e);
            if (e.Key == Key.Right)
            {
                if (page < hitomi.files.Length - 1)
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
                SetImage(hitomi.files[page]);
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
                if (page < hitomi.files.Length - 1)
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
                if (hitomi.files == null || hitomi.files.Length <= 0)
                    SetImage(new Uri("/Resources/loading2.png", UriKind.Relative));
                else
                    SetImage(hitomi.files[page]);
            }
        }
    }
}
