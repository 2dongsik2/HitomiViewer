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
        }

        public override void OnHitomiPanelDelayInit(HitomiPanel panel)
        {
            base.OnHitomiPanelDelayInit(panel);
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
