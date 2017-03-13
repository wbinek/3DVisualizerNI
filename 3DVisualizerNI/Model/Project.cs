using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;

namespace _3DVisualizerNI.Model
{
    class Project
    {
        public ObservableCollection<SpatialMeasurement> MeasurementList;

        public Project()
        {
            MeasurementList = new ObservableCollection<SpatialMeasurement>();

            Messenger.Default.Register<SpatialMeasurement>
            (
                this,
                "AddToList",
                (sm) => ReceiveResponse(sm)
            );
        }

        private object ReceiveResponse(SpatialMeasurement sm)
        {
            MeasurementList.Add(sm);
            return null;
        }
    }
}
