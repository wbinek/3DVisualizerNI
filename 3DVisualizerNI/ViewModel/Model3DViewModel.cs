using _3DVisualizerNI.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using System.Windows.Media.Media3D;

namespace _3DVisualizerNI.ViewModel
{
    /// <summary>
    /// ViewModel connecting 3D viewport with displayed models.
    /// </summary>
    public class Model3DViewModel : ViewModelBase
    {

        private Model3DGroup model3DContent;
        private Model3DGroup spatialResponse3DContent;
        private Model3DGroup intersectionPoints3DContent;

        public Model3DGroup Model3DContent
        {
            get
            {
                return model3DContent;
            }
            set
            {
                if (value != this.model3DContent)
                {
                    model3DContent = value;
                    RaisePropertyChanged("Model3DContent");
                }
            }
        }

        public Model3DGroup SpatialResponse3DContent
        {
            get
            {
                return spatialResponse3DContent;
            }
            set
            {
                if (value != this.spatialResponse3DContent)
                {
                    spatialResponse3DContent = value;
                    RaisePropertyChanged("SpatialResponse3DContent");
                }
            }
        }

        public Model3DGroup IntersectionPoints3DContent
        {
            get
            {
                return intersectionPoints3DContent;
            }
            set
            {
                if (value != this.intersectionPoints3DContent)
                {
                    intersectionPoints3DContent = value;
                    RaisePropertyChanged("IntersectionPoints3DContent");
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

            Messenger.Default.Register<IntersectionPoints>
            (
                this,
                (ip) => ReceiveIntersectionPoints(ip)
            );

            Messenger.Default.Register<SpatialMeasurement>
            (
                this,
                (sm) => ReceiveResponse(sm)
            );
        }

        private object ReceiveScene(Scene3D scene3D)
        {
            Model3DContent = scene3D.model;
            return null;
        }

        private object ReceiveResponse(SpatialMeasurement sm)
        {
            SpatialResponse3DContent = sm.responseModel;
            return null;
        }

        private object ReceiveIntersectionPoints(IntersectionPoints ip)
        {
            IntersectionPoints3DContent = ip.intersectionModel;
            return null;
        }
    }
}