using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using _3DVisualizerNI.Model.MeasurementTools;

namespace _3DVisualizerNI.ViewModel
{
    class AcceptResultsViewModel : ViewModelBase
    {
        public bool Accepted = false;
        public RelayCommand<Window> AcceptResultsCommand { get; private set; }
        public RelayCommand<Window> DeclineResultsCommand { get; private set; }

        public PlotModel plotW { get; private set; }
        public PlotModel plotX { get; private set; }
        public PlotModel plotY { get; private set; }
        public PlotModel plotZ { get; private set; }

        public double totalLevelValue { get; set; }
        public double maxLevelValue { get; set; }
        public double averageLevelValue { get; set; }
        public double noiseLevelValue { get; set; }
        public double snrLevelValue { get; set; }

        public AcceptResultsViewModel()
        {
            this.AcceptResultsCommand = new RelayCommand<Window>(this.AcceptMeasurement);
            this.DeclineResultsCommand = new RelayCommand<Window>(this.DeclineMeasurement);

            plotW = new PlotModel();
            plotX = new PlotModel();
            plotY = new PlotModel();
            plotZ = new PlotModel();
        }

        public void AcceptMeasurement(Window window)
        {
            Accepted = true;
            window.Close();
        }

        public void DeclineMeasurement(Window window)
        {
            window.Close();
        }

        public void plot(double[] data, double[] time, PlotModel plot, string label = "")
        {
            LineSeries amplitudeSeries = new LineSeries();
            amplitudeSeries.Points.Capacity = data.Length;
            amplitudeSeries.Color = OxyColor.FromRgb(0, 0, 200);
            amplitudeSeries.StrokeThickness = 1;
            amplitudeSeries.MinimumSegmentLength = 10;
            amplitudeSeries.Title = label;

            for (int i = 0; i < data.Length; i++)
            {
                amplitudeSeries.Points.Add(new DataPoint(time[i], data[i]));
            }
            plot.Series.Add(amplitudeSeries);

            LinearAxis xAxis = new LinearAxis();
            xAxis.Position = AxisPosition.Bottom;
            xAxis.MajorGridlineStyle = LineStyle.Solid;
            xAxis.MinorGridlineStyle = LineStyle.Dash;
            LinearAxis yAxis = new LinearAxis();
            yAxis.Position = AxisPosition.Left;
            yAxis.MajorGridlineStyle = LineStyle.Solid;
            yAxis.MinorGridlineStyle = LineStyle.Dash;

            plot.Axes.Add(xAxis);
            plot.Axes.Add(yAxis);

            plot.InvalidatePlot(true);
        }

        public void PlotData(double[] dataW, double[] dataX, double[] dataY, double[] dataZ, double[] time)
        {
            plot(dataW, time, plotW, "channel W");
            plot(dataX, time, plotX, "channel X");
            plot(dataY, time, plotY, "channel Y");
            plot(dataZ, time, plotZ, "channel Z");

            calculateStats(dataW, dataX, dataY, dataZ, time);
        }

        private void calculateStats(double[] dataW, double[] dataX, double[] dataY, double[] dataZ, double[] time)
        {
            int fs =(int)(1 / (time[1] - time[0]));
            totalLevelValue = Tools.getTotalLevel(dataW, fs);
            maxLevelValue = Tools.getMaxLevel(dataW);
            averageLevelValue = Tools.getAverageLevel(dataW);
            noiseLevelValue = Tools.getAverageLevel(dataW.Skip(dataW.Length-fs/2).ToArray());
            snrLevelValue = maxLevelValue - noiseLevelValue;
        }
    }
}
