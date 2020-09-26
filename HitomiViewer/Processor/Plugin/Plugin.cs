using HitomiViewer.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitomiViewer.Plugin
{
    public interface IPlugin
    {
        void OnInit(MainWindow main);
        void OnDelayInit(MainWindow main);
        void OnHitomiPanelInit(HitomiPanel panel);
        void OnHitomiPanelDelayInit(HitomiPanel panel);
        void OnHitomiChangeColor(HitomiPanel panel);
    }

    public abstract class Plugin : IPlugin
    {
        public virtual void OnInit(MainWindow main) { }
        public virtual void OnDelayInit(MainWindow main) { }
        public virtual void OnHitomiPanelInit(HitomiPanel panel) { }
        public virtual void OnHitomiPanelDelayInit(HitomiPanel panel) { }
        public virtual void OnHitomiChangeColor(HitomiPanel panel) { }
    }
}
