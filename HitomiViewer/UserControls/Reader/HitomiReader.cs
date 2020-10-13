using ExtensionMethods;
using HitomiViewer.Processor;
using HitomiViewerLibrary.Structs;
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
                BitmapImage image = null;
                Task<BitmapImage> task = null;
                for (int m = 0; m < 10 && image == null; m++)
                {
                    task = ImageProcessor.ProcessEncryptAsync(file.url);
                    try { image = await task; } catch { }
                }
                if (image == null && task.IsFaulted)
                {
                    MessageBox.Show($"{copypage + 1}번 이미지를 불러오는데 실패했습니다.\nlatest.log 에 정보가 기록됩니다.\n{file.url}\n{task.Exception.InnerException.ToString()}");
                    System.Reflection.MethodBase current = System.Reflection.MethodBase.GetCurrentMethod();
                    task.Exception.InnerException.WriteExcept(sourceName: current.FullName());
                }
                else
                {
                    image.Freeze();
                    if (images.Length == hitomi.files.Length)
                        images[copypage] = image;
                }
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
        protected override async void PreLoadAll(int start)
        {
            base.PreLoadAll(start);
            this.Title = hitomi.name + " 0/" + (hitomi.files.Length - 1);
            for (int i = start; i < hitomi.files.Length; i++)
            {
                this.Title = hitomi.name + " " + i + "/" + (hitomi.files.Length - 1);
                if (images == null || images.Length < hitomi.files.Length)
                    images = new BitmapImage[hitomi.files.Length];
                if (images[i] == null)
                {
                    BitmapImage image = null;
                    Task<BitmapImage> task = null;
                    for (int m = 0; m < 10 && image == null; m++)
                    {
                        task = ImageProcessor.ProcessEncryptAsync(hitomi.files[i].url);
                        try { image = await task; } catch { }
                    }
                    if (image == null && task.IsFaulted)
                    {
                        MessageBox.Show($"{i + 1}번 이미지를 불러오는데 실패했습니다.\nexcept.log 에 정보가 기록됩니다.\n{hitomi.files[i].url}\n{task.Exception.InnerException.Message}");
                        System.Reflection.MethodBase current = System.Reflection.MethodBase.GetCurrentMethod();
                        task.Exception.InnerException.WriteExcept(sourceName: current.FullName());
                        return;
                    }
                    else
                    {
                        image.Freeze();
                        images[i] = image;
                    }
                }
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
            if (e.Key == Key.Enter) PreLoadAll(0);
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.C)
                Clipboard.SetImage((BitmapSource)this.image.Source);
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
