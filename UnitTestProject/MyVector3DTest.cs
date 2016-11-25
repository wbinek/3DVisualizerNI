using _3DVisualizerNI.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Media.Media3D;

namespace UnitTestProject
{
    [TestClass]
    public class MyVector3DTest
    {
        [TestMethod]
        public void TestToSpherical()
        {
            Vector3D result = new Vector3D(0.9545, 5.4946, 1.7321);
            Vector3D testVector = MyVector3D.toSpherical(1, -1, 1);

            Assert.AreEqual(result.X, testVector.X, 0.01, "Not equal");
            Assert.AreEqual(result.Y, testVector.Y, 0.01, "Not equal");
            Assert.AreEqual(result.Z, testVector.Z, 0.01, "Not equal");
        }

        [TestMethod]
        public void TestToCartesian()
        {
            Vector3D result = new Vector3D(1, -1, 1);
            Vector3D testVector = MyVector3D.toCartesian(0.9545, 5.4946, 1.7321);

            Assert.AreEqual(result.X, testVector.X, 0.01, "Not equal");
            Assert.AreEqual(result.Y, testVector.Y, 0.01, "Not equal");
            Assert.AreEqual(result.Z, testVector.Z, 0.01, "Not equal");
        }

        [TestMethod]
        public void TestToSphericalDeg()
        {
            Vector3D result = new Vector3D(54.8, 315, 1.7321);
            Vector3D testVector = MyVector3D.toSphericalDeg(1, -1, 1);

            Assert.AreEqual(result.X, testVector.X, 0.1, "Not equal");
            Assert.AreEqual(result.Y, testVector.Y, 0.1, "Not equal");
            Assert.AreEqual(result.Z, testVector.Z, 0.1, "Not equal");
        }
    }
}