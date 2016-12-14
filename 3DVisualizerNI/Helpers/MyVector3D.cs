using System;
using System.Windows.Media.Media3D;

namespace _3DVisualizerNI.Helpers
{
    /// <summary>
    /// Static class containing vector coordinates conversions
    /// </summary>
    public static class MyVector3D
    {
        /// <summary>
        /// Transforms vector from Cartesian to Spherical coordinates
        /// Uses ISO convention
        /// </summary>
        /// <param name="x">x coordinate -> theta</param>
        /// <param name="y">y coordinate -> phi</param>
        /// <param name="z">z coordinate -> r</param>
        /// <returns>Vector in Spherical coordinates (theta, phi, r) in radians</returns>
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

        /// <summary>
        /// Transforms vector from Cartesian to Spherical coordinates
        /// Uses ISO convention
        /// </summary>
        /// <param name="cartesianVector">3D vector in Cartesian coordinates</param>
        /// <returns>Vector in Spherical coordinates (theta, phi, r) in radians</returns>
        public static Vector3D toSpherical(Vector3D cartesianVector)
        {
            Vector3D result = new Vector3D();
            result.Z = Math.Sqrt(cartesianVector.X * cartesianVector.X + cartesianVector.Y * cartesianVector.Y + cartesianVector.Z * cartesianVector.Z);
            if (result.Z == 0)
            {
                result.X = 0;
                result.Y = 0;
            }
            else
            {
                result.X = Math.Acos(cartesianVector.Z / result.Z);
                result.Y = Math.Atan2(-cartesianVector.Y, -cartesianVector.X) + Math.PI;
            }
            return result;
        }

        /// <summary>
        /// Transforms vector from Cartesian to Spherical coordinates and returns angles in degrees
        /// Uses ISO convention
        /// </summary>
        /// <param name="x">x coordinate -> theta</param>
        /// <param name="y">y coordinate -> phi</param>
        /// <param name="z">z coordinate -> r</param>
        /// <returns>Vector in Spherical coordinates (theta, phi, r) in degrees</returns>
        public static Vector3D toSphericalDeg(double x, double y, double z)
        {
            Vector3D result = toSpherical(x, y, z);
            result.X = result.X / (2 * Math.PI) * 360;
            result.Y = result.Y / (2 * Math.PI) * 360;

            return result;
        }

        /// <summary>
        /// Transforms vector from Cartesian to Spherical coordinates and returns angles in degrees
        /// Uses ISO convention
        /// </summary>
        /// <param name="cartesianVector">3D vector in Cartesian coordinates</param>
        /// <returns>Vector in Spherical coordinates (theta, phi, r) in degrees</returns>
        public static Vector3D toSphericalDeg(Vector3D cartesianVector)
        {
            Vector3D result = toSpherical(cartesianVector);
            result.X = result.X / (2 * Math.PI) * 360;
            result.Y = result.Y / (2 * Math.PI) * 360;

            return result;
        }

        /// <summary>
        /// Transforms Spherical coordinates to Cartesian
        /// </summary>
        /// <param name="theta">theta angle [rad]</param>
        /// <param name="phi">phi angle [rad]</param>
        /// <param name="r">vector length [rad]</param>
        /// <returns>Vector in Cartesian coordinates (x, y, z)</returns>
        public static Vector3D toCartesian(double theta, double phi, double r)
        {
            Vector3D result = new Vector3D();
            result.X = r * Math.Sin(theta) * Math.Cos(phi);
            result.Y = r * Math.Sin(theta) * Math.Sin(phi);
            result.Z = r * Math.Cos(theta);
            return result;
        }

        /// <summary>
        /// Transforms Spherical coordinates to Cartesian.
        /// Takes input in degrees
        /// </summary>
        /// <param name="theta">theta angle [deg]</param>
        /// <param name="phi">phi angle [deg]</param>
        /// <param name="r">vector length [deg]</param>
        /// <returns>Vector in Cartesian coordinates (x, y, z)</returns>
        public static Vector3D toCartesianDeg(double theta, double phi, double r)
        {
            return MyVector3D.toCartesian(theta / 360 * (2 * Math.PI), phi / 360 * (2 * Math.PI), r);
        }
    }
}