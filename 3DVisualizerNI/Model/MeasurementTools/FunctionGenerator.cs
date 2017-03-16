using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace _3DVisualizerNI.Model.MeasurementTools
{
    public enum generatorMethods
    {
        SineWave,
        ExponentialSweep
    }

    public static class FunctionGenerator
    {
        private static double Ln(double a)
        {
            return Math.Log(a, Math.E);
        }

        public static double[] generateSin(int length, int Fs, int f)
        {
            var sin = new double[length];
            var time = Tools.getTimeVector(length, Fs);

            for (var i = 0; i < length; i++)
                sin[i] = Math.Sin(2 * Math.PI * f * time[i]);

            return sin;
        }

        public static double[] generateZeros(int length)
        {
            return new double[length];
        }


        public static double[] generateExpSweep(int length, int Fs, int minFreq, int maxFreq)
        {
            var time = Tools.getTimeVector(length, Fs);
            var expSweep = new double[length];

            var maxT = time[length - 1];

            for (var i = 0; i < length; i++)
                expSweep[i] =
                    Math.Sin(2 * Math.PI * minFreq * (maxT / (Ln(maxFreq) - Ln(minFreq))) *
                             (Math.Pow(Math.E, (Ln(maxFreq) - Ln(minFreq)) * (time[i] / maxT)) - 1));

            return expSweep;
        }

        public static double[] repeatSignal(double[] signal, int breakLength, int repetitions)
        {
            int lengthSequence = (signal.Length + breakLength);
            double[] silence = new double[breakLength];
            double[] output = new double[lengthSequence * repetitions];
            for (int i = 0; i < repetitions; i++)
            {
                signal.CopyTo(output,i*lengthSequence);
                silence.CopyTo(output, (i * lengthSequence) + signal.Length);
            }
            return output;
        }

        public static double[] generateByEnum(generatorMethods type, int length, int Fs, int F1, int optionalF2 = 0, int breakLength = 0, int repetitions = 1)
        {
            var signal = new double[length];

            switch (type)
            {
                case generatorMethods.SineWave:
                    signal = generateSin(length, Fs, F1);
                    break;
                case generatorMethods.ExponentialSweep:
                    signal = generateExpSweep(length, Fs, F1, optionalF2);
                    break;
            }
            return repeatSignal(signal, breakLength, repetitions);
        }
    }
}
