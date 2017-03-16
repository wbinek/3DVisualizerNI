using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using NationalInstruments;
using NationalInstruments.DAQmx;

namespace _3DVisualizerNI.Model.MeasurementTools
{
    internal class aiaoDriver
    {
        private int cycle;
        private DataColumn[] dataColumn;
        private DataTable dataTable;
        private Task inputTask;

        private int sampleToRead;
        private double no_cycles;
        private int no_samples;
        private Task outputTask;
        private Task runningTask;
        private int sampleCount;

        private AsyncCallback soundCallback;
        private AnalogMultiChannelReader soundReader;
        private AnalogSingleChannelWriter writer;

        public aiaoDriver()
        {
            dataTable = new DataTable();
        }

        public event EventHandler MeasurementFinished;

        public void startMeasurement(double[] output, int noSamples, CardConfig cardConfig)
        {
            if (runningTask == null)
            {
                no_samples = noSamples;
                no_cycles = (double)no_samples / cardConfig.chSmplToRead;
                sampleToRead = cardConfig.chSmplToRead;

                if (no_cycles % 1 != 0 && no_cycles != 0)
                    MessageBox.Show("Measurement time and sampling settings doesnt match");
                else
                    try
                    {
                        // Create a new task
                        inputTask = new Task("inputTask");
                        outputTask = new Task("outputTask");

                        // Configure the Terminal Configuration and Excitation Source with enums
                        //AITerminalConfiguration terminal = proj.cardConfig.terminalConfig;
                        //AIExcitationSource exctSource = proj.cardConfig.excitationSource;

                        // Create the channel

                        foreach (var channel in cardConfig.chConfig)
                            if (channel.chActive)
                                inputTask.AIChannels.CreateMicrophoneChannel(channel.chName,
                                    "soundChannel" + channel.chName.Replace('/', '_'),
                                    channel.chSens, cardConfig.chMaxPress,
                                    cardConfig.terminalConfig, cardConfig.excitationSource, cardConfig.chIEPEVal,
                                    AISoundPressureUnits.Pascals);

                        outputTask.AOChannels.CreateVoltageChannel(cardConfig.aoChannelSelected,
                            "",
                            (double)cardConfig.aoMin,
                            (double)cardConfig.aoMax,
                            AOVoltageUnits.Volts);


                        // Configure the timing parameters
                        inputTask.Timing.ConfigureSampleClock("", cardConfig.chSmplRate,
                            SampleClockActiveEdge.Rising, SampleQuantityMode.ContinuousSamples);

                        outputTask.Timing.ConfigureSampleClock("", cardConfig.chSmplRate,
                            SampleClockActiveEdge.Rising, SampleQuantityMode.ContinuousSamples);

                        // Verify the Task
                        inputTask.Control(TaskAction.Verify);
                        outputTask.Control(TaskAction.Verify);

                        // Set up the start trigger
                        var deviceName = inputTask.AIChannels[0].PhysicalName.Split('/')[0];
                        var terminalNameBase = "/" + GetDeviceName(deviceName) + "/";
                        outputTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
                            terminalNameBase + "ai/StartTrigger",
                            DigitalEdgeStartTriggerEdge.Rising);

                        // Initialize the data table
                        InitializeDataTable(inputTask.AIChannels, ref dataTable);

                        //Write data to output

                        writer = new AnalogSingleChannelWriter(outputTask.Stream);
                        writer.WriteMultiSample(false, output);

                        // Start running the task
                        StartTask();

                        outputTask.Start();
                        inputTask.Start();

                        // Create the analog input sound reader
                        soundReader = new AnalogMultiChannelReader(inputTask.Stream);
                        soundCallback = SoundCallback;

                        // Use SynchronizeCallbacks to specify that the object 
                        // marshals callbacks across threads appropriately.
                        soundReader.SynchronizeCallbacks = true;
                        soundReader.BeginReadWaveform(cardConfig.chSmplToRead,
                            soundCallback, inputTask);

                        cycle = 1;
                        sampleCount = 0;
                    }
                    catch (DaqException exception)
                    {
                        // Display Errors
                        MessageBox.Show(exception.Message);
                        StopTask();
                    }
            }
        }

        private void SoundCallback(IAsyncResult ar)
        {
            try
            {
                if (runningTask != null && runningTask == ar.AsyncState)
                {
                    // Read the available data from the channels
                    var data = soundReader.EndReadWaveform(ar);

                    // Plot your data here
                    dataToDataTable(data, ref dataTable);


                    if (cycle != no_cycles)
                    {
                        // Set up a new callback
                        soundReader.BeginMemoryOptimizedReadWaveform(sampleToRead,
                            soundCallback, inputTask, data);
                        cycle++;
                    }
                    else
                    {
                        StopTask();
                    }
                }
            }
            catch (DaqException exception)
            {
                // Display Errors
                MessageBox.Show(exception.Message);
                StopTask();
            }
        }

        private void dataToDataTable(AnalogWaveform<double>[] sourceArray, ref DataTable dataTable)
        {
            // Iterate over channels
            var currentLineIndex = 0;
            foreach (var waveform in sourceArray)
            {
                for (var sample = 0; sample < waveform.Samples.Count; ++sample)
                    dataTable.Rows[sample + sampleCount][currentLineIndex] = waveform.Samples[sample].Value;
                currentLineIndex++;
            }
            sampleCount += sourceArray[0].SampleCount;
        }

        public void InitializeDataTable(AIChannelCollection channelCollection, ref DataTable data)
        {
            var numOfChannels = channelCollection.Count;
            data.Rows.Clear();
            data.Columns.Clear();
            dataColumn = new DataColumn[numOfChannels];
            var numOfRows = no_samples; //dlugosc sygnalu

            for (var currentChannelIndex = 0; currentChannelIndex < numOfChannels; currentChannelIndex++)
            {
                dataColumn[currentChannelIndex] = new DataColumn();
                dataColumn[currentChannelIndex].DataType = typeof(double);
                dataColumn[currentChannelIndex].ColumnName = channelCollection[currentChannelIndex].PhysicalName;
            }

            data.Columns.AddRange(dataColumn);

            for (var currentDataIndex = 0; currentDataIndex < numOfRows; currentDataIndex++)
            {
                var rowArr = new object[numOfChannels];
                data.Rows.Add(rowArr);
            }
        }

        public void stopMeasurement(bool kill = false)
        {
            if (runningTask != null)
            {
                // Dispose of the task
                runningTask = null;
                StopTask(kill);
            }
        }

        private void StartTask()
        {
            runningTask = inputTask;
        }

        private void StopTask(bool kill = false)
        {
            runningTask = null;
            inputTask.Dispose();
            outputTask.Dispose();

            if (MeasurementFinished != null && !kill)
                MeasurementFinished(this, EventArgs.Empty);
        }

        public DataTable getDataTable()
        {
            return dataTable;
        }

        public List<double[]> getDataAsList()
        {
            List<double[]> results = new List<double[]>();
            foreach (DataColumn col in dataTable.Columns)
            {
                results.Add(dataTable.AsEnumerable().Select(r => r.Field<double>(col.ColumnName)).ToArray());
            }
            return results;
        } 

        public static string GetDeviceName(string deviceName)
        {
            var device = DaqSystem.Local.LoadDevice(deviceName);
            if (device.BusType != DeviceBusType.CompactDaq)
                return deviceName;
            return device.CompactDaqChassisDeviceName;
        }
    }
}
