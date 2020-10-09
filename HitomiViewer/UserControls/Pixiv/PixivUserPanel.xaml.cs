using ExtensionMethods;
using HitomiViewer.Processor;
using HitomiViewer.Processor.Loaders;
using HitomiViewerLibrary;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace HitomiViewer.UserControls
{
    /// <summary>
    /// PixivUser.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PixivUserPanel : UserControl
    {
        private PixivUser user;

        public PixivUserPanel(PixivUser user)
        {
            this.user = user;
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            /*
            nameLabel.Content = user.user.name;
            follow.IsChecked = user.user.is_followed;
            if (user.user.is_followed)
                follow.Content = "팔로우 중입니다.";
            else
                follow.Content = "팔로우 중이 아닙니다.";
            Dispatcher dispatcher = this.Dispatcher;
            ImageProcessor.PixivImage(user.user.profile_image_urls.medium).then((BitmapImage profileImage) => dispatcher.Invoke(() =>
            {
                userImage.Source = profileImage;
                thumbBrush.ImageSource = profileImage;
            }));

            Global.Account.Pixiv.userDetail(user.user.id.ToString()).then((JObject detail) => dispatcher.Invoke(() =>
            {
                IllustsCount.Content = detail["profile"].IntValue("total_illusts") + detail["profile"].IntValue("total_manga");
                description.Content = detail["user"].StringValue("comment");
            }));

            foreach (Illust illust in user.illusts)
            {
                Image image = new Image();
                string thumb;
                if (illust.page_count <= 1)
                {
                    thumb = illust.meta_single_page.original_image_url;
                    Task.Factory.StartNew(async () =>
                    {
                        try
                        {
                            Pixiv pixiv = Global.Account.Pixiv;
                            JObject obj = await pixiv.ugoiraMetaData(illust.id.ToString());
                            WebClient wc = new WebClient();
                            wc.Headers.Add("Referer", "https://www.pixiv.net/");
                            byte[] zipbyte = await wc.DownloadDataTaskAsync(obj["ugoira_metadata"]["zip_urls"].StringValue("medium"));
                            PixivUgoira ugoiraImage = pixiv.UnZip(zipbyte);
                            ugoiraImage.original = obj["ugoira_metadata"]["zip_urls"].StringValue("medium");
                            ugoiraImage.delays = obj["ugoira_metadata"]["frames"].Select(x => x.IntValue("delay") ?? 0).ToList();
                            image.MouseDown += (object sender, MouseButtonEventArgs e) =>
                            {
                                Hitomi h = new Hitomi();
                                h.files = new string[] { thumb };
                                h.page = 1;
                                h.ugoiraImage = ugoiraImage;
                                h.name = illust.title;
                                Reader reader = new Reader(h);
                                if (!reader.IsClosed)
                                    reader.Show();
                                reader.Focus();
                            };
                            image.MouseEnter += (object sender, MouseEventArgs e) => Task.Factory.StartNew(() => Ugoira(this.Dispatcher, ugoiraImage, image));
                        }
                        catch { }
                    });
                }
                else
                    thumb = illust.meta_pages.First().original;
                ImageProcessor.PixivImage(thumb).then((BitmapImage result, object data) =>
                {
                    Image img = (Image)data;
                    img.Source = result;
                }, image);
                Illusts.Children.Add(image);
            }

            userImage.MouseDown += async (object sender, MouseButtonEventArgs e) =>
            {
                Global.MainWindow.MainPanel.Children.Clear();
                JObject data = await Global.Account.Pixiv.userIllusts(user.user.id.ToString());
                PixivLoader loader = new PixivLoader();
                loader.Default().Parser(data);
            };
            */
        }
        private void Ugoira(Dispatcher dispatcher, PixivUgoira ugoiraImage, Image image)
        {
            if (this.Visibility != Visibility.Visible)
                return;
            if (!image.IsMouseOver)
                return;
            dispatcher.Invoke(() =>
            {
                if (ugoiraImage.index >= ugoiraImage.bytesofimages.Count)
                    ugoiraImage.index = 0;
                image.Source = ImageProcessor.Bytes2Image2(ugoiraImage.bytesofimages[ugoiraImage.index]);
            });
            Thread.Sleep(ugoiraImage.delays[ugoiraImage.index++]);
            Ugoira(dispatcher, ugoiraImage, image);
        }
    }
}
