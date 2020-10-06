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
    }
}
