﻿using GalaSoft.MvvmLight.Messaging;
using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace _3DVisualizerNI.Model
{
    /// <summary>
    /// Class representing marker color and threshold for color selection
    /// </summary>
    public class DataColour
    {

        #region Public Constructors

        /// <summary>
        /// Default constructor for DataColour class
        /// </summary>
        public DataColour()
        {
            color = Colors.White;
            threshold = 0;
        }

        /// <summary>
        /// Constructor of DataColor class
        /// </summary>
        /// <param name="_color">marker color</param>
        /// <param name="_threshold">marker threshold</param>
        public DataColour(Color _color, double _threshold)
        {
            color = _color;
            threshold = _threshold;
        }

        #endregion Public Constructors

        #region Public Properties
        
        /// <summary>
        /// Property holding marker color
        /// </summary>
        public Color color { get; set; }

        /// <summary>
        /// Threshold for using this color
        /// </summary>
        public double threshold { get; set; }

        #endregion Public Properties

    }

    public class ResponseBasicProperties
    {
        private double _endTime = 0.08;
        private double _startTime = 0;

        public double[] amplitudeArray { get; set; }

        /// <summary>
        /// Start time for intersections display
        /// </summary>
        public double respStartTime
        {
            get { return _startTime; }
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                else if (value > respEndTime)
                {
                    value = respEndTime;
                }
                if (value != _startTime)
                {
                    _startTime = value;
                }
            }
        }

        /// <summary>
        /// Intersection points display end time.
        /// </summary>
        public double respEndTime
        {
            get { return _endTime; }
            set
            {
                if (value > (double)respLength / Fs)
                {
                    value = respLength / Fs;
                }
                else if (value < respStartTime)
                {
                    value = respStartTime;
                }
                if (value != _endTime)
                {
                    _endTime = value;
                }
            }
        }

        /// <summary>
        /// Length of window for display
        /// </summary>
        public int respLength { get; set; }

        /// <summary>
        /// Current measurement sampling frequency
        /// </summary>
        public int Fs { get; set; }
        
    }

    public class ResponseDisplayProperties
    {

        public ResponseDisplayProperties()
        {
            colorsTimeSet = new ObservableCollection<DataColour>();
            colorsAmplitudeSet = new ObservableCollection<DataColour>();
            initColorDisplayMode();
        }
        /// <summary>
        /// Collection of available coloring modes
        /// </summary>
        public ObservableCollection<String> colorDisplayMode { get; set; }

        /// <summary>
        /// Collection of colors for amplitude based coloring
        /// </summary>
        public ObservableCollection<DataColour> colorsAmplitudeSet { get; set; }

        /// <summary>
        /// Collection of colors for arrival time based coloring
        /// </summary>
        public ObservableCollection<DataColour> colorsTimeSet { get; set; }

        /// <summary>
        /// Currently selected color set
        /// </summary>
        public ObservableCollection<DataColour> currentColorSet
        {
            get
            {
                switch (selectedColorDisplayMode)
                {
                    case "Time [ms]":
                        return colorsTimeSet;

                    case "Amplitude [dB]":
                        return colorsAmplitudeSet;
                }
                return null;
            }
            set
            {
                switch (selectedColorDisplayMode)
                {
                    case "Time [ms]":
                        colorsTimeSet = value;
                        break;

                    case "Amplitude [dB]":
                        colorsAmplitudeSet = value;
                        break;
                }
            }
        }

        /// <summary>
        /// Marker scale
        /// </summary>
        public int markerScale { get; set; } = 2;

        /// <summary>
        /// If true markers will not be scaled according to amplitude
        /// </summary>
        public bool constantMarkerSize { get; set; } = false;

        /// <summary>
        /// Currently selected coloring mode
        /// </summary>
        public string selectedColorDisplayMode { get; set; } = "Time [ms]";

        /// <summary>
        /// If true only detected peaks will be displayed
        /// </summary>
        public bool showPeaksOnly { get; set; } = false;

        /// <summary>
        /// Creates legend for coloring by amplitude
        /// </summary>
        /// <param name="measurement">3D impulse response</param>
        public void buildAmplitudeLegend(SpatialMeasurement measurement)
        {
            double maxAmplitude = MeasurementUtils.todB(measurement.measurementData.getMax());

            DataColour set0 = new DataColour(Colors.DarkRed, maxAmplitude - 3);
            DataColour set1 = new DataColour(Colors.Red, maxAmplitude - 6);
            DataColour set2 = new DataColour(Colors.Orange, maxAmplitude - 9);
            DataColour set3 = new DataColour(Colors.Yellow, maxAmplitude - 12);
            DataColour set4 = new DataColour(Colors.GreenYellow, maxAmplitude - 15);
            DataColour set5 = new DataColour(Colors.Green, maxAmplitude - 18);
            DataColour set6 = new DataColour(Colors.Blue, maxAmplitude - 21);

            colorsAmplitudeSet.Add(set0);
            colorsAmplitudeSet.Add(set1);
            colorsAmplitudeSet.Add(set2);
            colorsAmplitudeSet.Add(set3);
            colorsAmplitudeSet.Add(set4);
            colorsAmplitudeSet.Add(set5);
            colorsAmplitudeSet.Add(set6);
        }

        /// <summary>
        /// Creates legend for coloring by arrival time
        /// </summary>
        /// <param name="measurement">3D impulse response</param>
        public void buildTimeLegend(SpatialMeasurement measurement)
        {
            double startTime = (double)measurement.measurementData.getMaxIdx() / measurement.measurementData.Fs * 1000;

            DataColour set0 = new DataColour(Colors.DarkRed, 0);
            DataColour set1 = new DataColour(Colors.Red, startTime - 5);
            DataColour set2 = new DataColour(Colors.Orange, startTime + 5);
            DataColour set3 = new DataColour(Colors.Yellow, startTime + 45);
            DataColour set4 = new DataColour(Colors.GreenYellow, startTime + 75);
            DataColour set5 = new DataColour(Colors.Green, startTime + 85);

            colorsTimeSet.Add(set0);
            colorsTimeSet.Add(set1);
            colorsTimeSet.Add(set2);
            colorsTimeSet.Add(set3);
            colorsTimeSet.Add(set4);
            colorsTimeSet.Add(set5);
        }

        /// <summary>
        /// Gets color from legend
        /// </summary>
        /// <param name="index">Current sample index</param>
        /// <param name="pressure">Current sample pressure</param>
        /// <returns></returns>
        public Color getColor(int index, double pressure, double Fs)
        {
            switch (selectedColorDisplayMode)
            {
                case "Time [ms]":
                    double time = (double)index / Fs * 1000;
                    foreach (DataColour color in colorsTimeSet.Reverse())
                    {
                        if (time > color.threshold) return color.color;
                    }
                    break;

                case "Amplitude [dB]":
                    double amplitude = MeasurementUtils.todB(pressure);
                    foreach (DataColour color in colorsAmplitudeSet)
                    {
                        if (amplitude > color.threshold)
                            return color.color;
                    }
                    break;
            }
            return Colors.White;
        }

        /// <summary>
        /// Initializer for color display modes.
        /// Add new elements if new modes needed.
        /// </summary>
        private void initColorDisplayMode()
        {
            colorDisplayMode = new ObservableCollection<string>();
            colorDisplayMode.Add("Time [ms]");
            colorDisplayMode.Add("Amplitude [dB]");
        }


    }

    /// <summary>
    /// Class containing information about model-response intersection points
    /// </summary>
    public class IntersectionPoints
    {

        #region Public Constructors
        /// <summary>
        /// Default constructor for IntersectionPoints class
        /// </summary>
        public IntersectionPoints()
        {
            intersectionPoints = new List<Point3D>();
            faceNormals = new List<Vector3D>();
            ResponseProperties = new ResponseBasicProperties();
            DisplayProperties = new ResponseDisplayProperties();
            PeakFindResults = new PeakFindData();

            Messenger.Default.Register<PeakFindData>
            (
                this,
                (_PeakFindResults) => ReceiveFilteredResponse(_PeakFindResults)
            );
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// 3D model with colored intersection points marked using default marker
        /// </summary>
        public Model3DGroup intersectionModel { get; set; }

        /// <summary>
        /// Object containing information about peak find settings and result
        /// </summary>
        public PeakFindData PeakFindResults { get; set; }

        public ResponseDisplayProperties DisplayProperties { get; set; }

        public ResponseBasicProperties ResponseProperties { get; set; }

        #endregion Public Properties

        #region Private Properties

        /// <summary>
        /// List of normals of planes intersecting with rays
        /// Used for setting markers in right direction
        /// </summary>
        private List<Vector3D> faceNormals { get; set; }

        /// <summary>
        /// List of intersection points
        /// </summary>
        private List<Point3D> intersectionPoints { get; set; }

        #endregion Private Properties

        #region Public Methods

        /// <summary>
        /// Builds intersection model
        /// </summary>
        public void builidIntersectionModel()
        {
            intersectionModel = new Model3DGroup();
            double[] drawAmplitudes;
            if (DisplayProperties.showPeaksOnly && PeakFindResults != null) drawAmplitudes = PeakFindResults.filteredAmplitudes; else drawAmplitudes = ResponseProperties.amplitudeArray;

            for (int i = (int)(ResponseProperties.respStartTime * ResponseProperties.Fs); i < ResponseProperties.respEndTime * ResponseProperties.Fs; i++)
            {
                if (drawAmplitudes[i] != 0)
                {
                    TruncatedConeVisual3D cone = new TruncatedConeVisual3D();
                    cone.Height = 0.01;
                    cone.Origin = intersectionPoints[i] - faceNormals[i] * cone.Height;
                    cone.Normal = -faceNormals[i];
                    if (DisplayProperties.constantMarkerSize)
                        cone.BaseRadius = 0.1 * Math.Sqrt(DisplayProperties.markerScale); else cone.BaseRadius = drawAmplitudes[i] * Math.Sqrt(DisplayProperties.markerScale);
                    cone.ThetaDiv = 5;
                    cone.BaseCap = false;
                    cone.TopCap = false;
                    cone.Material = new DiffuseMaterial(new SolidColorBrush(DisplayProperties.getColor(i, drawAmplitudes[i],ResponseProperties.Fs)));
                    cone.BackMaterial = null;
                    intersectionModel.Children.Add(cone.Content);
                }
            }
        }

        /// <summary>
        /// Get intersection points list
        /// </summary>
        private List<List<object>> GetIntersectionPointsList()
        {
            double[] drawAmplitudes;
            if (DisplayProperties.showPeaksOnly && PeakFindResults != null) drawAmplitudes = PeakFindResults.filteredAmplitudes; else drawAmplitudes = ResponseProperties.amplitudeArray;

            int index = 0;
            List<List<object>> intersectionPointsArray = new List<List<object>>();
            for (int i = (int)(ResponseProperties.respStartTime * ResponseProperties.Fs); i < ResponseProperties.respEndTime * ResponseProperties.Fs; i++)
            {
                if (drawAmplitudes[i] != 0)
                {
                    List<object> row = new List<object>();
                    row.Add(index);
                    row.Add(intersectionPoints[i].X);
                    row.Add(intersectionPoints[i].Y);
                    row.Add(intersectionPoints[i].Z);
                    row.Add((double)i / ResponseProperties.Fs);
                    row.Add(drawAmplitudes[i]);
                    intersectionPointsArray.Add(row);
                    index++;
                }
            }
            return intersectionPointsArray;
        }

        /// <summary>
        /// Export intersections as txt file
        /// </summary>
        public void SaveIntersectionPointsAsTxt()
        {
            List<string> headers = new List<string> { "id", "x", "y", "z", "time", "amplitude" };
            List<List<object>> intersectionPointsArray = GetIntersectionPointsList();

            string path = "";
            bool save = Utilities.ArrayToTxtExporter.getSavePath(ref path);         
            if (save)     
                Utilities.ArrayToTxtExporter.SaveListAsTxt(path, intersectionPointsArray, headers);
        }

        /// <summary>
        /// Calculates intersection points using ray tracing
        /// </summary>
        /// <param name="model">3D geometry model</param>
        /// <param name="measurement">3D impulse response</param>
        public void calculateIntersectionPoints(Model3DGroup model, SpatialMeasurement measurement)
        {
            ModelVisual3D testModel = new ModelVisual3D();
            testModel.Content = model;
            intersectionPoints.Clear();

            ResponseProperties.amplitudeArray = measurement.measurementData.getAmplitudeArray();
            ResponseProperties.respLength = ResponseProperties.amplitudeArray.Count();
            ResponseProperties.Fs = measurement.measurementData.Fs;

            Point3D origin = measurement.measurementPosition.ToPoint3D();

            for (int i = 0; i < ResponseProperties.respLength; i++)
            {
                Vector3D direction = measurement.measurementData.getDirectionAtIdx(i);
                RayHitTester(testModel, origin, direction);
            }

            DisplayProperties.buildAmplitudeLegend(measurement);
            DisplayProperties.buildTimeLegend(measurement);
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Simple ray tracer
        /// </summary>
        /// <param name="model">3D model</param>
        /// <param name="origin">Ray start point</param>
        /// <param name="direction">Ray direction</param>
        private void RayHitTester(ModelVisual3D model, Point3D origin, Vector3D direction)
        {
            Viewport3DVisual test = new Viewport3DVisual();
            System.Windows.Media.Media3D.RayHitTestParameters hitParams =
                new System.Windows.Media.Media3D.RayHitTestParameters(
                origin,
                direction
                );
            VisualTreeHelper.HitTest(model, null, ResultCallback, hitParams);
        }

        /// <summary>
        /// Method returning ray tracing collision information
        /// </summary>
        /// <param name="result">Hit test result</param>
        /// <returns></returns>
        private HitTestResultBehavior ResultCallback(HitTestResult result)
        {
            // Did we hit 3D?
            RayHitTestResult rayResult = result as RayHitTestResult;
            if (rayResult != null)
            {
                // Did we hit a MeshGeometry3D?
                RayMeshGeometry3DHitTestResult rayMeshResult =
                    rayResult as RayMeshGeometry3DHitTestResult;

                if (rayMeshResult != null)
                {
                    // Yes we did!
                    intersectionPoints.Add(rayMeshResult.PointHit);

                    Vector3D v1 = rayMeshResult.MeshHit.Positions[rayMeshResult.VertexIndex1].ToVector3D() - rayMeshResult.MeshHit.Positions[rayMeshResult.VertexIndex2].ToVector3D();
                    Vector3D v2 = rayMeshResult.MeshHit.Positions[rayMeshResult.VertexIndex1].ToVector3D() - rayMeshResult.MeshHit.Positions[rayMeshResult.VertexIndex3].ToVector3D();
                    Vector3D normal = Vector3D.CrossProduct(v1, v2);
                    normal.Normalize();
                    faceNormals.Add(normal);

                    return HitTestResultBehavior.Stop;
                }
            }

            return HitTestResultBehavior.Continue;
        }

        private object ReceiveFilteredResponse(PeakFindData _PeakFindResults)
        {
            PeakFindResults = _PeakFindResults;
            builidIntersectionModel();
            return null;
        }

        #endregion Private Methods

    }
}