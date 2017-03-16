using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NationalInstruments.DAQmx;

namespace _3DVisualizerNI.Model.MeasurementTools
{
    [Serializable]
    public class ChannelConfig
    {
        public ChannelConfig(string name, bool active, double sens)
        {
            chName = name;
            chActive = active;
            chSens = sens;
        }

        public string chName { get; set; }
        public bool chActive { get; set; }
        public double chSens { get; set; }
    }

    [Serializable]
    public class CardConfig
    {
        public List<int> AvalibleSampling
        {
            get
            {
                List< int > SamplingRates = new List<int>();
                SamplingRates.Add(51200);
                SamplingRates.Add(102400);
                return SamplingRates;
            }
        }

        public List<int> AvalibleSampleToRead
        {
            get
            {
                List<int> SampleToRead = new List<int>();
                SampleToRead.Add(2560);
                SampleToRead.Add(5120);
                SampleToRead.Add(10240);
                SampleToRead.Add(25600);
                SampleToRead.Add(51200);
                SampleToRead.Add(102400);
                return SampleToRead;
            }
        }

        public Array AvalibleAITerminalConfiguration
        {
            get { return Enum.GetValues(typeof(AITerminalConfiguration)); }
        }

        public Array AvalibleIEPESource
        {
            get { return Enum.GetValues(typeof(AIExcitationSource)); }
        }

        public CardConfig()
        {
            chConfig = new List<ChannelConfig>();
            aoChannelAvalible = new List<string>();

            var chName = new string[0];

            try
            {
                chName = DaqSystem.Local.GetPhysicalChannels(PhysicalChannelTypes.AI, PhysicalChannelAccess.External);
                aoChannelAvalible =
                    DaqSystem.Local.GetPhysicalChannels(PhysicalChannelTypes.AO, PhysicalChannelAccess.External)
                        .ToList();
            }
            catch (DaqException)
            {
                System.Windows.MessageBox.Show("DAQmx driver not running");
            }

            foreach (var input in chName)
            {
                var channel = new ChannelConfig(input, false, 50);
                chConfig.Add(channel);
            }

            try
            {
                chConfig[0].chActive = true;
            }
            catch (ArgumentOutOfRangeException)
            {
                System.Windows.MessageBox.Show("No input channel detected");
            }

            try
            {
                aoChannelSelected = aoChannelAvalible[0];
            }
            catch (ArgumentOutOfRangeException)
            {
                System.Windows.MessageBox.Show("No output channel detected");
            }

            try
            {
                for(int i=1;i<4;i++)
                    chConfig[i].chActive = true;
            }
            catch (ArgumentOutOfRangeException)
            {
                System.Windows.MessageBox.Show("Not enough input channels to carry measurement. Use at least 4 channels.");
            }

            loadDefault();
        }

        public List<ChannelConfig> chConfig { get; set; }

        public int chSmplRate { get; set; }
        public int chSmplToRead { get; set; }
        public double chMaxPress { get; set; }
        public AITerminalConfiguration terminalConfig { get; set; }
        public AIExcitationSource excitationSource { get; set; }
        public double chIEPEVal { get; set; }

        public List<string> aoChannelAvalible { get; set; }
        public string aoChannelSelected { get; set; }
        public decimal aoMax { get; set; }
        public decimal aoMin { get; set; }

        public double sigLength { get; set; }

        public void loadDefault()
        {
            chSmplRate = 51200;
            chSmplToRead = 5120;
            chMaxPress = 120;
            terminalConfig = AITerminalConfiguration.Pseudodifferential;
            excitationSource = AIExcitationSource.None;
            chIEPEVal = 0.002;
            aoMax = 1;
            aoMin = -1;
        }

        public string[] getActiveChannels()
        {
            var result = new List<string>();
            foreach (var channel in chConfig)
                if (channel.chActive)
                    result.Add(channel.chName);
            return result.ToArray();
        }

        public int getNumActiveChannels()
        {
            var numActive = 0;
            foreach (var channel in chConfig)
                if (channel.chActive)
                    numActive++;
            return numActive;
        }
    }
}
