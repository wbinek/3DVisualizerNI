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
        public Model3DGroup intersectionModel { get; set; }
        public List<Point3D> intersectionPoints { get; set; }
        public double[] amplitudes { get; set; }

        public IntersectionPoints()
        {
            intersectionPoints = new List<Point3D>();
        }

        public void calculateIntersectionPoints(Model3DGroup model, SpatialMeasurement measuremet)
        {
            ModelVisual3D testModel = new ModelVisual3D();
            testModel.Content = model;

            amplitudes = measuremet.getAmplitudeArray();
            Point3D origin = measuremet.position.ToPoint3D();

            for (int i = 0; i < 10000; i++)
            {
                Vector3D direction = measuremet.getDirectionAtIdx(i);
                RayHitTester(testModel, origin, direction);
            }
        }

        public void bilidIntersectionModel()
        {
            intersectionModel = new Model3DGroup();
            for (int i = 0; i < intersectionPoints.Count; i++)
            {
                SphereVisual3D sphere = new SphereVisual3D();
                sphere.Center = intersectionPoints[i];
                sphere.Radius = amplitudes[i] * amplitudes[i] * 5;
                intersectionModel.Children.Add(sphere.Content);
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
                    return HitTestResultBehavior.Stop;
                }
            }

            return HitTestResultBehavior.Continue;
        }
    }
}
