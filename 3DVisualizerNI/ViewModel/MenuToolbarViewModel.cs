using _3DVisualizerNI.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace _3DVisualizerNI.ViewModel
{
    public class MenuToolbarViewModel : ViewModelBase
    {
        Scene3D scene3D;
        SpatialMeasurement spatialMeasurement;
        
        public RelayCommand LoadModelCommand { get; private set; }
        public RelayCommand LoadMeasurementCommand { get; private set; }

        public MenuToolbarViewModel()
        {
            this.LoadModelCommand = new RelayCommand(this.LoadModel);
            this.LoadMeasurementCommand = new RelayCommand(this.LoadMeasurement);
        }

        public void LoadModel()
        {
            scene3D = new Scene3D();
            scene3D.LoadModel();

            Messenger.Default.Send<Scene3D>(scene3D);
        }

        public void LoadMeasurement()
        {
            spatialMeasurement = new SpatialMeasurement();
            spatialMeasurement.importWaveResult();

            Messenger.Default.Send<SpatialMeasurement>(spatialMeasurement);
        }
    }
}
