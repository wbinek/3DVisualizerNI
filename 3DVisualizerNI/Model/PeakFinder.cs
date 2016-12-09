using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DVisualizerNI.Model
{
    public static class PeakFinder
    {
        public static double[] FindPeaksZScore(double[] amplitudes, int lag, double threshold, double influence, double minLevel)
        {
            double[] peaks = new double[amplitudes.Length];
            double[] filteredY = new double[amplitudes.Length];
            double[] avgFilter = new double[amplitudes.Length];
            double[] stdFilter = new double[amplitudes.Length];

            Array.Copy(amplitudes, 0, filteredY, 0, lag+lag+1);
            avgFilter[lag] = filteredY.Skip(0).Take(lag + 1 + lag).Average();
            stdFilter[lag] = std(filteredY.Skip(0).Take(lag + 1 + lag).ToArray());

            for(int i = lag + 1; i < amplitudes.Length - lag; i++)
            {
                if(MeasurementUtils.todB(amplitudes[i])>minLevel && Math.Abs(amplitudes[i]-avgFilter[i-1]) > threshold * stdFilter[i - 1])
                {
                    peaks[i] = amplitudes[i];
                }
                filteredY[i + lag] = influence * amplitudes[i + lag] + (1 - influence) * filteredY[i + lag - 1];
                avgFilter[i] = filteredY.Skip(i-lag-1).Take(lag + 1 + lag).Average();
                stdFilter[i] = std(filteredY.Skip(i-lag-1).Take(lag + 1 + lag).ToArray());
            }
            return peaks;
        }

        private static double std(double[] data)
        {
            double average = data.Average();
            double sumOfSquaresOfDifferences = data.Select(val => (val - average) * (val - average)).Sum();
            return Math.Sqrt(sumOfSquaresOfDifferences / data.Length);
        }
    }
}
