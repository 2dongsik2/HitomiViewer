using ExtensionMethods;
using HitomiViewer.Processor;
using HitomiViewer.Scripts;
using HitomiViewerLibrary.Structs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace HitomiViewer
{
    /// <summary>
    /// Reader.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class IReader : Window
    {
        protected MainWindow window;
        protected int page;
        protected BitmapImage[] images;
        public bool IsClosed { get; private set; }

        protected override void OnSourceInitialized(EventArgs e)
        {
            //IconHelper.RemoveIcon(this);
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            IsClosed = true;
        }

        protected virtual void Init()
        {
            
        }
        protected virtual void ClearMemory()
        {

        }

        public virtual void ChangeMode()
        {
            this.Background = new SolidColorBrush(Global.background);
        }

        protected virtual void Image_KeyDown(object sender, KeyEventArgs e)
        {
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
            if (e.Key == Key.Escape)
            {
                if (base.WindowStyle == WindowStyle.None && base.WindowState == WindowState.Maximized)
                {
                    base.WindowStyle = WindowStyle.SingleBorderWindow;
                    base.WindowState = WindowState.Normal;
                }
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.C)
                Clipboard.SetImage((BitmapSource)this.image.Source);
        }

        protected virtual void PreLoadAll(int start)
        {
            
        }

        protected virtual void SetImage(Uri link) { }

        protected virtual void SetImage(Hitomi.HFile file)
        {
            BitmapImage LoadingImage = new BitmapImage(new Uri("/Resources/loading2.gif", UriKind.RelativeOrAbsolute));
            LoadingImage.Freeze();
            image.Source = LoadingImage;
            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(image, LoadingImage);
        }
        protected virtual void Image_MouseDown(object sender, MouseEventArgs e) { }
        protected virtual void PreLoad() { }

        public string ImageSourceToString(ImageSource imageSource) {
            byte[] bytes = null;
            var bitmapSource = imageSource as BitmapSource;
            var encoder = new BmpBitmapEncoder();
            if (bitmapSource != null) {
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                using (var stream = new System.IO.MemoryStream()) {
                    encoder.Save(stream);
                    bytes = stream.ToArray();
                }
            }
            return Convert.ToBase64String(bytes);
        }
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);
        public ImageSource ImageSourceFromBitmap(Bitmap bmp)
        {
            try
            {
                var handle = bmp.GetHbitmap();
                BitmapSource source = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                DeleteObject(handle);
                return source;
            }
            catch
            {
                return ImageSourceFromBitmap(bmp);
            }
        }
    }
}
