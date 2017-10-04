using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Numerics;
using MathNet.Numerics.IntegralTransforms;

namespace _3DVisualizerNI.Model.MeasurementTools
{

    public static class Tools
    {
        public static IEnumerable<IEnumerable<T>> Split<T>(this T[] array, int size)
        {
            for (var i = 0; i < (float)array.Length / size; i++)
            {
                yield return array.Skip(i * size).Take(size);
            }
        }

        public static T[,] SplitToSquare2D<T>(this T[] array, int size)
        {
            var buffer = new T[(int)Math.Ceiling((double)array.Length / size), size];
            for (var i = 0; i < (float)array.Length / size; i++)
            {
                for (var j = 0; j < size; j++)
                {
                    buffer[i, j] = array[i + j];
                }
            }
            return buffer;
        }

        public static List<DataTable> SplitTable(DataTable originalTable, int batchSize)
        {
            List<DataTable> tables = new List<DataTable>();
            int i = 0;
            int j = 1;
            DataTable newDt = originalTable.Clone();
            newDt.TableName = "Table_" + j;
            newDt.Clear();
            foreach (DataRow row in originalTable.Rows)
            {
                DataRow newRow = newDt.NewRow();
                newRow.ItemArray = row.ItemArray;
                newDt.Rows.Add(newRow);
                i++;
                if (i == batchSize)
                {
                    tables.Add(newDt);
                    j++;
                    newDt = originalTable.Clone();
                    newDt.TableName = "Table_" + j;
                    newDt.Clear();
                    i = 0;
                }
            }
            return tables;
        }

        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());

            var attribute
                = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute))
                    as DescriptionAttribute;

            return attribute == null ? value.ToString() : attribute.Description;
        }

        public static double getAverageLevel(double[] w)
        {
            if (w != null)
            {
                double psq = 0;
                double len = w.Length;
                Array.ForEach(w, x => psq += x * x / len);
                return 10 * Math.Log10(psq / (4E-10));
            }
            return 0;
        }

        public static double getMaxLevel(double[] w)
        {
            double maxVal = w.Select(x => Math.Abs(x)).Max();
            return 10 * Math.Log10(maxVal*maxVal / 4e-10);
        }

        public static double getTotalLevel(double[] w, int fs)
        {
            if (w != null)
            {
                double psq = 0;
                double len = w.Length;
                Array.ForEach(w, x => psq += x * x/fs);
                return 10 * Math.Log10(psq / (4E-10));
            }
            return 0;
        }

        public static double[] getTimeVector(int length, int Fs)
        {
            var time = new double[length];

            for (var i = 0; i < length; i++)
                time[i] = i * (1 / (double)Fs);
            return time;
        }

        public static double[] getPowerSpectrum(double[] signal, int Fs)
        {
            var ps = new double[signal.Length];

            var ft = double2Complex(signal);
            Fourier.Forward(ft, FourierOptions.NoScaling);
            var realft = Array.ConvertAll(ft, item => Math.Pow(item.Magnitude / signal.Length, 2));
            return realft;
        }

        public static double[] getSpectrum(double[] signal, int Fs)
        {
            var ps = new double[signal.Length];

            var ft = double2Complex(signal);
            Fourier.Forward(ft, FourierOptions.NoScaling);
            var realft = Array.ConvertAll(ft, item => item.Magnitude / signal.Length);
            return realft;
        }

        public static double[] getSpectrumdB(double[] signal, int Fs)
        {
            var ps = new double[signal.Length];

            var ft = double2Complex(signal);
            Fourier.Forward(ft, FourierOptions.NoScaling);
            var realft = Array.ConvertAll(ft, item => item.Magnitude / signal.Length);
            return Array.ConvertAll(realft, smpl => 20 * Math.Log10(smpl / 2e-5));
        }

        public static double getSignalEnergy(double[] w)
        {
            double psq = 0;
            double len = w.Length;
            Array.ForEach(w, x => psq += x * x);
            return psq;
        }

        public static double[] getFreqVector(int length, int Fs)
        {
            return Fourier.FrequencyScale(length, Fs);
            ;
        }

        public static Complex[] double2Complex(double[] data)
        {
            return Array.ConvertAll(data, item => (Complex)item);
        }

        public static double[] complexReal2Double(Complex[] data)
        {
            return Array.ConvertAll(data, item => item.Real);
        }

        public static double[] getResultdB(double[] wynik_pomiaru)
        {
            return Array.ConvertAll(wynik_pomiaru, smpl => 20 * Math.Log10(Math.Abs(smpl) / 2e-5));
        }

        public static Complex[] MatlabHilbert(double[] xr)
        {
            var x = (from sample in xr select new Complex(sample, 0)).ToArray();
            Fourier.BluesteinForward(x, FourierOptions.Default);
            var h = new double[x.Length];
            var fftLengthIsOdd = (x.Length | 1) == 1;
            if (fftLengthIsOdd)
            {
                h[0] = 1;
                for (var i = 1; i < xr.Length / 2; i++) h[i] = 2;
            }
            else
            {
                h[0] = 1;
                h[xr.Length / 2] = 1;
                for (var i = 1; i < xr.Length / 2; i++) h[i] = 2;
            }
            for (var i = 0; i < x.Length; i++) x[i] *= h[i];
            Fourier.BluesteinInverse(x, FourierOptions.Default);
            return x;
        }

        public static double[] calculateEnvelopeFunction(double[] data)
        {
            var AnaliticSignal = MatlabHilbert(data);
            var envelope = Array.ConvertAll(AnaliticSignal, x => x.Magnitude);
            return envelope;
        }

        public static double[] fastConvolution(double[] data, double[] filter)
        {
            int M = data.Length;
            int L = filter.Length;
            int N = M + L-1;
            //int next = (int) Math.Pow(2, Math.Ceiling(Math.Log(N) / Math.Log(2)));

            double[] paddedData = new double[N];
            double[] paddedfilter = new double[N];

            Array.Copy(data, paddedData, data.Length);
            Array.Copy(filter, paddedfilter, filter.Length);

            var H = Tools.double2Complex(paddedfilter);
            var X = Tools.double2Complex(paddedData);

            Fourier.Forward(H, FourierOptions.Matlab);
            Fourier.Forward(X, FourierOptions.Matlab);

            var ytc = X.Zip(H, (xs, ys) => xs * ys).ToArray();
            Fourier.Inverse(ytc, FourierOptions.Matlab);
            double[] yt = Tools.complexReal2Double(ytc);


            //return yt.Take(M).ToArray();
            return yt.Skip(L-1).ToArray();
        }

        /// <summary>
        ///     Fits a line to a collection of (x,y) points.
        /// </summary>
        /// <param name="xVals">The x-axis values.</param>
        /// <param name="yVals">The y-axis values.</param>
        /// <param name="inclusiveStart">The inclusive inclusiveStart index.</param>
        /// <param name="exclusiveEnd">The exclusive exclusiveEnd index.</param>
        /// <param name="rsquared">The r^2 value of the line.</param>
        /// <param name="yintercept">The y-intercept value of the line (i.e. y = ax + b, yintercept is b).</param>
        /// <param name="slope">The slop of the line (i.e. y = ax + b, slope is a).</param>
        public static void LinearRegression(double[] xVals, double[] yVals,
            int inclusiveStart, int exclusiveEnd,
            out double rsquared, out double yintercept,
            out double slope)
        {
            //Debug.Assert(xVals.Length == yVals.Length);
            double sumOfX = 0;
            double sumOfY = 0;
            double sumOfXSq = 0;
            double sumOfYSq = 0;
            double ssX = 0;
            double ssY = 0;
            double sumCodeviates = 0;
            double sCo = 0;
            double count = exclusiveEnd - inclusiveStart;

            for (var ctr = inclusiveStart; ctr < exclusiveEnd; ctr++)
            {
                var x = xVals[ctr];
                var y = yVals[ctr];
                sumCodeviates += x * y;
                sumOfX += x;
                sumOfY += y;
                sumOfXSq += x * x;
                sumOfYSq += y * y;
            }
            ssX = sumOfXSq - sumOfX * sumOfX / count;
            ssY = sumOfYSq - sumOfY * sumOfY / count;
            var RNumerator = count * sumCodeviates - sumOfX * sumOfY;
            var RDenom = (count * sumOfXSq - sumOfX * sumOfX)
                         * (count * sumOfYSq - sumOfY * sumOfY);
            sCo = sumCodeviates - sumOfX * sumOfY / count;

            var meanX = sumOfX / count;
            var meanY = sumOfY / count;
            var dblR = RNumerator / Math.Sqrt(RDenom);
            rsquared = dblR * dblR;
            yintercept = meanY - sCo / ssX * meanX;
            slope = sCo / ssX;
        }
    }
}
