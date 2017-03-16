using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NationalInstruments.DAQmx;
using _3DVisualizerNI.Model.MeasurementTools;

namespace _3DVisualizerNI.ViewModel
{
    class MakeMeasurementViewModel : ViewModelBase
    {
        public CardConfig cardConfig;
        public MeasurementConfig measurementConfig;

        public RelayCommand StartMeasurementCommand { get; private set; }

        public List<ChannelConfig> AvalibleChannels
        {
            get { return cardConfig.chConfig; }
        }

        public List<int> SamplingList
        {
            get { return cardConfig.AvalibleSampling; }
        }
        public int SamplingSelected
        {
            get { return cardConfig.chSmplRate; }
            set { cardConfig.chSmplRate = value; }
        }

        public List<int> SampleToReadList
        {
            get { return cardConfig.AvalibleSampleToRead; }
        }
        public int SampleToReadSelected
        {
            get { return cardConfig.chSmplToRead; }
            set { cardConfig.chSmplToRead = value; }
        }

        public double maxPreassure
        {
            get { return cardConfig.chMaxPress; }
            set { cardConfig.chMaxPress = value; }
        }

        public Array AvalibleAITerminalConfig
        {
            get { return cardConfig.AvalibleAITerminalConfiguration; }
        }
        public AITerminalConfiguration AITerminalConfigSelected
        {
            get { return cardConfig.terminalConfig; }
            set { cardConfig.terminalConfig = value; }
        }

        public Array AvalibleIEPESource
        {
            get { return cardConfig.AvalibleIEPESource; }
        }
        public AIExcitationSource IEPESourceSelected
        {
            get { return cardConfig.excitationSource; }
            set{ cardConfig.excitationSource = value; }
        }

        public double IEPECurrent
        {
            get { return cardConfig.chIEPEVal; }
            set { cardConfig.chIEPEVal = value; }
        }

        public List<string> AvalibleOutputChannels
        {
            get { return cardConfig.aoChannelAvalible; }
        }
        public string OutputChannelSelected
        {
            get { return cardConfig.aoChannelSelected; }
            set { cardConfig.aoChannelSelected = value; }
        }

        public decimal MaximumOutputVal
        {
            get { return cardConfig.aoMax; }
            set { cardConfig.aoMax = value; }
        }

        public decimal MinimumOutputVal
        {
            get { return cardConfig.aoMin; }
            set { cardConfig.aoMin = value; }
        }

        public Array AvalibleMeasurementMethod
        {
            get { return measurementConfig.AvaliblemMeasurementMethod; }
        }
        public MeasurementMethods MeasurementMethodSelected
        {
            get { return measurementConfig.measMethod; }
            set { measurementConfig.measMethod = value; }
        }

        public int AveragesNo
        {
            get { return measurementConfig.averages; }
            set { measurementConfig.averages = value; }
        }

        public int Fmin
        {
            get { return measurementConfig.fmin; }
            set { measurementConfig.fmin = value; }
        }

        public int Fmax
        {
            get
            {
                if (measurementConfig.fmax == 0) return cardConfig.chSmplRate / 2;
                return measurementConfig.fmax;
            }
            set
            {
                if (value > cardConfig.chSmplRate / 2) value = cardConfig.chSmplRate / 2;
                measurementConfig.fmax = value;
            }
        }

        public double measLength
        {
            get { return measurementConfig.measLength; }
            set { measurementConfig.measLength = value; }
        }

        public double breakLength
        {
            get { return measurementConfig.breakLength; }
            set { measurementConfig.breakLength = value; }
        }

        public MakeMeasurementViewModel()
        {
            cardConfig = new CardConfig();
            measurementConfig = new MeasurementConfig();
            this.StartMeasurementCommand = new RelayCommand(this.StartMeasurement);
        }

        public double TimerMax{get; set;}
        public double TimerValue { get; set; }
        private Timer timer;
        private int timerInterval=500;

        public void StartMeasurement()
        {
            TimerValue = 0;
            TimerMax = AveragesNo * (measLength + breakLength);
            RaisePropertyChanged("TimerMax");
            RaisePropertyChanged("TimerValue");
            timer =new Timer(timerInterval);
            timer.Elapsed += timer_Elapsed;
            timer.Start();

            MeasurementExecutioner me = new MeasurementExecutioner(cardConfig,measurementConfig);
            me.startMeasurement();

           }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            TimerValue += (double)timerInterval / 1000;
            RaisePropertyChanged("TimerValue");
            if (TimerValue >= TimerMax)
                timer.Stop();
        }
    }
}
