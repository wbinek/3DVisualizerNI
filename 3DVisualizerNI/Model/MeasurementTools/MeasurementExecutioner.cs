using MathNet.Numerics.IntegralTransforms;
using MathNet.Filtering;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using _3DVisualizerNI.Model.FilterTools;
using _3DVisualizerNI.ViewModel;
using _3DVisualizerNI.Views;
using System.IO;
using NAudio.Wave;

namespace _3DVisualizerNI.Model.MeasurementTools
{
    public class MeasurementExecutioner
    {
        private readonly aiaoDriver recorder;
        private readonly CardConfig cardConfig;
        private readonly MeasurementConfig measConfig;
        private readonly object thisLock = new object();
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
            List<double[]> dane = recorder.getDataAsList();
            recorder.Dispose();
            switch (measConfig.measMethod)
            {
                case MeasurementMethods.SweepSine:
                    postprocess(dane);
                    break;
                case MeasurementMethods.Calibrate:
                    caluclateAvaerageLevel(dane);
                    break;
            }
        }

        private void caluclateAvaerageLevel(List<double[]> dane)
        {
            Messenger.Default.Send<double>(Tools.getAverageLevel(dane[0]), "averageLevel");
        }

        public void startMeasurement()
        {
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
                case MeasurementMethods.Calibrate:
                    output = FunctionGenerator.generateByEnum(measConfig.genMethod,
                                            (int)(cardConfig.chSmplRate * measConfig.measLength), cardConfig.chSmplRate,
                                            measConfig.fmin, measConfig.fmax, (int)measConfig.breakLength * cardConfig.chSmplRate, measConfig.averages+1);
                    break;
                case MeasurementMethods.SweepSine:
                    output = FunctionGenerator.generateByEnum(measConfig.genMethod,
                        (int)(cardConfig.chSmplRate * measConfig.measLength), cardConfig.chSmplRate,
                        measConfig.fmin, measConfig.fmax, (int)measConfig.breakLength * cardConfig.chSmplRate, measConfig.averages+1);
                    break;
            }

            var i = 0;
            Array.ForEach(output, x => { output[i++] = x * (double)cardConfig.aoMax; });
        }

        private void postprocess(List<double[]> dane)
        {

            //double[] chW = calculateImpulseResp(dane[0]);
            //double[] chX = calculateImpulseResp(dane[1]);
            //double[] chY = calculateImpulseResp(dane[2]);
            //double[] chZ = calculateImpulseResp(dane[3]);

            double[] chW;
            double[] chX;
            double[] chY;
            double[] chZ;

            var tasks = new List<Task<double[]>>();
            tasks.Add(Task<double[]>.Factory.StartNew(() => calculateImpulseResp(dane[0])));
            tasks.Add(Task<double[]>.Factory.StartNew(() => calculateImpulseResp(dane[1])));
            tasks.Add(Task<double[]>.Factory.StartNew(() => calculateImpulseResp(dane[2])));
            tasks.Add(Task<double[]>.Factory.StartNew(() => calculateImpulseResp(dane[3])));
            Task.WaitAll(tasks.ToArray());
            chW = tasks[0].Result;
            chX = tasks[1].Result;
            chY = tasks[2].Result;
            chZ = tasks[3].Result;

            showAcceptanceWindow(chW, chX, chY, chZ, dane[0], dane[1], dane[2], dane[3]);
        }

        private void showAcceptanceWindow(double[] chW, double[] chX, double[] chY, double[] chZ, double[] ch0 = null, double[] ch1 = null, double[] ch2 = null, double[] ch3 = null)
        {
            int length = (int)(cardConfig.chSmplRate * (measConfig.breakLength/*));//*/ + measConfig.measLength));
            var Fs = cardConfig.chSmplRate;

            var timevector = Tools.getTimeVector(length, Fs);

            AcceptResultsWindow resultsWindow = new AcceptResultsWindow();
            AcceptResultsViewModel vm = new AcceptResultsViewModel();
            resultsWindow.DataContext = vm;
            vm.PlotData(chW, chX, chY, chZ, timevector);
            resultsWindow.ShowDialog();

            if (vm.Accepted)
            {
                var pomiar = new SpatialMeasurement();
                pomiar.measurementData.setData(chW, chX, chY, chZ);
                pomiar.measurementData.Fs = cardConfig.chSmplRate;
                pomiar.measurementName = DateTime.Now.ToString();
                pomiar.buildResponseModel();
                pomiar.setTransforms();

                Messenger.Default.Send<SpatialMeasurement>(pomiar, "AddToList");

                ///To do!!!!
                //Show save window on measurement finished. Temp solution - should be moved somwhere else.
                //Make separate (static) wave save class.
                if (ch0 != null)
                {
                    string path = pomiar.saveWaveResult();
                    string fileName = Path.GetFileNameWithoutExtension(path);
                    path = Path.Combine(Path.GetDirectoryName(path), fileName + "_raw.wav");

                    WaveFormat format = WaveFormat.CreateIeeeFloatWaveFormat(Fs, 4);
                    WaveFileWriter writer = new WaveFileWriter(path, format);

                    double max = ch0.Select(x => Math.Abs(x)).Max();
                    for (int i = 0; i < ch0.Length; i++)
                    {

                        writer.WriteSample((float)(ch0[i]/max));
                        writer.WriteSample((float)(ch1[i]/max));
                        writer.WriteSample((float)(ch2[i]/max));
                        writer.WriteSample((float)(ch3[i]/max));
                    }
                    writer.Close();

                }

            }


        }

        private double[] calculateImpulseResp(double[] inputData)
        {
            // Initialize variables
            int length = (int)(cardConfig.chSmplRate * (measConfig.breakLength/*));//*/ + measConfig.measLength));
            double[] response = new double[length];

            //Calculate impulse repsponses
            if (measConfig.measMethod == MeasurementMethods.SweepSine)
            {
                // Split results into measurements to average
                var reference = output.Split(length).First();
                var measurementList = inputData.Split(length);

                var outputFFT = Tools.double2Complex(reference.ToArray());
                Fourier.Forward(outputFFT, FourierOptions.Matlab);

                //var tasks = new List<Task>();
                //calculate all impulse responses
                foreach (var oneMeasurement in measurementList.Skip(1).Take(measurementList.Count()-2))
                {
                    //tasks.Add(Task.Factory.StartNew(() => mtCalculateResponse(measConfig.averages, oneMeasurement.ToArray(), response, outputFFT, measConfig.processMethod)));
                    mtCalculateResponse(measConfig.averages, oneMeasurement.ToArray(), response, reference.ToArray(), outputFFT, measConfig.processMethod);
                }
                //Task.WaitAll(tasks.ToArray());
            }

            //Return response
            return response;
        }

        private void preprocess()
        {
            if (measConfig.fmax == 0) measConfig.fmax = cardConfig.chSmplRate / 2;
        }

        internal double getLength()
        {
            return measConfig.measLength * measConfig.averages;
        }

        protected void mtCalculateResponse(int averages, double[] input, double[] target, double[] output, Complex[] outputFFT, PostProcessMethods processing)
        {
            /// Use for testing
            //input = output;

            if (processing == PostProcessMethods.FilterInput)
            {
                //Filter input signal
                input = Butterworth.filterResult(measConfig.fmax, measConfig.fmin, input, cardConfig.chSmplRate, 12);
            }

            /// Old impulse response calculation method using circular convolution
            //var resultFFT = Tools.double2Complex(input);
            //Fourier.Forward(resultFFT, FourierOptions.Matlab);

            //var resultFFTDivided = resultFFT.Zip(outputFFT, (x, y) => x / y).ToArray();

            //if (processing == PostProcessMethods.ZeroFDomainValues)
            //{
            //    int startSample = (int)(resultFFTDivided.Length * ((double)measConfig.fmin / cardConfig.chSmplRate));
            //    int endSample = (int)(resultFFTDivided.Length * ((double)measConfig.fmax / cardConfig.chSmplRate));
            //    double average = resultFFTDivided.Skip(startSample).Take(endSample - startSample).Select((x) => x.Magnitude).Average();

            //    Complex[] modified = new Complex[resultFFTDivided.Length];
            //    modified = Array.ConvertAll(modified, x => new Complex(average, 0));

            //    resultFFTDivided.Skip(startSample).Take(endSample - startSample + 1).ToArray().CopyTo(modified, startSample);
            //    resultFFTDivided.Skip(resultFFTDivided.Length - endSample).Take(endSample - startSample + 1).ToArray().CopyTo(modified, resultFFTDivided.Length - endSample);

            //    resultFFTDivided = modified;

            //}
            ////var displayOutput = Array.ConvertAll(outputFFT, item => item.Magnitude);
            ////var displayInput = Array.ConvertAll(resultFFT, item => item.Magnitude);
            ////var displayRatio = Array.ConvertAll(resultFFTDivided, item => item.Magnitude);   

            //Fourier.Inverse(resultFFTDivided, FourierOptions.Matlab);
            //var result = Tools.complexReal2Double(resultFFTDivided);

            // Impulse response calculation using linear convolution
            var time = Tools.getTimeVector((int)(cardConfig.chSmplRate * measConfig.measLength), cardConfig.chSmplRate);
            var maxT = time[time.Count() - 1];
            double L = maxT / Math.Log10(2 * Math.PI * (measConfig.fmax / measConfig.fmin));

            double[] invsweep = output.Take((int)(cardConfig.chSmplRate * measConfig.measLength)).Reverse().Zip(time, (x, t) => (1d / output.Length) * x * Math.Exp(-t / L)).ToArray();
            double[] result = Tools.fastConvolution(input, invsweep);

            /// Alternative linear convolution using filter method - much slower!
            /// This returns signal starting from 0 sample whereas the porper data starts at L-1
            //MathNet.Filtering.FIR.OnlineFirFilter filter = new MathNet.Filtering.FIR.OnlineFirFilter(invsweep);
            //double[] result = filter.ProcessSamples(input);

            //showAcceptanceWindow(displayOutput, displayInput, displayRatio, result);
            lock (thisLock)
            {
                for (var i = 0; i < result.Length; i++)
                    target[i] += result[i] / averages;
            }
        }
    }
}
