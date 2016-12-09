using _3DVisualizerNI.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
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
        public double[] amplitudes { get; set; }
        public double[] filteredAmplitudes { get; set; }

        public int lag { get; set; } = 50;
        public double threshold { get; set; } = 1.5;
        public double influence { get; set; } = 0.5;
        public double minLevel { get; set; } = 20;


        public int Fs { get; set; }
        public PlotModel MyModel { get; private set; }
        public RelayCommand PeakDetectionCommand { get; private set; }

        public PeakFindViewModel()
        {
            this.PeakDetectionCommand = new RelayCommand(PeakDetection);
        }       

        public void InitPlotModel()
        {
            MyModel = new PlotModel();

            LineSeries data = new LineSeries();
            for(int i=0;i<amplitudes.Length;i++)
            {
                data.Points.Add(new DataPoint((double)i/Fs, amplitudes[i]));
            }
            MyModel.Series.Add(data);
            RaisePropertyChanged("MyModel");
        }

        private void PeakDetection()
        {
            filteredAmplitudes = PeakFinder.FindPeaksZScore(amplitudes, lag, threshold, influence, minLevel);
            updateDisplay();
        }

        private void updateDisplay()
        {
            MyModel.Series.Clear();

            LineSeries amp = new LineSeries();
            amp.Color = OxyColor.FromRgb(0, 128, 0);
            for (int i = 0; i < amplitudes.Length; i++)
            {
                amp.Points.Add(new DataPoint((double)i / Fs, amplitudes[i]));
            }
            MyModel.Series.Add(amp);

            if (filteredAmplitudes != null)
            {
                LineSeries peaks = new LineSeries();
                peaks.Color = OxyColor.FromRgb(0, 0, 128);
                for (int i = 0; i < filteredAmplitudes.Length; i++)
                {
                    peaks.Points.Add(new DataPoint((double)i / Fs, filteredAmplitudes[i]));
                }
                MyModel.Series.Add(peaks);
            }
            RaisePropertyChanged("MyModel");
        }
    }
}
