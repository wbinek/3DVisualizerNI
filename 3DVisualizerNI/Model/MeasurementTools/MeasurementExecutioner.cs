using MathNet.Numerics.IntegralTransforms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using _3DVisualizerNI.ViewModel;
using _3DVisualizerNI.Views;

namespace _3DVisualizerNI.Model.MeasurementTools
{
    public class MeasurementExecutioner
    {
        //private DataTable dane = new DataTable();
        private List<double[]> dane = new List<double[]>();
        private readonly aiaoDriver recorder;
        private readonly CardConfig cardConfig;
        private readonly MeasurementConfig measConfig;
        private readonly object thisLock = new object();
        private int count;
        private double[] output = new double[0];

        public MeasurementExecutioner(CardConfig _cardConfig, MeasurementConfig _measConfig)
        {
            recorder = new aiaoDriver();
            cardConfig = _cardConfig;
            measConfig = _measConfig;
            recorder.MeasurementFinished += OnMeasurementFinished;
        }

        private void OnMeasurementFinished(object sender, EventArgs args)
        {
            dane = recorder.getDataAsList();
            postprocess();
            count++;
        }

        public void startMeasurement()
        {
            count = 1;
            preprocess();
            generateOutput();
            recorder.startMeasurement(output, output.Length, cardConfig);
        }

        public void stopMeasurement()
        {
            recorder.stopMeasurement(true);
        }

        public void generateOutput()
        {
            switch (measConfig.measMethod)
            {
                //case MeasurementMethods.Rejestracja_impulsu:
                //    output =
                //        FunctionGenerator.generateZeros((int)(cardConfig.chSmplRate * measConfig.measLength));
                //    break;
                case MeasurementMethods.SweepSine:
                    output = FunctionGenerator.generateByEnum(measConfig.genMethod,
                        (int)(cardConfig.chSmplRate * measConfig.measLength), cardConfig.chSmplRate,
                        measConfig.fmin, measConfig.fmax, (int)measConfig.breakLength * cardConfig.chSmplRate, measConfig.averages);
                    break;
            }

            var i = 0;
            Array.ForEach(output, x => { output[i++] = x * (double)cardConfig.aoMax; });
        }

        private void postprocess()
        {
            var wynik = calculateImpulseResp();
            showAcceptanceWindow(wynik);
        }

        private void showAcceptanceWindow(List<double[]> wynik)
        {
            int length = (int)(cardConfig.chSmplRate * (measConfig.breakLength + measConfig.measLength));
            var Fs = cardConfig.chSmplRate;

            var timevector = Tools.getTimeVector(length, Fs);

            SpatialMeasurement sm= new SpatialMeasurement();

            AcceptResultsWindow resultsWindow = new AcceptResultsWindow();
            AcceptResultsViewModel vm = new AcceptResultsViewModel();
            resultsWindow.DataContext = vm;
            vm.PlotW(wynik[0], timevector);
            vm.PlotX(wynik[1], timevector);
            vm.PlotY(wynik[2], timevector);
            vm.PlotZ(wynik[3], timevector);
            resultsWindow.ShowDialog();

            if (vm.Accepted)
            {
                var pomiar = new SpatialMeasurement();
                pomiar.measurementData.setData(wynik[0], wynik[1], wynik[2], wynik[3]);
                pomiar.measurementData.Fs = cardConfig.chSmplRate;
                pomiar.measurementName = DateTime.Now.ToString();
                pomiar.buildResponseModel();
                pomiar.setTransforms();

                Messenger.Default.Send<SpatialMeasurement>(pomiar, "AddToList");
            }
        }

        private List<double[]> calculateImpulseResp()
        {
            // Initialize variables
            int length = (int)(cardConfig.chSmplRate * (measConfig.breakLength + measConfig.measLength));
            double[] chW = new double[length];
            double[] chX = new double[length];
            double[] chY = new double[length];
            double[] chZ = new double[length];

            // Split results into measurements to average
            var reference = output.Split(length).First();
            var ListW = dane[0].Split(length);
            var ListX = dane[1].Split(length);
            var ListY = dane[2].Split(length);
            var ListZ = dane[3].Split(length);

            //Calculate impulse repsponses
            if (measConfig.measMethod == MeasurementMethods.SweepSine)
            {
                var outputFFT = Tools.double2Complex(reference.ToArray());
                Fourier.Forward(outputFFT, FourierOptions.Matlab);

                var tasks = new List<Task>();
                //calculate all impulse responses
                foreach (var channelList in (new List<Tuple<IEnumerable<IEnumerable<double>>,double[]>> { new Tuple<IEnumerable<IEnumerable<double>>, double[]>(ListW,chW),
                                                                                            new Tuple<IEnumerable<IEnumerable<double>>, double[]>(ListX,chX),
                                                                                            new Tuple<IEnumerable<IEnumerable<double>>, double[]>(ListY,chY),
                                                                                            new Tuple<IEnumerable<IEnumerable<double>>, double[]>(ListZ,chZ)}))
                    foreach (var oneMeasurement in channelList.Item1)
                    {
                        tasks.Add(Task.Factory.StartNew(() => mtCalculateResponse(measConfig.averages, oneMeasurement.ToArray(), channelList.Item2, outputFFT)));
                    }
                Task.WaitAll(tasks.ToArray());
            }

            //Add responses to final result
            List<double[]> impulseResponses = new List<double[]>();
            impulseResponses.Add(chW);
            impulseResponses.Add(chX);
            impulseResponses.Add(chY);
            impulseResponses.Add(chZ);
            return impulseResponses;
        }

        private void preprocess()
        {
            dane.Clear();
            if (measConfig.fmax == 0) measConfig.fmax = cardConfig.chSmplRate / 2;
        }

        internal double getLength()
        {
            return measConfig.measLength * measConfig.averages;
        }

        private void mtCalculateResponse(int averages, double[] result, double[] target, Complex[] outputFFT)
        {

            var resultFFT = Tools.double2Complex(result);
            Fourier.Forward(resultFFT, FourierOptions.Matlab);

            resultFFT = resultFFT.Zip(outputFFT, (x, y) => x / y).ToArray();

            Fourier.Inverse(resultFFT, FourierOptions.Matlab);
            result = Tools.complexReal2Double(resultFFT);

            lock (thisLock)
            {
                for (var i = 0; i < result.Length; i++)
                    target[i] += result[i] / averages;
            }
        }
    }
}
