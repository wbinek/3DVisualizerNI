using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace _3DVisualizerNI.Model
{
    public static class MyVector3D
    {
        public static Vector3D toSpherical(double x, double y, double z)
        {
            Vector3D result = new Vector3D();
            result.Z = Math.Sqrt(x * x + y * y + z * z);
            if (result.Z == 0)
            {
                result.X = 0;
                result.Y = 0;
            }
            else
            {
                result.X = Math.Acos(z / result.Z);
                result.Y = Math.Atan2(-y, -x) + Math.PI;
            }
            return result;
        }

        public static Vector3D toSphericalDeg(double x, double y, double z)
        {
            Vector3D result = toSpherical(x, y, z);
            result.X = result.X / (2 * Math.PI) * 360;
            result.Y = result.Y / (2 * Math.PI) * 360;

            return result;
        }

        public static Vector3D toCartesian(double theta, double phi, double r)
        {
            Vector3D result = new Vector3D();
            result.X = r * Math.Sin(theta) * Math.Cos(phi);
            result.Y = r * Math.Sin(theta) * Math.Sin(phi);
            result.Z = r * Math.Cos(theta);
            return result;
        }

        public static Vector3D toCartesianDeg(double theta, double phi, double r)
        {
            return MyVector3D.toCartesian(theta / 360 * (2 * Math.PI), phi / 360 * (2 * Math.PI), r);
        }


    }
}
