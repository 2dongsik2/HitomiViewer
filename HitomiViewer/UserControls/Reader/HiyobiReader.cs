using ExtensionMethods;
using HitomiViewer.Processor;
using HitomiViewer.Scripts;
using HitomiViewerLibrary.Structs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            this.image.Source = hiyobi.thumbnail.preview_img.ToImage();
            this.Title = hiyobi.name;
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            if (hiyobi.files == null || hiyobi.files.Length <= 0)
            {
                MessageBox.Show("이미지를 불러올 수 없습니다.");
                Close();
            }
            new TaskFactory().StartNew(async () => {
                while (hiyobi.files == null || hiyobi.files.Length <= 0) { }
                if (hiyobi.thumbnail.preview_img == null)
                    this.image.Source = (await ImageProcessor.ProcessAsync(hiyobi.files[0].url)).ToImage();
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
                BitmapImage image = null;
                Task<byte[]> task = null;
                for (int m = 0; m < 10 && image == null; m++)
                {
                    task = ImageProcessor.ProcessAsync(file.url);
                    try { image = (await task).ToImage(); } catch { }
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
                    if (images.Length == hiyobi.files.Length)
                        images[copypage] = image;
                }
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
        protected override async void PreLoad()
        {
            base.PreLoad();
            if (page + 1 >= hiyobi.files.Length) return;
            int loadpage = page + 1;
            if (images == null || images.Length < hiyobi.files.Length)
                images = new BitmapImage[hiyobi.files.Length];
            if (images[loadpage] != null) return;
            Hitomi.HFile file = hiyobi.files[loadpage];
            BitmapImage image = null;
            Task<byte[]> task = null;
            for (int m = 0; m < 10 && image == null; m++)
            {
                task = ImageProcessor.ProcessAsync(file.url);
                try { image = (await task).ToImage(); } catch { }
            }
            if (image == null && task.IsFaulted)
            {
                MessageBox.Show($"{loadpage + 1}번 이미지를 불러오는데 실패했습니다.\nlatest.log 에 정보가 기록됩니다.\n{file.url}\n{task.Exception.InnerException.ToString()}");
                System.Reflection.MethodBase current = System.Reflection.MethodBase.GetCurrentMethod();
                task.Exception.InnerException.WriteExcept(sourceName: current.FullName());
            }
            else
            {
                image.Freeze();
                if (images.Length == hiyobi.files.Length)
                    images[loadpage] = image;
            }
        }
        protected override void PreLoadAll(int start)
        {
            PreLoadAll(start, null);
        }
        protected override async void PreLoadAll(int start = 0, int? max = null)
        {
            base.PreLoadAll(start);
            this.Title = hiyobi.name + " 0/" + (hiyobi.files.Length - 1);
            int end = max ?? hiyobi.files.Length;
            for (int i = start; i < end; i++)
            {
                if (this.IsClosed)
                {
                    images = images.Select(x => x = null).ToArray();
                    GC.Collect();
                    return;
                }
                this.Title = hiyobi.name + " " + i + "/" + (end - 1);
                if (images == null || images.Length < hiyobi.files.Length)
                    images = new BitmapImage[hiyobi.files.Length];
                if (images[i] == null)
                {
                    BitmapImage image = null;
                    Task<byte[]> task = null;
                    for (int m = 0; m < 10 && image == null; m++)
                    {
                        task = ImageProcessor.ProcessAsync(hiyobi.files[i].url);
                        try { image = (await task).ToImage(); } catch { }
                    }
                    if (image == null && task.IsFaulted)
                    {
                        MessageBox.Show($"{i + 1}번 이미지를 불러오는데 실패했습니다.\nexcept.log 에 정보가 기록됩니다.\n{hiyobi.files[i].url}\n{task.Exception.InnerException.Message}");
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
        private Task CLMRunner = null;
        protected override void ClearMemory()
        {
            base.ClearMemory();
            if (CLMRunner != null && !CLMRunner.IsCompleted) return;
            CLMRunner = Task.Factory.StartNew(() =>
            {
                if (page % 10 == 0)
                {
                    BitmapImage[] notnullimages = images.Where(x => x != null).ToArray();
                    if (notnullimages.Length < 1) return;
                    long size = notnullimages.Select(x => ImageProcessor.Image2Bytes(x).LongLength).Sum() / notnullimages.LongLength;
                    hiyobi.FileInfo.size = size;
                }
                double byteperimage = ((double)hiyobi.FileInfo.size) / 1024 / 1024;
                for (int i = 0; i < page; i++)
                {
                    if (byteperimage == 0) byteperimage = 20;
                    if (page - (1024 * 0.5 / byteperimage) >= i)
                        images[i] = null;
                }
                GC.Collect();
            });
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
            if (e.Key == Key.Enter)
            {
                if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    CountBox countBox = new CountBox("페이지", "원하는 페이지 수", 1);
                    int page = (int)countBox.ShowDialog();
                    bool success = countBox.success;
                    if (success)
                        PreLoadAll(0, page);
                }
                else
                    PreLoadAll(base.page);
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.C)
                Clipboard.SetImage((BitmapSource)this.image.Source);
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
