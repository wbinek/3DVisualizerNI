using System;
using System.Collections;
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

namespace _3DVisualizerNI.CustomControls
{
    /// <summary>
    /// Interaction logic for ComboWithLabel.xaml
    /// </summary>
    public partial class ComboWithLabel : UserControl
    {
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public object Item
        {
            get { return (object)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        public IEnumerable List
        {
            get { return (IEnumerable)GetValue(ListProperty); }
            set { SetValue(ListProperty, value); }
        }

        public static readonly DependencyProperty LabelProperty = DependencyProperty
        .Register("Label",
                typeof(string),
                typeof(ComboWithLabel),
                new FrameworkPropertyMetadata("Unnamed Label"));

        public static readonly DependencyProperty ListProperty = DependencyProperty
        .Register("List",
            typeof(IEnumerable),
            typeof(ComboWithLabel),
            new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty ItemProperty = DependencyProperty
            .Register("Item",
                    typeof(object),
                    typeof(ComboWithLabel),
                    new FrameworkPropertyMetadata(null,FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public ComboWithLabel()
        {
            InitializeComponent();
            Root.DataContext = this;
        }
    }
}
