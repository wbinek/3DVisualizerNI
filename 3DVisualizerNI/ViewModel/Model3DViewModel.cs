using _3DVisualizerNI.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace _3DVisualizerNI.ViewModel
{
    public class Model3DViewModel : ViewModelBase
    {
        private Model3DGroup model3DContent;

        public Model3DGroup Model3DContent
        {
            get {
                return model3DContent;
            }
            set {
                if (value != this.model3DContent)
                {
                    model3DContent = value;
                    RaisePropertyChanged("Model3DContent");
                }
            }
        }

        public Model3DViewModel()
        {
            Messenger.Default.Register<Scene3D>
            (
                this,
                (scene3D) => ReceiveScene(scene3D)
            );
        }

        private object ReceiveScene(Scene3D scene3D)
        {
            Model3DContent = scene3D.model;            
            return null;
        }
    }
}
