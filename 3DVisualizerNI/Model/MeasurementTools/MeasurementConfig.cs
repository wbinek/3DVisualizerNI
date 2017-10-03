using System;

namespace _3DVisualizerNI.Model.MeasurementTools
{
    public enum MeasurementMethods
    {
        SweepSine,
        Calibrate,
    }

    public enum PostProcessMethods
    {
        NoProcessing,
        FilterInput,
        ZeroFDomainValues,
    }

    [Serializable]
    public class MeasurementConfig
    {
        public MeasurementConfig()
        {
            setDefault();
        }

        public Array AvaliblemMeasurementMethod
        {
            get { return Enum.GetValues(typeof(MeasurementMethods)); }
        }

        public Array AvaliblePostProcessMethod
        {
            get { return Enum.GetValues(typeof(PostProcessMethods)); }
        }

        public double measLength { get; set; }
        public double breakLength { get; set; }
        public MeasurementMethods measMethod { get; set; }
        public generatorMethods genMethod { get; set; }
        public PostProcessMethods processMethod { get; set; }
        public int fmin { get; set; }
        public int fmax { get; set; }
        public int averages { get; set; }
        
        public void setDefault()
        {
            measLength = 5;
            breakLength = 1;
            measMethod = MeasurementMethods.SweepSine;
            genMethod = generatorMethods.ExponentialSweep;
            processMethod = PostProcessMethods.NoProcessing;
            fmin = 20;
            fmax = 0;
            averages = 5;
        }
    }
}
