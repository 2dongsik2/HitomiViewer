using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace HitomiViewer.UserControls
{
    public class IgnoreWheelScrollViewer : ScrollViewer
    {
        private ScrollBar verticalScrollbar;

        public override void OnApplyTemplate()
        {
            // Call base class
            base.OnApplyTemplate();

            // Obtain the vertical scrollbar
            this.verticalScrollbar = this.GetTemplateChild("PART_VerticalScrollBar") as ScrollBar;
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            // Only handle this message if the vertical scrollbar is in use
            if (this.verticalScrollbar != null &&
                this.verticalScrollbar.Visibility == System.Windows.Visibility.Visible &&
                this.verticalScrollbar.IsEnabled)
            {
                // Perform default handling
                base.OnMouseWheel(e);
            }
        }
    }
}
