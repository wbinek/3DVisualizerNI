using _3DVisualizerNI.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

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
        }

        public RelayCommand LoadMeasurementCommand { get; private set; }
        public RelayCommand LoadModelCommand { get; private set; }
        public void LoadMeasurement()
        {
            spatialMeasurement = new SpatialMeasurement();
            spatialMeasurement.importWaveResult();

            Messenger.Default.Send<SpatialMeasurement>(spatialMeasurement);
        }

        public void LoadModel()
        {
            scene3D = new Scene3D();
            scene3D.LoadModel();

            Messenger.Default.Send<Scene3D>(scene3D);
        }
    }
}