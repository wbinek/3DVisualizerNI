using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace _3DVisualizerNI.Model
{
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
        public int scale { get; set; } = 1;
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
        }

        public void calculateIntersectionPoints(Model3DGroup model, SpatialMeasurement measuremet)
        {
            ModelVisual3D testModel = new ModelVisual3D();
            testModel.Content = model;
            intersectionPoints.Clear();

            amplitudes = measuremet.getAmplitudeArray();
            respLength = amplitudes.Count();
            Fs = measuremet.Fs;

            Point3D origin = measuremet.position.ToPoint3D();

            for (int i = 0; i < respLength; i++)
            {
                Vector3D direction = -measuremet.getDirectionAtIdx(i);
                RayHitTester(testModel, origin, direction);
            }

        }

        public void builidIntersectionModel()
        {
            intersectionModel = new Model3DGroup();

            for (int i = (int)(startTime * Fs); i < endTime * Fs; i++)
            {
                //    SphereVisual3D sphere = new SphereVisual3D();
                //    sphere.Center = intersectionPoints[i];
                //    sphere.Radius = Math.Pow(amplitudes[i] * scale * scale, 2f / 3f);
                //    sphere.ThetaDiv = 4;
                //    sphere.PhiDiv = 2;
                //    sphere.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Red));
                //    intersectionModel.Children.Add(sphere.Content);

                TruncatedConeVisual3D cone = new TruncatedConeVisual3D();
                cone.Height = 0.01;
                cone.Origin = intersectionPoints[i] - faceNormals[i] * cone.Height;
                cone.Normal = -faceNormals[i];
                cone.BaseRadius = Math.Pow(amplitudes[i] * scale * scale, 2f / 3f);
                cone.ThetaDiv = 5;
                cone.BaseCap = false;
                cone.TopCap = false;
                cone.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Yellow));
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
    }
}
