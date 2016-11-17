using _3DVisualizerNI.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace _3DVisualizerNI.ViewModel
{
    public class PropertiesViewModel : ViewModelBase
    {
        SpatialMeasurement spatialMeasurement;

        public ObservableCollection<int> ResolutionList { get; set; }
        public int ResolutionSelected {
            get { if(spatialMeasurement != null) return spatialMeasurement.resolution;
                return 5;
            }
            set
            {
                spatialMeasurement.resolution = value;
                Messenger.Default.Send<SpatialMeasurement>(spatialMeasurement);
            }
        }
        public double pX
        {
            get
            {
                if (spatialMeasurement != null) return spatialMeasurement.position.X;
                return 0;
            }
            set
            {
                Vector3D newPos = spatialMeasurement.position;
                newPos.X = value;
                spatialMeasurement.position = newPos;
                Messenger.Default.Send<SpatialMeasurement>(spatialMeasurement);
            }
        }
        public double pY
        {
            get
            {
                if (spatialMeasurement != null) return spatialMeasurement.position.Y;
                return 0;
            }
            set
            {
                Vector3D newPos = spatialMeasurement.position;
                newPos.Y = value;
                spatialMeasurement.position = newPos;
                Messenger.Default.Send<SpatialMeasurement>(spatialMeasurement);
            }
        }
        public double pZ
        {
            get
            {
                if (spatialMeasurement != null) return spatialMeasurement.position.Z;
                return 0;
            }
            set
            {
                Vector3D newPos = spatialMeasurement.position;
                newPos.Z = value;
                spatialMeasurement.position = newPos;
                Messenger.Default.Send<SpatialMeasurement>(spatialMeasurement);
            }
        }

        public PropertiesViewModel()
        {
            ResolutionList = new ObservableCollection<int>();
            ResolutionList.Add(1);
            ResolutionList.Add(2);
            ResolutionList.Add(5);
            ResolutionList.Add(10);
            ResolutionList.Add(15);

            Messenger.Default.Register<SpatialMeasurement>
            (
                this,
                (sm) => ReceiveResponse(sm)
            );
        }

        private object ReceiveResponse(SpatialMeasurement sm)
        {
            spatialMeasurement = sm;
            return null;
        }
    }
}
