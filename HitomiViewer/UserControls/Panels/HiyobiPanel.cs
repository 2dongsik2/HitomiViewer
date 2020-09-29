using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace HitomiViewer.UserControls.Panels
{
    class HiyobiPanel : IHitomiPanel
    {

        public HiyobiPanel(HiyobiGallery h, bool large = true, bool file = false, bool blur = false)
        {
            this.large = large;
            this.file = file;
            this.blur = blur;
            this.h = h;
            
            Init();
            InitEvent();
        }

        public override void Init()
        {
            thumbNail.Source = h.thumbnail.preview_img;
            thumbBrush.ImageSource = h.thumbnail.preview_img;
            nameLabel.Content = h.name;
        }

        public override void ChangeColor()
        {
            //Plugin.PluginHandler.FireOnHitomiChangeColor(this);
            panel.Background = new SolidColorBrush(Global.background);
            border.Background = new SolidColorBrush(Global.imagecolor);
            InfoPanel.Background = new SolidColorBrush(Global.Menuground);
            bottomPanel.Background = new SolidColorBrush(Global.Menuground);
            authorsPanel.Background = new SolidColorBrush(Global.Menuground);
            nameLabel.Foreground = new SolidColorBrush(Global.fontscolor);
            sizeLabel.Foreground = new SolidColorBrush(Global.fontscolor);
            pageLabel.Foreground = new SolidColorBrush(Global.fontscolor);
            sizeperpageLabel.Foreground = new SolidColorBrush(Global.fontscolor);
            foreach (UIElement elem in authorsPanel.Children)
                (elem as Label).Foreground = new SolidColorBrush(Global.artistsclr);
            (authorsPanel.Children[0] as Label).Foreground = new SolidColorBrush(Global.fontscolor);
            tagPanel.Background = new SolidColorBrush(Global.Menuground);
        }
        public static void ChangeColor(HitomiPanel hpanel)
        {
            DockPanel panel = hpanel.panel as DockPanel;

            Border border = panel.Children[0] as Border;
            DockPanel InfoPanel = panel.Children[1] as DockPanel;

            StackPanel bottomPanel = InfoPanel.Children[1] as StackPanel;
            StackPanel authorsStackPanel = InfoPanel.Children[2] as StackPanel;

            DockPanel authorsPanel = authorsStackPanel.Children[0] as DockPanel;

            Label nameLabel = InfoPanel.Children[0] as Label;

            Label sizeLabel = bottomPanel.Children[0] as Label;
            Label pageLabel = bottomPanel.Children[2] as Label;
            Label sizeperpageLabel = bottomPanel.Children[4] as Label;

            StackPanel tagPanel = InfoPanel.Children[2] as StackPanel;

            panel.Background = new SolidColorBrush(Global.background);
            border.Background = new SolidColorBrush(Global.imagecolor);
            InfoPanel.Background = new SolidColorBrush(Global.Menuground);
            bottomPanel.Background = new SolidColorBrush(Global.Menuground);
            nameLabel.Foreground = new SolidColorBrush(Global.fontscolor);
            sizeLabel.Foreground = new SolidColorBrush(Global.fontscolor);
            pageLabel.Foreground = new SolidColorBrush(Global.fontscolor);
            sizeperpageLabel.Foreground = new SolidColorBrush(Global.fontscolor);
            authorsPanel.Children.Cast<UIElement>().Select(x => (x as Label).Foreground = new SolidColorBrush(Global.artistsclr));
            tagPanel.Background = new SolidColorBrush(Global.Menuground);
        }

        public override void HitomiPanel_Loaded(object sender, RoutedEventArgs e)
        {
            base.HitomiPanel_Loaded(sender, e);
            thumbNail.MouseEnter += (object _, MouseEventArgs __) => thumbNail.ToolTip = GetToolTip(panel.Height);
        }
    }
}
