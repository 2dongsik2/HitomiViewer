using ExtensionMethods;
using HitomiViewer.Processor;
using HitomiViewer.Processor.Loaders;
using HitomiViewerLibrary;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
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

namespace HitomiViewer.UserControls.Panels
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
            }), null, sourceName: MethodBase.GetCurrentMethod().FullName());

            Global.Account.Pixiv.userDetail(user.user.id.ToString()).then((JObject detail) => dispatcher.Invoke(() =>
            {
                IllustsCount.Content = detail["profile"].IntValue("total_illusts") + detail["profile"].IntValue("total_manga");
                description.Content = detail["user"].StringValue("comment");
            }), null, sourceName: MethodBase.GetCurrentMethod().FullName());

        }
        private void Ugoira(Dispatcher dispatcher, PixivUgoira ugoiraImage, Image image)
        {
            if (this.Visibility != Visibility.Visible)
                return;
            if (!image.IsMouseOver)
                return;
            if (ugoiraImage.index >= ugoiraImage.bytesofimages.Count)
                ugoiraImage.index = 0;
            image.Source = ImageProcessor.Bytes2Image2(ugoiraImage.bytesofimages[ugoiraImage.index]);
            Thread.Sleep(ugoiraImage.delays[ugoiraImage.index++]);
            Ugoira(dispatcher, ugoiraImage, image);
        }
    }
}
