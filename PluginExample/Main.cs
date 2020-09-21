using HitomiViewer;
using HitomiViewer.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginExample
{
    public class Main : Plugin
    {
        public override void OnInit(MainWindow main)
        {
            base.OnInit(main);
            main.DarkMode.IsChecked = true;
        }
    }
}
