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

        public void PlotW(double[] data, double[] time)
        {
            LineSeries amplitudeSeries = new LineSeries();
            amplitudeSeries.Color = OxyColor.FromRgb(0, 0, 200);
            amplitudeSeries.StrokeThickness = 1;
            amplitudeSeries.MinimumSegmentLength = 10;
            amplitudeSeries.Title = "channel W";

            for (int i = 0; i < data.Length; i++)
            {
                amplitudeSeries.Points.Add(new DataPoint(time[i], data[i]));
            }
            plotW.Series.Add(amplitudeSeries);

            LinearAxis xAxis = new LinearAxis();
            xAxis.Position = AxisPosition.Bottom;
            xAxis.MajorGridlineStyle = LineStyle.Solid;
            xAxis.MinorGridlineStyle = LineStyle.Dash;
            LinearAxis yAxis = new LinearAxis();
            yAxis.Position = AxisPosition.Left;
            yAxis.MajorGridlineStyle = LineStyle.Solid;
            yAxis.MinorGridlineStyle = LineStyle.Dash;

            plotW.Axes.Add(xAxis);
            plotW.Axes.Add(yAxis);

            plotW.InvalidatePlot(true);
        }

        public void PlotX(double[] data, double[] time)
        {
            LineSeries amplitudeSeries = new LineSeries();
            amplitudeSeries.Color = OxyColor.FromRgb(0, 0, 200);
            amplitudeSeries.StrokeThickness = 1;
            amplitudeSeries.MinimumSegmentLength = 10;
            amplitudeSeries.Title = "channel X";

            for (int i = 0; i < data.Length; i++)
            {
                amplitudeSeries.Points.Add(new DataPoint(time[i], data[i]));
            }
            plotX.Series.Add(amplitudeSeries);

            LinearAxis xAxis = new LinearAxis();
            xAxis.Position = AxisPosition.Bottom;
            xAxis.MajorGridlineStyle = LineStyle.Solid;
            xAxis.MinorGridlineStyle = LineStyle.Dash;
            LinearAxis yAxis = new LinearAxis();
            yAxis.Position = AxisPosition.Left;
            yAxis.MajorGridlineStyle = LineStyle.Solid;
            yAxis.MinorGridlineStyle = LineStyle.Dash;

            plotX.Axes.Add(xAxis);
            plotX.Axes.Add(yAxis);

            plotX.InvalidatePlot(true);
        }

        public void PlotY(double[] data, double[] time)
        {
            LineSeries amplitudeSeries = new LineSeries();
            amplitudeSeries.Color = OxyColor.FromRgb(0, 0, 200);
            amplitudeSeries.StrokeThickness = 1;
            amplitudeSeries.MinimumSegmentLength = 10;
            amplitudeSeries.Title = "channel Y";

            for (int i = 0; i < data.Length; i++)
            {
                amplitudeSeries.Points.Add(new DataPoint(time[i], data[i]));
            }
            plotY.Series.Add(amplitudeSeries);

            LinearAxis xAxis = new LinearAxis();
            xAxis.Position = AxisPosition.Bottom;
            xAxis.MajorGridlineStyle = LineStyle.Solid;
            xAxis.MinorGridlineStyle = LineStyle.Dash;
            LinearAxis yAxis = new LinearAxis();
            yAxis.Position = AxisPosition.Left;
            yAxis.MajorGridlineStyle = LineStyle.Solid;
            yAxis.MinorGridlineStyle = LineStyle.Dash;

            plotY.Axes.Add(xAxis);
            plotY.Axes.Add(yAxis);

            plotY.InvalidatePlot(true);
        }

        public void PlotZ(double[] data, double[] time)
        {
            LineSeries amplitudeSeries = new LineSeries();
            amplitudeSeries.Color = OxyColor.FromRgb(0, 0, 200);
            amplitudeSeries.StrokeThickness = 1;
            amplitudeSeries.MinimumSegmentLength = 10;
            amplitudeSeries.Title = "channel Z";

            for (int i = 0; i < data.Length; i++)
            {
                amplitudeSeries.Points.Add(new DataPoint(time[i], data[i]));
            }
            plotZ.Series.Add(amplitudeSeries);

            LinearAxis xAxis = new LinearAxis();
            xAxis.Position = AxisPosition.Bottom;
            xAxis.MajorGridlineStyle = LineStyle.Solid;
            xAxis.MinorGridlineStyle = LineStyle.Dash;
            LinearAxis yAxis = new LinearAxis();
            yAxis.Position = AxisPosition.Left;
            yAxis.MajorGridlineStyle = LineStyle.Solid;
            yAxis.MinorGridlineStyle = LineStyle.Dash;

            plotZ.Axes.Add(xAxis);
            plotZ.Axes.Add(yAxis);

            plotZ.InvalidatePlot(true);
        }
    }
}
