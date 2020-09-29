using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitomiViewer.UserControls.Panels
{
    class HiyobiPanel : IHitomiPanel
    {
        public new HiyobiGallery h;

        public HiyobiPanel(HiyobiGallery h, bool large = true, bool file = false, bool blur = false)
        {
            this.large = large;
            this.file = file;
            this.blur = blur;
            this.h = h;
            InitializeComponent();
            Init();
            InitEvent();
        }
    }
}
