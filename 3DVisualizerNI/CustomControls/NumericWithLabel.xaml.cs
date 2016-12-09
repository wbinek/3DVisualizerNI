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

namespace _3DVisualizerNI.CustomControls
{
    /// <summary>
    /// Interaction logic for NumericWithLabel.xaml
    /// </summary>
    public partial class NumericWithLabel : UserControl
    {
        public string Label { 
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public string Format
        {
            get { return (string)GetValue(FormatProperty); }
            set { SetValue(FormatProperty, value); }
        }

        public static readonly DependencyProperty LabelProperty = DependencyProperty
        .Register("Label",
                typeof(string),
                typeof(NumericWithLabel),
                new FrameworkPropertyMetadata("Unnamed Label"));

        public static readonly DependencyProperty FormatProperty = DependencyProperty
        .Register("Format",
            typeof(string),
            typeof(NumericWithLabel),
            new FrameworkPropertyMetadata("N3"));

        public static readonly DependencyProperty ValueProperty = DependencyProperty
            .Register("Value",
                    typeof(double),
                    typeof(NumericWithLabel),
                    new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public NumericWithLabel()
        {           
            InitializeComponent();
            Root.DataContext = this;
        }
    }
}
