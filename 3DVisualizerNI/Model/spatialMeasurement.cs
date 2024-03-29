﻿using HelixToolkit.Wpf;
using System;
using System.IO;
using System.Linq;
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

        public static double deg2rad(double deg)
        {
            return Math.PI * deg / 180;
        }

        #endregion Public Methods
    }

    /// <summary>
    /// Class containing basic measurement result data
    /// </summary>
    [Serializable]
    public class MeasurementData
    {
        #region Private Fields

        /// <summary>
        /// Arrays containing measurement result
        /// </summary>
        private double[] w, x, y, z;

        private int _fs;

        #endregion Private Fields

        #region Public Properties

        public void setData(double[] W, double[] X, double[] Y, double[] Z)
        {
            w = W;
            x = X;
            y = Y;
            z = Z;
        }

        /// <summary>
        /// Measurement sampling frequency
        /// </summary>
        public int Fs { get { return _fs; } set { _fs = value; } }

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

        public double[] getX()
        {
            return x;
        }

        public double[] getY()
        {
            return y;
        }

        public double[] getZ()
        {
            return z;
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

        public double getTotalLevel()
        {
            if (w != null)
            {
                double psq = 0;
                Array.ForEach(w, x => psq += x * x / Fs);
                return 10 * Math.Log10(psq / (4E-10));
            }
            return 0;
        }

        /// <summary>
        /// Imports result from wave at given path
        /// </summary>
        /// <param name="path">Path to wavfile</param>
        public void importWaveResult(string path)
        {
            Utilities.waveSaveRead.readWaveResult(path,ref w,ref x,ref y,ref z,ref _fs);
        }

        public void saveResultAsWave(string path)
        {
            Utilities.waveSaveRead.saveResultAsWave(path, w, x, y, z, Fs);
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
        private double _scale = 0.001;

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
        public double measurementScale
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
        [NonSerialized]
        private Model3DGroup _responseModel;
        public Model3DGroup responseModel { get { return _responseModel; } set { _responseModel = value; } }

        public string measurementName { get; set; }
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
            double stpPhi = MeasurementUtils.deg2rad(measurementResolution);

            for (int i = 0; i < measurementData.getLength(); i++)
            {
                Vector3D vector = MyVector3D.toSphericalDeg(measurementData.getDirectionAtIdx(i));
                r[(int)(vector.X / measurementResolution), (int)(vector.Y / measurementResolution)] += (measurementData.getAmplitudeAtIdx(i) * measurementData.getDirectionAtIdx(i)).Length;
            }
            double max = 0;
            //Add cones to model to create a view of spatial impulse response
            for (int i = 0; i < bins / 2; i++)
            {
                double elementArea = stpPhi *
                        (Math.Cos(MeasurementUtils.deg2rad((i) * measurementResolution)) -
                        Math.Cos(MeasurementUtils.deg2rad((i + 1) * measurementResolution))) / (4 * Math.PI);
                for (int j = 0; j < bins; j++)         
                {
                    TruncatedConeVisual3D cone = new TruncatedConeVisual3D();
                    cone.Origin = center;
                    cone.Height = r[i, j]/elementArea;
                    if (cone.Height > max) max = cone.Height;
                    cone.Normal = MyVector3D.toCartesianDeg(i * measurementResolution, j * measurementResolution, r[i, j]);
                    cone.BaseRadius = 0;
                    cone.TopRadius = 2*Math.Sqrt(elementArea * (cone.Height * cone.Height) / Math.PI);
                    cone.BaseCap = false;
                    cone.ThetaDiv = 5;
                    responseModel.Children.Add(cone.Content);
                }
            }
            measurementScale = 1/max;
        }

        /// <summary>
        /// Method importing spatial response from wav file
        /// </summary>
        public void importWaveResult(string path)
        {
            measurementData.importWaveResult(path);
            measurementName = Path.GetFileName(path);
            onImport();
        }

        public void onImport()
        {
            buildResponseModel();
            setTransforms();
        }

        public void onPorjectLoad()
        {
            responseModel = new Model3DGroup();
            onImport();
        }


        public void saveWaveResult(string path)
        {
            measurementData.saveResultAsWave(path);
        }

        /// <summary>
        /// Method setting 3D model transforms (position and scale)
        /// </summary>
        public void setTransforms()
        {
            Transform3DGroup newTransform = new Transform3DGroup();

            Rotation3D rot = new AxisAngleRotation3D();


            newTransform.Children.Add(new RotateTransform3D());
            newTransform.Children.Add(new TranslateTransform3D(measurementPosition));
            newTransform.Children.Add(new ScaleTransform3D(new Vector3D(measurementScale, measurementScale, measurementScale), measurementPosition.ToPoint3D()));
            responseModel.Transform = newTransform;
        }

        #endregion Public Methods
    }
}