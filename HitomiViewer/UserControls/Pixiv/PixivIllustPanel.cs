using HitomiViewer.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitomiViewer.UserControls.Panels
{
    class PixivIllustPanel : IHitomiPanel
    {
        private new readonly Pixiv.Illust h;

        public PixivIllustPanel(Pixiv.Illust h, bool large = true, bool file = false, bool blur = false)
        {
            this.large = large;
            this.file = file;
            this.blur = blur;
            this.h = h;
            base.h = new IHitomi();
            base.h.thumbnail.preview_url = h.image_urls.LargestSizeUrl();

            Init();
            InitEvent();
        }
    }
}
