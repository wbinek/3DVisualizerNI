using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DVisualizerNI.Model
{
    public class PeakFindData
    {
        public double[] filteredAmplitudes { get; set; }
        public int lag { get; set; } = 50;
        public double threshold { get; set; } = 2;
        public double influence { get; set; } = 0.1;
        public double minLevel { get; set; } = 35;
    }


    public static class PeakFinder
    {
        public static double[] FindPeaksZScore(double[] amplitudes, int lag, double threshold, double influence, double minLevel, out double[] avgFilter, out double[] stdFilter )
        {
            double[] peaks = new double[amplitudes.Length];
            double[] filteredY = new double[amplitudes.Length];
            avgFilter = new double[amplitudes.Length];
            stdFilter = new double[amplitudes.Length];

            Array.Copy(amplitudes, 0, filteredY, 0, lag+lag+1);
            avgFilter[lag] = avr(filteredY,0,lag + 1 + lag);
            stdFilter[lag] = std(filteredY, avgFilter[lag],0,lag + 1 + lag);

            for(int i = lag + 1; i < amplitudes.Length - lag; i++)
            {
                if(MeasurementUtils.todB(amplitudes[i])>minLevel && Math.Abs(amplitudes[i]-avgFilter[i-1]) > threshold * stdFilter[i - 1])
                {
                    peaks[i] = amplitudes[i];
                }
                filteredY[i + lag] = influence * amplitudes[i + lag] + (1 - influence) * filteredY[i + lag - 1];
                avgFilter[i] = avr(filteredY,i-lag,lag + 1 + lag);
                stdFilter[i] = std(filteredY, avgFilter[i],i-lag,lag + 1 + lag);
            }
            return peaks;
        }

        private static double std(double[] data, double avr, int startIdx, int length)
        {
            double sumOfSquaresOfDifferences = 0;
            for (int i = startIdx; i< startIdx + length; i++) {
                sumOfSquaresOfDifferences += (data[i] - avr) * (data[i] - avr);
            }
            return Math.Sqrt(sumOfSquaresOfDifferences / length);
        }

        private static double avr(double[] data, int startIdx, int length)
        {
            double sum=0;
            for(int i=startIdx; i< startIdx + length; i++)
            {
                sum += data[i];
            }
            return sum / length;
        }
    }
}
