using HelixToolkit.Wpf;
using Microsoft.Win32;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace _3DVisualizerNI.Model
{
    [Serializable]
    public class SpatialMeasurement
    {
        private double[] w, x, y, z;
        private double Fs { get; set; }
        private int _resolution = 5;
        private Vector3D _position;


        public int resolution
        {
            get { return _resolution; }
            set {
                if (_resolution != value)
                {
                    _resolution = value;
                    buildResponseModel();
                }
            }
        }
        public Vector3D position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
                setPosition();
            }
        }
      
        public Model3DGroup responseModel { get; set; }

        public SpatialMeasurement() {}

        public void importWaveResult()
        {
            //Get File Path
            string path;
            OpenFileDialog OpenDialog = new OpenFileDialog();
            OpenDialog.Filter = "wav files (*.wav)|*.wav";

            if (OpenDialog.ShowDialog() == true)
            {
                path = OpenDialog.FileName;
                AudioFileReader reader = new AudioFileReader(path);

                if (reader.WaveFormat.Channels != 4)
                {
                    MessageBox.Show("Wave doesn't contain four channels. Can't import.");
                    return;
                }

                List<float> _w = new List<float>();
                List<float> _x = new List<float>();
                List<float> _y = new List<float>();
                List<float> _z = new List<float>();

                float[] buffer = new float[reader.WaveFormat.Channels];
                Fs = reader.WaveFormat.SampleRate;
                while (reader.Read(buffer, 0, reader.WaveFormat.Channels) > 0)
                {
                    _w.Add(buffer[0]);
                    _x.Add(buffer[1]);
                    _y.Add(buffer[2]);
                    _z.Add(buffer[3]);
                }

                w = Array.ConvertAll(_w.ToArray(), s => (double)s);
                x = Array.ConvertAll(_x.ToArray(), s => (double)s);
                y = Array.ConvertAll(_y.ToArray(), s => (double)s);
                z = Array.ConvertAll(_z.ToArray(), s => (double)s);

                buildResponseModel();
            }

        }

        public void buildResponseModel()
        {
            responseModel = new Model3DGroup();
            Point3D center = new Point3D(0, 0, 0);

            //Calculate the amount of energy with specified resolution
            int bins = 360 / resolution;

            double[,] r = new double[bins/2, bins];

            for (int i = 0; i < w.Length; i++)
            {
                Vector3D vector = MyVector3D.toSphericalDeg(x[i], y[i], z[i]);
                r[(int)(vector.X / resolution), (int)(vector.Y / resolution)] += w[i] * w[i];               
            }

            //Add cones to model to create a view of spatial impulse response
            for (int i = 0; i < bins/2; i++)
                for (int j = 0; j < bins; j++)
            {
                {
                    TruncatedConeVisual3D cone = new TruncatedConeVisual3D();

                    cone.Origin = center;
                    cone.Height = r[i,j]*100;
                    cone.Normal = MyVector3D.toCartesianDeg(i*resolution,j*resolution,r[i,j]);
                    cone.BaseRadius = 0.0001;
                    cone.TopRadius = 0.01;
                    responseModel.Children.Add(cone.Content);
                }
            }
        }

        public void setPosition()
        {
           
            Transform3DGroup newTransform = new Transform3DGroup();

            newTransform.Children.Add(new TranslateTransform3D(position));
            responseModel.Transform = newTransform;
        }
    }
}
