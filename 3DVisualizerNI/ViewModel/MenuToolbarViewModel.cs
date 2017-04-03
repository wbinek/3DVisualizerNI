using _3DVisualizerNI.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Win32;
using _3DVisualizerNI.Views;

namespace _3DVisualizerNI.ViewModel
{
    /// <summary>
    /// View Model connecting menu toolbar with appropriate classes.
    /// </summary>
    public class MenuToolbarViewModel : ViewModelBase
    {
        private Scene3D scene3D;
        private SpatialMeasurement spatialMeasurement;

        public MenuToolbarViewModel()
        {
            this.LoadModelCommand = new RelayCommand(this.LoadModel);
            this.LoadMeasurementCommand = new RelayCommand(this.LoadMeasurement);
            this.MakeMeasurementCommand = new RelayCommand(this.MakeMeasurement);
            this.NewProjectCommand = new RelayCommand(this.NewProject);
        }

        public RelayCommand LoadMeasurementCommand { get; private set; }
        public RelayCommand MakeMeasurementCommand { get; private set; }
        public RelayCommand LoadModelCommand { get; private set; }
        public RelayCommand NewProjectCommand { get; private set; }
        public void LoadMeasurement()
        {
            //Get File Path
            OpenFileDialog OpenDialog = new OpenFileDialog();
            OpenDialog.Filter = "wav files (*.wav)|*.wav";
            OpenDialog.Multiselect = true;

            if (OpenDialog.ShowDialog() == true)
            {
                foreach (var path in OpenDialog.FileNames)
                {
                    spatialMeasurement = new SpatialMeasurement();
                    spatialMeasurement.importWaveResult(path);
                    Messenger.Default.Send<SpatialMeasurement>(spatialMeasurement, "AddToList");
                }              
            }         
        }

        public void LoadModel()
        {
            scene3D = new Scene3D();
            scene3D.LoadModel();

            Messenger.Default.Send<Scene3D>(scene3D);
        }

        public void NewProject()
        {
            Project project = new Project();
            Messenger.Default.Send<Project>(project);
        }

        public void MakeMeasurement()
        {
            MakeMeasurementWindow measWindow = new MakeMeasurementWindow();
            MakeMeasurementViewModel measViewModel = new MakeMeasurementViewModel();
            measWindow.DataContext = measViewModel;
            measWindow.Show();
        }
    }
}