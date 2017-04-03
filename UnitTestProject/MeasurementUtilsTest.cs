using System;
using System.Linq;
using System.Windows.Media.Animation;
using System.Numerics;
using MathNet.Numerics.IntegralTransforms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using _3DVisualizerNI.Model.FilterTools;
using _3DVisualizerNI.Model.MeasurementTools;

namespace UnitTestProject
{
    [TestClass]
    public class MeasurementUtilsTest
    {
        [TestMethod]
        public void TestRepeatSplit()
        {
            double[] signal = FunctionGenerator.generateExpSweep(1000, 44100, 10, 20000);
            double[] repeated = FunctionGenerator.repeatSignal(signal,500,2);
            Assert.AreEqual(repeated.Length, 3000,"Wrong repeated size");

            var measurementList = repeated.Split(1500);
            Assert.AreEqual(measurementList.First().ToArray().Length, 1500, "Wrong size after split");
             
            double[] s1 = measurementList.First().ToArray();
            double[] s2 = measurementList.Last().ToArray();

            double error = s1.Zip(s2, (x, y) => Math.Abs(x - y)).Sum();
            Assert.AreEqual(error, 0, "Wrong data after split");
        }

        [TestMethod]
        public void TestFilter()
        {
            double[] s = new double[44100];
            s[10] = 1;

            double[] filtered = Butterworth.filterResult(1000, 20000, s, 44100, 24);
            double[] fft = new double[filtered.Length + 2];
            filtered.CopyTo(fft,0);

            Fourier.ForwardReal(fft, filtered.Length, FourierOptions.Matlab);

            Assert.AreEqual(0, new Complex(fft[1000], fft[1001]).Magnitude, 0.00001,"Data not filtered");
            Assert.AreEqual(1, new Complex(fft[4000], fft[4001]).Magnitude, 0.0000001, "Data not passed");
        }

        [TestMethod]
        public void TestFilterOnInput()
        {
            double[] s = FunctionGenerator.generateExpSweep(44100, 44100, 1000, 20000);
            double[] filtered = Butterworth.filterResult(1000, 20000, s, 44100, 6);

            double errorTime = s.Zip(filtered, (x, y) => Math.Abs(x - y)).Sum()/s.Length;
            //Assert.AreEqual(0, errorTime, 0.00001, "Filtered data different from source in time domain");


            double[] fftFiltered = new double[filtered.Length + 2];
            filtered.CopyTo(fftFiltered, 0);
            double[] fftS = new double[s.Length + 2];
            s.CopyTo(fftS, 0);

            Fourier.ForwardReal(fftFiltered, filtered.Length, FourierOptions.Matlab);
            Fourier.ForwardReal(fftS, s.Length, FourierOptions.Matlab);

            double errorFreq = fftS.Skip(2000).Take(40001-2000).Zip(fftFiltered.Skip(2000).Take(40001 - 2000), (x, y) => Math.Abs(x - y)).Sum()/s.Length;

            Assert.AreEqual(0, errorFreq, 0.00001, "Filtered data different from source in frequency domain");
        }
    }
}
