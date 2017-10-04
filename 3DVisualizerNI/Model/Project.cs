using System;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight.Messaging;
using System.IO;
using System.Xml.Serialization;

namespace _3DVisualizerNI.Model
{
    [Serializable]
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

        public void WriteToBinaryFile(string filePath)
        {
            using (Stream stream = File.Open(filePath, FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, this);
            }
        }

        public void LoadFromBinaryFile(string filePath)
        {
            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                Project loadedProj = (Project)binaryFormatter.Deserialize(stream);
                this.MeasurementList = loadedProj.MeasurementList;
                onLoad();
            }
        }

        private void onLoad()
        {
            foreach(SpatialMeasurement measurement in MeasurementList)
            {
                measurement.onPorjectLoad();
            }
        }

        private object ReceiveResponse(SpatialMeasurement sm)
        {
            MeasurementList.Add(sm);
            return null;
        }
    }
}
