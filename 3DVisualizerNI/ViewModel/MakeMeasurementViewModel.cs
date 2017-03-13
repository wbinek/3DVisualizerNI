using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using NationalInstruments.DAQmx;
using _3DVisualizerNI.Model;

namespace _3DVisualizerNI.ViewModel
{
    class MakeMeasurementViewModel : ViewModelBase
    {
        public CardConfig cardConfig;

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

        public MakeMeasurementViewModel()
        {
            cardConfig = new CardConfig();
        }

    }
}
