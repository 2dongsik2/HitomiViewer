using HitomiViewer;
using HitomiViewer.Plugin;
using HitomiViewer.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PluginExample
{
    public class Main : Plugin
    {
        private Dictionary<HitomiPanel, StackPanel> parodysDict = new Dictionary<HitomiPanel, StackPanel>();

        public override void OnInit(MainWindow main)
        {
            base.OnInit(main);
            main.DarkMode.IsChecked = true;
        }

        public override void OnHitomiPanelInit(HitomiPanel panel)
        {
            base.OnHitomiPanelInit(panel);
            if (!panel.large) return;
            if (panel.h.parodys != null && panel.h.parodys.Count > 0)
            {
                StackPanel parodysStack = new StackPanel();
                DockPanel parodysPanel = new DockPanel();
                parodysStack.Name = "parodysStack";
                {
                    Label parodysLabel = new Label();
                    parodysLabel.Content = "원작 : ";
                    parodysPanel.Children.Add(parodysLabel);
                }
                parodysStack.Children.Add(parodysPanel);
                for (int i = 0; i < panel.h.parodys.Count; i++)
                {
                    Hitomi.DisplayValue dv = panel.h.parodys[i];
                    if (i != 0)
                    {
                        Label dot = new Label();
                        dot.Content = ", ";
                        dot.Padding = new Thickness(0, 5, 2.5, 5);
                        parodysPanel.Children.Add(dot);
                    }
                    Label lb = new Label();
                    lb.Content = dv.Display;
                    lb.Foreground = new SolidColorBrush(Global.artistsclr);
                    lb.Cursor = Cursors.Hand;
                    lb.Padding = new Thickness(0, 5, 0, 5);
                    parodysPanel.Children.Add(lb);
                }
                parodysStack.Orientation = Orientation.Vertical;
                DockPanel.SetDock(parodysStack, Dock.Top);
                parodysDict.Add(panel, parodysStack);
                panel.AdditionalPanel.Children.Add(parodysStack);
                panel.AdditionalPanel.Visibility = Visibility.Visible;
            }
        }

        public override void OnHitomiPanelDelayInit(HitomiPanel panel)
        {
            base.OnHitomiPanelDelayInit(panel);
            if (!panel.large) return;
            if (panel.h.parodys != null && panel.h.parodys.Count > 0)
                panel.panel.Height += 26;
        }

        public override void OnHitomiChangeColor(HitomiPanel panel)
        {
            base.OnHitomiChangeColor(panel);
            if (!parodysDict.ContainsKey(panel)) return;
            StackPanel parodysStack = parodysDict[panel];
            (parodysStack.Children[0] as DockPanel).Background = new SolidColorBrush(Global.Menuground);
            ((parodysStack.Children[0] as DockPanel).Children[0] as Label).Foreground = new SolidColorBrush(Global.fontscolor);
        }
    }
}
