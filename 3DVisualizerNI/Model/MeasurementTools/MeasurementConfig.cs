using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DVisualizerNI.Model.MeasurementTools
{
    public enum MeasurementMethods
    {
        SweepSine,
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

        public double measLength { get; set; }
        public double breakLength { get; set; }
        public MeasurementMethods measMethod { get; set; }
        public generatorMethods genMethod { get; set; }
        public int fmin { get; set; }
        public int fmax { get; set; }
        public int averages { get; set; }
        
        public void setDefault()
        {
            measLength = 5;
            breakLength = 1;
            measMethod = MeasurementMethods.SweepSine;
            genMethod = generatorMethods.ExponentialSweep;
            fmin = 20;
            fmax = 0;
            averages = 5;
        }
    }
}
