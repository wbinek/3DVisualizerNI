using System.ComponentModel;
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
            Closing += OnWindowClosing;
        }

        private void OnWindowClosing(object obj, CancelEventArgs arg)
        {
            Application.Current.Shutdown();
        }
    }
}