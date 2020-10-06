using HitomiViewer.UserControls;
using Newtonsoft.Json.Linq;
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
        void UnknownFileLoaded(int TypeId, JObject data);
    }

    public abstract class Plugin : IPlugin
    {
        public virtual void OnInit(MainWindow main) { }
        public virtual void OnDelayInit(MainWindow main) { }
        public virtual void UnknownFileLoaded(int TypeId, JObject data) { }
    }
}
