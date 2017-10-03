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
            TextWriter writer = null;
            try
            {
                var serializer = new XmlSerializer(typeof(Project));
                writer = new StreamWriter(filePath, false);
                serializer.Serialize(writer, this);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }

        public void LoadFromBinaryFile(string filePath)
        {
            TextReader reader = null;
            try
            {
                var serializer = new XmlSerializer(typeof(Project));
                reader = new StreamReader(filePath);
                Project loaded =  (Project)serializer.Deserialize(reader);
                this.MeasurementList = loaded.MeasurementList;
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }

        private object ReceiveResponse(SpatialMeasurement sm)
        {
            MeasurementList.Add(sm);
            return null;
        }
    }
}
