using ExtensionMethods;
using HitomiViewer.Encryption;
using HitomiViewer.Scripts;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WebPWrapper;

namespace HitomiViewer.Processor
{
    class ImageProcessor
    {
        public static BitmapImage Process(string url)
        {
            if (url.isUrl())
            {
                if (url.EndsWith(".webp"))
                    return LoadWebWebPImage(url);
                else
                    return LoadWebImage(url);
            }
            if (Global.config.file_encrypt.Get<bool>())
            {
                try
                {
                    byte[] org = File.ReadAllBytes(url);
                    byte[] dec = FileDecrypt.Default(org);
                    using (var ms = new MemoryStream(dec))
                    {
                        var image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad; // here
                        image.StreamSource = ms;
                        image.EndInit();
                        return image;
                    }
                }
                catch
                {
                    return ProcessEncrypt(url);
                }
            }
            return ProcessFile(url);
        }
        public static BitmapImage ProcessFile(string url)
        {
            try
            {
                return LoadMemory(url);
            }
            catch (FileNotFoundException)
            {
                return FromResource("NoImage.jpg");
            }
            catch (NotSupportedException)
            {
                return FromResource("ErrEncrypted.jpg");
            }
        }
        public static BitmapImage ProcessEncrypt(string url)
        {
            if (url.isUrl())
            {
                if (url.EndsWith(".webp"))
                {
                    return LoadWebWebPImage(url);
                }
                else
                {
                    return LoadWebImage(url);
                }
            }
            else if (Global.config.file_encrypt.Get<bool>())
            {
                try
                {
                    byte[] org = File.ReadAllBytes(url);
                    byte[] dec = FileDecrypt.Default(org);
                    using (var ms = new MemoryStream(dec))
                    {
                        var image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad; // here
                        image.StreamSource = ms;
                        image.EndInit();
                        return image;
                    }
                }
                catch {
                    try
                    {
                        return LoadMemory(url);
                    }
                    catch (FileNotFoundException)
                    {
                        return FromResource("NoImage.jpg");
                    }
                    catch (NotSupportedException)
                    {
                        return FromResource("ErrEncrypted.jpg");
                    }
                }
            }
            else
            {
                try
                {
                    return LoadMemory(url);
                }
                catch (FileNotFoundException)
                {
                    return FromResource("NoImage.jpg");
                }
                catch (NotSupportedException)
                {
                    return FromResource("ErrEncrypted.jpg");
                }
            }
        }
        public static async Task<BitmapImage> ProcessEncryptAsync(string url)
        {
            if (url.isUrl())
            {
                if (url.EndsWith(".webp"))
                {
                    BitmapImage result = await LoadWebWebPImageAsyncException(url);
                    return result;
                }
                else
                {
                    BitmapImage result = await LoadWebImageAsyncException(url);
                    return result;
                }
            }
            else if (Global.config.file_encrypt.Get<bool>())
            {
                try
                {
                    byte[] org = File.ReadAllBytes(url);
                    byte[] dec = FileDecrypt.Default(org);
                    using (var ms = new MemoryStream(dec))
                    {
                        var image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad; // here
                        image.StreamSource = ms;
                        image.EndInit();
                        return image;
                    }
                }
                catch
                {
                    try
                    {
                        return LoadMemory(url);
                    }
                    catch (FileNotFoundException)
                    {
                        return FromResource("NoImage.jpg");
                    }
                    catch (NotSupportedException)
                    {
                        return FromResource("ErrEncrypted.jpg");
                    }
                }
            }
            else
            {
                try
                {
                    return LoadMemory(url);
                }
                catch (FileNotFoundException)
                {
                    return FromResource("NoImage.jpg");
                }
                catch (NotSupportedException)
                {
                    return FromResource("ErrEncrypted.jpg");
                }
            }
        }

        public static BitmapImage LoadMemory(string url)
        {
            var bitmap = new BitmapImage();
            var stream = File.OpenRead(url);

            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            stream.Close();
            stream.Dispose();
            bitmap.Freeze();
            return bitmap;
        }
        public static BitmapImage LoadWebImage(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                    return null;
                System.Net.WebClient wc = new System.Net.WebClient();
                Byte[] MyData = wc.DownloadData(url);
                wc.Dispose();
                BitmapImage bimgTemp = new BitmapImage();
                bimgTemp.BeginInit();
                bimgTemp.StreamSource = new MemoryStream(MyData);
                bimgTemp.EndInit();
                return bimgTemp;
            }
            catch
            {
                return null;
            }
        }
        public static async Task<BitmapImage> LoadWebImageAsync(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                    return null;
                System.Net.WebClient wc = new System.Net.WebClient();
                wc.Headers.Add("Referer", new Uri(url).Host);
                Byte[] MyData = await wc.DownloadDataTaskAsync(url);
                wc.Dispose();
                BitmapImage bimgTemp = new BitmapImage();
                bimgTemp.BeginInit();
                bimgTemp.StreamSource = new MemoryStream(MyData);
                bimgTemp.EndInit();
                return bimgTemp;
            }
            catch
            {
                return null;
            }
        }
        public static async Task<BitmapImage> LoadWebImageAsyncException(string url)
        {
            if (string.IsNullOrEmpty(url))
                return null;
            System.Net.WebClient wc = new System.Net.WebClient();
            wc.Headers.Add("Referer", "https://" + new Uri(url).Host);
            Byte[] MyData = await wc.DownloadDataTaskAsync(url);
            wc.Dispose();
            BitmapImage bimgTemp = new BitmapImage();
            bimgTemp.BeginInit();
            bimgTemp.StreamSource = new MemoryStream(MyData);
            bimgTemp.EndInit();
            return bimgTemp;
        }
        public static BitmapImage LoadWebWebPImage(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                    return null;
                System.Net.WebClient wc = new System.Net.WebClient();
                wc.Headers.Add("Referer", "https://hitomi.la/");
                Byte[] MyData = wc.DownloadData(url);
                wc.Dispose();
                WebP webP = new WebP();
                Bitmap bitmap = webP.Decode(MyData);
                MemoryStream ms = new MemoryStream();
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                var bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.EndInit();
                return bi;
            }
            catch
            {
                return null;
            }
        }
        public static async Task<BitmapImage> LoadWebWebPImageAsyncException(string url)
        {
            if (string.IsNullOrEmpty(url))
                return null;
            System.Net.WebClient wc = new System.Net.WebClient();
            wc.Headers.Add("Referer", "https://" + new Uri(url).Host);
            Byte[] MyData = await wc.DownloadDataTaskAsync(url);
            wc.Dispose();
            WebP webP = new WebP();
            Bitmap bitmap = webP.Decode(MyData);
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            var bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.EndInit();
            return bi;
        }

        public static BitmapImage FromIncludedResource(string psResourceName)
        {
            Uri oUri = new Uri($"pack://siteoforigin:,,,/Resources/{psResourceName}");
            return new BitmapImage(oUri);
        }
        public static BitmapImage FromIncludedResourceWithName(string psAssemblyName, string psResourceName)
        {
            Uri oUri = new Uri("pack://application:,,,/" + psAssemblyName + ";component/" + psResourceName, UriKind.RelativeOrAbsolute);
            return new BitmapImage(oUri);
        }
        public static BitmapImage FromResource(string psResourceName)
        {
            Uri oUri = new Uri($"/Resources/{psResourceName}", UriKind.RelativeOrAbsolute);
            return new BitmapImage(oUri);
        }

        public static BitmapImage WebPBytes2Image(byte[] data)
        {
            using (WebP webP = new WebP())
            {
                try
                {
                    Bitmap bitmap = webP.Decode(data);
                    MemoryStream ms = new MemoryStream();
                    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                    var bi = new BitmapImage();
                    bi.BeginInit();
                    bi.StreamSource = ms;
                    bi.EndInit();
                    ms.Dispose();
                    return bi;
                }
                catch
                {
                    return null;
                }
            }
        }
        public static BitmapImage Bytes2Image(byte[] array)
        {
            using (MemoryStream ms = new MemoryStream(array))
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }
        public static BitmapImage Bytes2Image2(byte[] array)
        {
            BitmapImage bimgTemp = new BitmapImage();
            bimgTemp.BeginInit();
            bimgTemp.StreamSource = new MemoryStream(array);
            bimgTemp.EndInit();
            return bimgTemp;
        }
        public static Byte[] Image2Bytes(BitmapImage image)
        {
            try
            {
                byte[] bytes = null;
                var bitmapSource = image as BitmapSource;
                var encoder = new BmpBitmapEncoder();
                if (bitmapSource != null)
                {
                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                    using (var stream = new System.IO.MemoryStream())
                    {
                        encoder.Save(stream);
                        bytes = stream.ToArray();
                    }
                }
                return bytes;
            }
            catch
            {
                return new byte[0];
            }
        }
        public static BitmapImage Bitmap2BitmapImage(Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.EndInit();
                return bi;
            }
        }
        public static BitmapSource Bitmap2BitmapSource(Bitmap bitmap)
        {
            return Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(),
                                      IntPtr.Zero,
                                      Int32Rect.Empty,
                                      BitmapSizeOptions.FromEmptyOptions());
        }
        public static async Task<BitmapImage> PixivImage(string url)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Referer", "https://www.pixiv.net/");
            HttpResponseMessage response = await client.GetAsync(url);
            byte[] data = await response.Content.ReadAsByteArrayAsync();
            BitmapImage bimgTemp = new BitmapImage();
            bimgTemp.BeginInit();
            bimgTemp.StreamSource = new MemoryStream(data);
            bimgTemp.EndInit();
            return bimgTemp;
        }
    }
    public static partial class ExtensionMethods
    {
        public static BitmapImage ToImage(this byte[] array) => ImageProcessor.Bytes2Image(array);
        public static BitmapImage ToBitmapImage(this Bitmap bitmap) => ImageProcessor.Bitmap2BitmapImage(bitmap);
        public static BitmapSource ToBitmapSource(this Bitmap bitmap) => ImageProcessor.Bitmap2BitmapSource(bitmap);
    }
}
