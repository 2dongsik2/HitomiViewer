using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace HitomiViewer.Scripts
{
    class Invoker
    {
        public static void Invoke(Action Method)
        {
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Normal, Method);
        }
        public static void Invoke(Dispatcher dispatcher, Action Method)
        {
            dispatcher.Invoke(DispatcherPriority.Normal, Method);
        }
    }
}
