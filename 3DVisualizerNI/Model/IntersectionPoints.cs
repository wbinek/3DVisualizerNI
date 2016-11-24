using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace _3DVisualizerNI.Model
{
    public class DataColour
    {
        public Color color { get; set; }
        public double treshold { get; set; }

        public DataColour()
        {
            color = Colors.White;
            treshold = 0;
        }
        public DataColour(Color _color, double _treshold)
        {
            color = _color;
            treshold = _treshold;
        }
    }

    public class IntersectionPoints
    {
        private double _startTime = 0;
        private double _endTime = 0.08;
        public int respLength { get; protected set; }
        public int Fs { get; protected set; }

        public Model3DGroup intersectionModel { get; set; }
        public List<Point3D> intersectionPoints { get; set; }
        public List<Vector3D> faceNormals { get; set; }
        public double[] amplitudes { get; set; }

        public ObservableCollection<DataColour> colorsTimeSet { get; set; }
        public ObservableCollection<DataColour> colorsAmplitudeSet { get; set; }

        public ObservableCollection<DataColour> currentColorSet
        {
            get
            {
                switch (sellectedColorDisplayMode)
                {
                    case "Time":
                        return colorsTimeSet ;
                    case "Amplitude":
                        return colorsAmplitudeSet;
                }
                return null;
            }
            set              
            {
                switch (sellectedColorDisplayMode)
                {
                    case "Time":
                        colorsTimeSet = value ;
                        break;
                    case "Amplitude":
                        colorsAmplitudeSet = value;
                        break;
                }
            }
        }


        public ObservableCollection<String> colorDisplayMode { get; set; }
        public string sellectedColorDisplayMode { get; set; } = "Time";

        public int scale { get; set; } = 2;
        public double startTime
        {
            get { return _startTime; }
            set { if (value < 0)
                {
                    value = 0;
                }
                else if(value>endTime)
                {
                    value = endTime;
                }
                if (value != _startTime)
                {
                    _startTime = value;                  
                }

            }
        }
        public double endTime
        {
            get { return _endTime; }
            set
            {
                if (value > (double)respLength/Fs)
                {
                    value = respLength / Fs;
                }
                else if (value < startTime)
                {
                    value = startTime;
                }
                if (value != _endTime)
                {
                    _endTime = value;                  
                }
            }
        }

        public IntersectionPoints()
        {
            intersectionPoints = new List<Point3D>();
            faceNormals = new List<Vector3D>();
            colorsTimeSet = new ObservableCollection<DataColour>();
            colorsAmplitudeSet = new ObservableCollection<DataColour>();

            initColorDisplayMode();
        }
        private void initColorDisplayMode()
        {
            colorDisplayMode = new ObservableCollection<string>();
            colorDisplayMode.Add("Time");
            colorDisplayMode.Add("Amplitude");
        }

        public void calculateIntersectionPoints(Model3DGroup model, SpatialMeasurement measurement)
        {
            ModelVisual3D testModel = new ModelVisual3D();
            testModel.Content = model;
            intersectionPoints.Clear();

            amplitudes = measurement.getAmplitudeArray();
            respLength = amplitudes.Count();
            Fs = measurement.Fs;

            Point3D origin = measurement.position.ToPoint3D();

            for (int i = 0; i < respLength; i++)
            {
                Vector3D direction = measurement.getDirectionAtIdx(i);
                RayHitTester(testModel, origin, direction);
            }

            buildAmplitudeLegend(measurement);
            buildTimeLegend(measurement);

        }
        public void builidIntersectionModel()
        {
            intersectionModel = new Model3DGroup();

            for (int i = (int)(startTime * Fs); i < endTime * Fs; i++)
            {
                TruncatedConeVisual3D cone = new TruncatedConeVisual3D();
                cone.Height = 0.01;
                cone.Origin = intersectionPoints[i] - faceNormals[i] * cone.Height;
                cone.Normal = -faceNormals[i];
                cone.BaseRadius = amplitudes[i] * Math.Sqrt(scale);
                cone.ThetaDiv = 5;
                cone.BaseCap = false;
                cone.TopCap = false;
                cone.Material = new DiffuseMaterial(new SolidColorBrush(getColor(i,amplitudes[i])));
                cone.BackMaterial = null;
                intersectionModel.Children.Add(cone.Content);
            }
        }
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
        private void buildTimeLegend(SpatialMeasurement measurement)
        {
            double startTime = (double)measurement.getMaxIdx() / measurement.Fs;

            DataColour set0 = new DataColour(Colors.DarkRed, 0);
            DataColour set1 = new DataColour(Colors.Red, startTime - 0.01);
            DataColour set2 = new DataColour(Colors.Orange, startTime + 0.01);
            DataColour set3 = new DataColour(Colors.Yellow, startTime + 0.05);
            DataColour set4 = new DataColour(Colors.GreenYellow, startTime + 0.07);
            DataColour set5 = new DataColour(Colors.Green, startTime + 0.09);

            colorsTimeSet.Add(set0);
            colorsTimeSet.Add(set1);
            colorsTimeSet.Add(set2);
            colorsTimeSet.Add(set3);
            colorsTimeSet.Add(set4);
            colorsTimeSet.Add(set5);

        }
        private void buildAmplitudeLegend(SpatialMeasurement measurement)
        {
            double maxAmplitude = MeasurementUtils.todB(measurement.getMax());

            DataColour set0 = new DataColour(Colors.DarkRed, maxAmplitude-3);
            DataColour set1 = new DataColour(Colors.Red, maxAmplitude - 6);
            DataColour set2 = new DataColour(Colors.Orange, maxAmplitude - 9);
            DataColour set3 = new DataColour(Colors.Yellow, maxAmplitude - 12);
            DataColour set4 = new DataColour(Colors.GreenYellow, maxAmplitude - 15);
            DataColour set5 = new DataColour(Colors.Green, maxAmplitude - 18);

            colorsAmplitudeSet.Add(set0);
            colorsAmplitudeSet.Add(set1);
            colorsAmplitudeSet.Add(set2);
            colorsAmplitudeSet.Add(set3);
            colorsAmplitudeSet.Add(set4);
            colorsAmplitudeSet.Add(set5);
        }

        private Color getColor(int index, double preassure)
        {
            switch (sellectedColorDisplayMode)
            {
                case "Time":
                    double time = (double)index / Fs;
                    foreach (DataColour color in colorsTimeSet.Reverse())
                    {
                        if (time > color.treshold) return color.color;
                    }
                    break;

                case "Amplitude":
                    double amplitude = MeasurementUtils.todB(preassure);
                    foreach (DataColour color in colorsAmplitudeSet)
                    {
                        if (amplitude > color.treshold)
                            return color.color;
                    }
                    break;           
            }
            return Colors.White;
        }
    }
}
