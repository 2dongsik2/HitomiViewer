using ExtensionMethods;
using HitomiViewer.Api;
using HitomiViewer.Encryption;
using HitomiViewer.Plugin;
using HitomiViewer.Processor;
using HitomiViewer.Processor.Loaders;
using HitomiViewer.Scripts;
using HitomiViewer.Structs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Path = System.IO.Path;

#pragma warning disable CS0618 // 형식 또는 멤버는 사용되지 않습니다.
namespace HitomiViewer.UserControls
{
    /// <summary>
    /// HitomiPanel.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class IHitomiPanel : UserControl
    {
        public IHitomi h;
        public bool large;
        public bool file;
        public bool blur;
        public IHitomiPanel()
        {
            InitializeComponent();
            Init();
            InitEvent();
        }

        public virtual void InitEvent() { }
        public virtual void Init() { }

        public virtual void ContextSetup() { }

        public ToolTip GetToolTip(double height)
        {
            if (h.thumbnail.preview_img == null) return null;
            if (!thumbNail.IsVisible) return null;
            //b = 비율
            //Magnif = 배율
            double b = height / h.thumbnail.preview_img.Height;
            double top = thumbNail.PointToScreen(new Point(0, 0)).Y;
            double bottom = top + thumbNail.ActualHeight;
            double WorkHeight = SystemParameters.WorkArea.Bottom;
            double MagnifSize = height * Global.Magnif;
            double Len = WorkHeight / 2 - (bottom - thumbNail.ActualHeight / 2);
            bool up = Len <= 0;
            double VisualMaxSize = 0;
            if (up)
                VisualMaxSize = top;
            else
                VisualMaxSize = WorkHeight - bottom;
            double size = MagnifSize > VisualMaxSize ? VisualMaxSize : MagnifSize;
            FrameworkElementFactory image = new FrameworkElementFactory(typeof(Image));
            {
                image.SetValue(Image.HeightProperty, size);
                image.SetValue(Image.HorizontalAlignmentProperty, HorizontalAlignment.Left);
                image.SetValue(Image.SourceProperty, h.thumbnail.preview_img);
            }
            FrameworkElementFactory elemborder = new FrameworkElementFactory(typeof(Border));
            {
                elemborder.SetValue(Border.BorderThicknessProperty, new Thickness(1));
                elemborder.SetValue(Border.BorderBrushProperty, new SolidColorBrush(Global.outlineclr));
                elemborder.AppendChild(image);
            }
            FrameworkElementFactory panel = new FrameworkElementFactory(typeof(StackPanel));
            {
                panel.AppendChild(elemborder);
            }
            ToolTip toolTip = new ToolTip
            {
                Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom,
                Template = new ControlTemplate
                {
                    VisualTree = panel
                }
            };
            return toolTip;
        }

        public virtual void ChangeColor() { }

        public virtual void HitomiPanel_Loaded(object sender, RoutedEventArgs e) { }

        public virtual void Folder_Remove_Click(object sender, RoutedEventArgs e) { }
        public virtual void Folder_Open_Click(object sender, RoutedEventArgs e) { }
        public virtual void CopyNumber_Click(object sender, RoutedEventArgs e) => Clipboard.SetText(h.id);
        public virtual void Hiyobi_Download_Click(object sender, RoutedEventArgs e) { }
        public virtual void Hitomi_Download_Click(object sender, RoutedEventArgs e) { }
        public virtual void Pixiv_Download_Click(object sender, RoutedEventArgs e) { }
        public virtual void Folder_Hiyobi_Search_Click(object sender, RoutedEventArgs e) { }
        public virtual void Encrypt_Click(object sender, RoutedEventArgs e) { }
        public virtual void Decrypt_Click(object sender, RoutedEventArgs e) { }
        public virtual void Favorite_Click(object sender, RoutedEventArgs e) { }
        public virtual void FavoriteRemove_Click(object sender, RoutedEventArgs e) { }
        public virtual void authorLabel_MouseDown(object sender, MouseButtonEventArgs e) { }
        public virtual void DownloadData_Click(object sender, RoutedEventArgs e) { }
        public virtual void DownloadImage_Click(object sender, RoutedEventArgs e) { }
        public virtual void ImageDownloadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e) { }
        public virtual void tagScroll_MouseWheel(object sender, MouseWheelEventArgs e) { }
        public virtual void thumbNail_MouseEnter(object sender, MouseEventArgs e) { }

        public virtual void AtHitomi_Click(object sender, RoutedEventArgs e) { }
    }
}
#pragma warning restore CS0618 // 형식 또는 멤버는 사용되지 않습니다.