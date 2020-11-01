using HitomiViewerLibrary.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HitomiViewer.UserControls
{
    /// <summary>
    /// tag.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TagControl : UserControl
    {
        public Hitomi.HTag.TType TagType
        {
            get { return (Hitomi.HTag.TType)GetValue(TagTypeProperty); }
            set { SetValue(TagTypeProperty, value); }
        }
        public string TagName
        {
            get { return (string)GetValue(TagNameProperty); }
            set { SetValue(TagNameProperty, value); }
        }
        public Brush TagColor {
            get { return (Brush)GetValue(TagColorProperty); }
            set { SetValue(TagColorProperty, value); }
        }

        private string searchName = null;
        public string SearchName
        {
            get => searchName ?? TagName;
            set => searchName = value;
        }

        public TagControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        public static readonly DependencyProperty TagTypeProperty
            = DependencyProperty.Register(
                "TagType",
                typeof(Hitomi.HTag.TType),
                typeof(TagControl),
                new PropertyMetadata(Hitomi.HTag.TType.tag)
            );
        public static readonly DependencyProperty TagNameProperty
            = DependencyProperty.Register(
                "TagName",
                typeof(string),
                typeof(TagControl),
                new PropertyMetadata("태그없음")
            );
        public static readonly DependencyProperty TagColorProperty
            = DependencyProperty.Register(
                "TagColor",
                typeof(Brush),
                typeof(TagControl),
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(255, 94, 94)))
            );
    }
}
