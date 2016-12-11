using HelixToolkit.Wpf;
using Microsoft.Win32;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Media3D;

namespace _3DVisualizerNI.Model
{
    /// <summary>
    /// Static class with conversion methods used in spatial measurement analysis and display
    /// </summary>
    public static class MeasurementUtils
    {
        #region Public Methods

        /// <summary>
        /// Converts pressure to dB
        /// </summary>
        /// <param name="preassure">pressure [Pa}]</param>
        /// <returns></returns>
        public static double todB(double preassure)
        {
            return 10 * Math.Log10(preassure * preassure / (4E-10));
        }

        /// <summary>
        /// Converts sound level to pressure squared
        /// </summary>
        /// <param name="level">level [dB]</param>
        /// <returns></returns>
        public static double toPreassureSquared(double level)
        {
            return (4E-10) * Math.Pow(10, 0.1 * level);
        }

        #endregion Public Methods
    }

    /// <summary>
    /// Class containing basic measurement result data
    /// </summary>
    public class MeasurementData
    {
        #region Private Fields

        /// <summary>
        /// Arrays containing measurement result
        /// </summary>
        private double[] w, x, y, z;

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        /// Measurement sampling frequency
        /// </summary>
        public int Fs { get; set; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Method returning w array
        /// </summary>
        /// <returns>Amplitude array w</returns>
        public double[] getAmplitudeArray()
        {
            return w;
        }

        /// <summary>
        /// Method returning amplitude at specified index
        /// </summary>
        /// <param name="idx"></param>
        /// <returns>Amplitude w at specified index</returns>
        public double getAmplitudeAtIdx(int idx)
        {
            return w[idx];
        }

        /// <summary>
        /// Method returning 3d vector at specified index
        /// </summary>
        /// <param name="idx">sample number</param>
        /// <returns>Vector (x,y,z) at selected index</returns>
        public Vector3D getDirectionAtIdx(int idx)
        {
            return new Vector3D(x[idx], y[idx], z[idx]);
        }

        /// <summary>
        /// Returns measurement length
        /// </summary>
        /// <returns>Measurement length</returns>
        public int getLength()
        {
            return w.Length;
        }

        /// <summary>
        /// Returns max amplitude value
        /// </summary>
        /// <returns>Max amplitude (w) value</returns>
        public double getMax()
        {
            if (w != null)
            {
                return w.Select(x => Math.Abs(x)).Max();
            }
            return 0;
        }

        /// <summary>
        /// Returns index of max value
        /// </summary>
        /// <returns>Index of max amplitude (w) value</returns>
        public int getMaxIdx()
        {
            if (w != null)
            {
                double max = getMax();
                return Array.IndexOf(Array.ConvertAll(w, x => Math.Abs(x)), max);
            }
            return 0;
        }

        /// <summary>
        /// Imports result from wave at given path
        /// </summary>
        /// <param name="path">Path to wavfile</param>
        public void importWaveResult(string path)
        {
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
        }

        #endregion Public Methods
    }

    /// <summary>
    /// Class holding spatial measurement information
    /// </summary>
    [Serializable]
    public class SpatialMeasurement
    {
        #region Private Fields

        /// <summary>
        /// Measurement position
        /// </summary>
        private Vector3D _position;

        /// <summary>
        /// Measurement display resolution
        /// </summary>
        private int _resolution = 5;

        /// <summary>
        /// Measurement display scale
        /// </summary>
        private int _scale = 5;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public SpatialMeasurement()
        {
            responseModel = new Model3DGroup();
            measurementData = new MeasurementData();
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Object containing measurement result
        /// </summary>
        public MeasurementData measurementData;

        /// <summary>
        /// Vector describing measurement position
        /// </summary>
        public Vector3D measurementPosition
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
                setTransforms();
            }
        }

        /// <summary>
        /// Measurement display resolution
        /// </summary>
        public int measurementResolution
        {
            get { return _resolution; }
            set
            {
                if (_resolution != value)
                {
                    _resolution = value;
                    buildResponseModel();
                    setTransforms();
                }
            }
        }

        /// <summary>
        /// Measurement display scale
        /// </summary>
        public int measurementScale
        {
            get
            {
                return _scale;
            }
            set
            {
                _scale = value;
                setTransforms();
            }
        }

        /// <summary>
        /// 3D Model of measured response
        /// </summary>
        public Model3DGroup responseModel { get; set; }
        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Method building 3D model of imported response
        /// </summary>
        public void buildResponseModel()
        {
            responseModel.Children.Clear();
            Point3D center = new Point3D(0, 0, 0);

            //Calculate the amount of energy with specified resolution
            int bins = 360 / measurementResolution;

            double[,] r = new double[bins / 2, bins];

            for (int i = 0; i < measurementData.getLength(); i++)
            {
                Vector3D vector = MyVector3D.toSphericalDeg(measurementData.getDirectionAtIdx(i));
                r[(int)(vector.X / measurementResolution), (int)(vector.Y / measurementResolution)] += measurementData.getAmplitudeAtIdx(i) * measurementData.getAmplitudeAtIdx(i);
            }

            //Add cones to model to create a view of spatial impulse response
            for (int i = 0; i < bins / 2; i++)
                for (int j = 0; j < bins; j++)
                {
                    {
                        TruncatedConeVisual3D cone = new TruncatedConeVisual3D();
                        cone.Origin = center;
                        cone.Height = r[i, j];
                        cone.Normal = MyVector3D.toCartesianDeg(i * measurementResolution, j * measurementResolution, r[i, j]);
                        cone.BaseRadius = 0;
                        cone.TopRadius = (2 * Math.PI * r[i, j] / (2 * bins));
                        cone.BaseCap = false;
                        cone.ThetaDiv = 5;
                        responseModel.Children.Add(cone.Content);
                    }
                }
        }

        /// <summary>
        /// Method importing spatial response from wav file
        /// </summary>
        public void importWaveResult()
        {
            //Get File Path
            string path;
            OpenFileDialog OpenDialog = new OpenFileDialog();
            OpenDialog.Filter = "wav files (*.wav)|*.wav";

            if (OpenDialog.ShowDialog() == true)
            {
                path = OpenDialog.FileName;
                measurementData.importWaveResult(path);
                buildResponseModel();
                setTransforms();
            }
        }

        /// <summary>
        /// Method setting 3D model transforms (position and scale)
        /// </summary>
        public void setTransforms()
        {
            Transform3DGroup newTransform = new Transform3DGroup();

            newTransform.Children.Add(new TranslateTransform3D(measurementPosition));
            newTransform.Children.Add(new ScaleTransform3D(new Vector3D(measurementScale, measurementScale, measurementScale), measurementPosition.ToPoint3D()));
            responseModel.Transform = newTransform;
        }

        #endregion Public Methods
    }
}