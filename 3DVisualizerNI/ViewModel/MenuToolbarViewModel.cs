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

        public RelayCommand LoadModelCommand { get; private set; }

        public MenuToolbarViewModel()
        {
            this.LoadModelCommand = new RelayCommand(this.LoadModel);
        }

        public void LoadModel()
        {
            scene3D = new Scene3D();
            scene3D.LoadModel();

            Messenger.Default.Send<Scene3D>(scene3D);
        }
    }
}
