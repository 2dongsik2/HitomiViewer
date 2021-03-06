﻿using ExtensionMethods;
using HitomiViewer.Encryption;
using HitomiViewer.Plugin;
using HitomiViewer.Processor;
using HitomiViewer.Processor.Loaders;
using HitomiViewer.Scripts;
using HitomiViewer.UserControls.Reader;
using HitomiViewerLibrary;
using HitomiViewerLibrary.Loaders;
using HitomiViewerLibrary.Structs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Path = System.IO.Path;

#pragma warning disable CS0618 // 형식 또는 멤버는 사용되지 않습니다.
namespace HitomiViewer.UserControls.Panels
{
    /// <summary>
    /// HitomiPanel.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class HitomiPanel : IHitomiPanel
    {
        public new Hitomi h;

        public HitomiPanel(Hitomi h, bool large = true, bool file = false, bool blur = false)
        {
            this.large = large;
            this.file = file;
            this.blur = blur;
            this.h = h;
            base.h = this.h;
            InitializeComponent();
            Init();
            InitEvent();
        }

        public override void InitEvent()
        {
            thumbNail.MouseDown += (object sender, MouseButtonEventArgs e) =>
            {
                HitomiReader reader = new HitomiReader(h);
                if (!reader.IsClosed)
                    reader.Show();
            };
        }
        public override void Init()
        {
            if (h.files == null)
                h.files = new Hitomi.HFile[0];
            BitmapImage image = h.thumbnail.preview_img.ToImage();
            thumbNail.Source = image;
            thumbBrush.ImageSource = image;
            nameLabel.Content = h.name;
            pageLabel.Content = h.files.Length;
            sizeLabel.Content = CF.File.SizeStr(h.FileInfo.size);
            if (h.files.Length > 0)
                sizeperpageLabel.Content = CF.File.SizeStr(h.FileInfo.size / h.files.Length);
            TagsInit();
            ChangeColor();
        }
        private void TagsInit()
        {
            if (h.tags == null) return;
            foreach (Hitomi.HTag value in h.tags)
            {
                TagControl tagElem = new TagControl
                {
                    TagName = value.full
                };
                if (value.ttype == Hitomi.HTag.TType.female)
                    tagElem.TagColor = new SolidColorBrush(Color.FromRgb(255, 94, 94));
                else if (value.ttype == Hitomi.HTag.TType.male)
                    tagElem.TagColor = new SolidColorBrush(Color.FromRgb(65, 149, 244));
                else
                    tagElem.TagColor = new SolidColorBrush(Color.FromRgb(153, 153, 153));
                tagPanel.Children.Add(tagElem);
            }
        }
        public override void ContextSetup()
        {
            Folder_Remove.Visibility = Visibility.Collapsed;
            Folder_Hiyobi_Search.Visibility = Visibility.Collapsed;
            Hiyobi_Download.Visibility = Visibility.Collapsed;
            Pixiv_Download.Visibility = Visibility.Collapsed;
            AtHitomi.Visibility = Visibility.Collapsed;
            Encrypt.Visibility = Visibility.Collapsed;
            Decrypt.Visibility = Visibility.Collapsed;
            CopyNumber.Visibility = Visibility.Visible;
            if (file)
            {
                Folder_Remove.Visibility = Visibility.Visible;
                if (Global.config.password.Get<string>() != null)
                {
                    Encrypt.Visibility = Visibility.Visible;
                    Decrypt.Visibility = Visibility.Visible;
                }
            }
            if (h.id == null || h.id == "")
            {
                DownloadData.Visibility = Visibility.Collapsed;
                DownloadImage.Visibility = Visibility.Collapsed;
                CopyNumber.Visibility = Visibility.Collapsed;
            }
            ConfigFile cfg = new ConfigFile();
            ConfigFileData obj = cfg.config;
            List<string> favs = obj.favorites.Get<List<string>>();
            if (favs.Contains(h.id))
            {
                Favorite.Visibility = Visibility.Collapsed;
                FavoriteRemove.Visibility = Visibility.Visible;
            }
            else
            {
                Favorite.Visibility = Visibility.Visible;
                FavoriteRemove.Visibility = Visibility.Collapsed;
            }
        }

        public override void ChangeColor()
        {
            base.ChangeColor();
        }

        public override async void HitomiPanel_Loaded(object sender, RoutedEventArgs e)
        {
            this.nameLabel.Content = h.name + " (로딩중)";
            InternetP parser = new InternetP();
            parser.index = int.Parse(h.id);
            h = await parser.HitomiData();
            //h = await parser.HitomiData2(h, int.Parse(h.id));
            if (Global.config.origin_thumb.Get<bool>() && h.files != null && h.files.Length >= 1 && h.files[0] != null)
            {
                this.pageLabel.Content = h.files.Length;
                this.nameLabel.Content = h.name + " (썸네일 로딩중)";
                ImageProcessor.ProcessAsync(h.files[0].url).then((byte[] image) =>
                {
                    h.thumbnail.preview_img = image;
                    base.h = this.h;
                    this.nameLabel.Content = h.name;
                }, null, sourceName: MethodBase.GetCurrentMethod().FullName());
            }
            else
            {
                var preview_image = await ImageProcessor.ProcessAsync(h.thumbnail.preview_url).@catch(null, MethodBase.GetCurrentMethod().FullName());
                h.thumbnail.preview_img = preview_image;
                base.h = this.h;
                this.nameLabel.Content = h.name;
            }
            BitmapImage image = h.thumbnail.preview_img.ToImage();
            thumbNail.Source = image;
            thumbBrush.ImageSource = image;
            thumbNail.MouseEnter += (object _, MouseEventArgs __) => thumbNail.ToolTip = GetToolTip(panel.Height);
            Init();
        }

        public override void Folder_Remove_Click(object sender, RoutedEventArgs e)
        {
            
        }
        public override void Folder_Open_Click(object sender, RoutedEventArgs e)
        {
            
        }
        public override void CopyNumber_Click(object sender, RoutedEventArgs e) => Clipboard.SetText(h.id);
        public override void Hitomi_Download_Click(object sender, RoutedEventArgs e)
        {
            
        }
        public override void Encrypt_Click(object sender, RoutedEventArgs e)
        {
            
        }
        public override void Decrypt_Click(object sender, RoutedEventArgs e)
        {

        }
        public override void Favorite_Click(object sender, RoutedEventArgs e)
        {
            
        }
        public override void FavoriteRemove_Click(object sender, RoutedEventArgs e)
        {
            
        }
        public override void authorLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }
        public override void DownloadData_Click(object sender, RoutedEventArgs e)
        {
            
        }
        public override void DownloadImage_Click(object sender, RoutedEventArgs e)
        {
            
        }
        public override async void ImageDownloadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            h.thumbnail.preview_url = h.files.First().url;
            h.thumbnail.preview_img = await ImageProcessor.ProcessAsync(h.thumbnail.preview_url).@catch(null, sourceName: MethodBase.GetCurrentMethod().FullName());
            Init();
        }
        public override void tagScroll_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scroll = Global.MainWindow.MainScroll;
            double offset = 0.45;
            if (e.Delta > 0)
                scroll.ScrollToVerticalOffset(scroll.VerticalOffset - (offset * e.Delta));
            else
                scroll.ScrollToVerticalOffset(scroll.VerticalOffset - (offset * e.Delta));
            e.Handled = true;
        }

        public override void AtHitomi_Click(object sender, RoutedEventArgs e)
        {
            //HiyobiLoader hiyobi = new HiyobiLoader();
            //hiyobi.Default();
            //await hiyobi.Parser(hiyobiList.ToArray());
            HitomiLoader hitomi = new HitomiLoader();
            hitomi.Default();
            hitomi.Parser(new int[] { int.Parse(h.id) });
        }
    }
}
#pragma warning restore CS0618 // 형식 또는 멤버는 사용되지 않습니다.