using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _3DVisualizerNI.CustomControls
{
    public class Notifier : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    /// <summary>
    /// Interaction logic for Vector3DControll.xaml
    /// </summary>
    public partial class Vector3DControl : UserControl, INotifyPropertyChanged
    {
        public double X
        {
            get { return Vector.X; }
            set
            {
                Vector3D temp = Vector;
                temp.X = value;
                Vector = temp;
            }
        }

        public double Y
        {
            get { return Vector.Y; }
            set
            {
                Vector3D temp = Vector;
                temp.Y = value;
                Vector = temp;
            }
        }

        public double Z
        {
            get { return Vector.Z; }
            set
            {
                Vector3D temp = Vector;
                temp.Z = value;
                Vector = temp;
            }
        }

        public Vector3D Vector
        {
            get { return (Vector3D)GetValue(VectorProperty); }
            set { SetValue(VectorProperty, value); }
        }

        public void update()
        {
            RaisePropertyChanged("X");
            RaisePropertyChanged("Y");
            RaisePropertyChanged("Z");
        }

        public static readonly DependencyProperty VectorProperty = DependencyProperty
        .Register("Vector",
        typeof(Vector3D),
        typeof(Vector3DControl),
        new FrameworkPropertyMetadata(new Vector3D(0,0,0), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnVectorChanged)));

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public Vector3DControl()
        {
            InitializeComponent();
            Root.DataContext = this;
        }

        private static void OnVectorChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            Vector3DControl control = (Vector3DControl) sender;
            control.update();
        }
    }
}
