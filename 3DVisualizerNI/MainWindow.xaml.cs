using System.Windows;
using _3DVisualizerNI.ViewModel;

namespace _3DVisualizerNI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ((MenuToolbarViewModel)MenuToolbarView.DataContext).NewProject();
        }
    }
}