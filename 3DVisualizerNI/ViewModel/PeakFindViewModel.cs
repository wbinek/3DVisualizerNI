using _3DVisualizerNI.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DVisualizerNI.ViewModel
{
    public class PeakFindViewModel : ViewModelBase
    {
        private PeakFindData PeakFindResult;

        public double[] amplitudes { get; set; }
        public double[] filteredAmplitudes
        {
            get
            {
                return PeakFindResult.filteredAmplitudes;
            }

            set
            {
                PeakFindResult.filteredAmplitudes = value;
            }
        }
        public double[] avrAmpitude;
        public double[] stdValue;

        public int lag
        {
            get
            {
                return PeakFindResult.lag;
            }

            set
            {
                PeakFindResult.lag = value;
            }
        }
        public double threshold
        {
            get
            {
                return PeakFindResult.threshold;
            }

            set
            {
                PeakFindResult.threshold = value;
            }
        }
        public double influence
        {
            get
            {
                return PeakFindResult.influence;
            }

            set
            {
                PeakFindResult.influence = value;
            }
        }
        public double minLevel
        {
            get
            {
                return PeakFindResult.minLevel;
            }

            set
            {
                PeakFindResult.minLevel = value;
            }
        }



        public int Fs { get; set; }
        public PlotModel MyModel { get; private set; }
        public RelayCommand PeakDetectionCommand { get; private set; }

        public PeakFindViewModel(PeakFindData PeakFindResults)
        {
            PeakFindResult = PeakFindResults;
            this.PeakDetectionCommand = new RelayCommand(PeakDetection);
        }       

        public void InitPlotModel()
        {
            MyModel = new PlotModel();
            RaisePropertyChanged("MyModel");

            updateDisplay();
            
        }

        private void PeakDetection()
        {
            filteredAmplitudes = PeakFinder.FindPeaksZScore(amplitudes, lag, threshold, influence, minLevel, out avrAmpitude, out  stdValue);
            Messenger.Default.Send<PeakFindData>(PeakFindResult);
            updateDisplay();
        }

        private void updateDisplay()
        {
            MyModel.Series.Clear();

            if (stdValue != null)
            {
                AreaSeries stdValueSeries1 = new AreaSeries();
                stdValueSeries1.Color = OxyColor.FromRgb(0, 200, 100);
                stdValueSeries1.Fill = OxyColors.LightBlue;

                for (int i = 0; i < stdValue.Length; i++)
                {
                    stdValueSeries1.Points.Add(new DataPoint((double)i / Fs, avrAmpitude[i] + threshold * stdValue[i]));
                    stdValueSeries1.Points2.Add(new DataPoint((double)i / Fs, avrAmpitude[i] - threshold * stdValue[i]));
                }
                MyModel.Series.Add(stdValueSeries1);
            }

            LineSeries amplitudeSeries = new LineSeries();
            amplitudeSeries.Color = OxyColor.FromRgb(0, 0, 200);
            amplitudeSeries.StrokeThickness = 1;
            amplitudeSeries.MinimumSegmentLength = 10;

            for (int i = 0; i < amplitudes.Length; i++)
            {
                amplitudeSeries.Points.Add(new DataPoint((double)i / Fs, amplitudes[i]));
            }
            MyModel.Series.Add(amplitudeSeries);

            if (filteredAmplitudes != null)
            {
                LineSeries peaksSeries = new LineSeries();
                peaksSeries.Color = OxyColor.FromRgb(0, 150, 0);
                peaksSeries.StrokeThickness = 1;
                peaksSeries.MinimumSegmentLength = 10;

                for (int i = 0; i < filteredAmplitudes.Length; i++)
                {
                    peaksSeries.Points.Add(new DataPoint((double)i / Fs, filteredAmplitudes[i]));
                }
                MyModel.Series.Add(peaksSeries);
            }

            if (avrAmpitude != null)
            {
                LineSeries avrAmplitudeSeries = new LineSeries();
                avrAmplitudeSeries.Color = OxyColor.FromRgb(0, 200, 100);
                avrAmplitudeSeries.StrokeThickness = 1;
                avrAmplitudeSeries.MinimumSegmentLength = 10;

                for (int i = 0; i < avrAmpitude.Length; i++)
                {
                    avrAmplitudeSeries.Points.Add(new DataPoint((double)i / Fs, avrAmpitude[i]));
                }
                MyModel.Series.Add(avrAmplitudeSeries);
            }
          
            MyModel.InvalidatePlot(true);
        }
    }
}
