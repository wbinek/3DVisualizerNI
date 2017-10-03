using Microsoft.Win32;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using Xceed.Wpf.Toolkit;

namespace _3DVisualizerNI.Model.Utilities
{
    static
    class waveSaveRead
    {
        /// <summary>
        /// Imports result from wave at given path
        /// </summary>
        /// <param name="path">Path to wavfile</param>
        static public void readWaveResult(string path,ref double[] w, ref double[] x, ref double[] y, ref double[] z, ref int Fs)
        {
            AudioFileReader reader = new AudioFileReader(path);

            if (reader.WaveFormat.Channels != 4)
            {
                MessageBox.Show("Wave doesn't contain four channels. Can't import.");
                return;
            }

            List<float> _w = new List<float>();
            List<float> _x = new List<float>();
            List<float> _y = new List<float>();
            List<float> _z = new List<float>();

            float[] buffer = new float[reader.WaveFormat.Channels];
            Fs = reader.WaveFormat.SampleRate;

            int read = reader.Read(buffer, 0, reader.WaveFormat.Channels);
            double scale = 1;
            while (read > 0)
            {
                _w.Add(buffer[0]);
                _x.Add(buffer[1]);
                _y.Add(buffer[2]);
                _z.Add(buffer[3]);
                read = reader.Read(buffer, 0, reader.WaveFormat.Channels);

                if (read == 1)
                {
                    scale = buffer[0];
                    break;
                }
            }
            w = Array.ConvertAll(_w.ToArray(), s => (double)(s / scale));
            x = Array.ConvertAll(_x.ToArray(), s => (double)(s / scale));
            y = Array.ConvertAll(_y.ToArray(), s => (double)(s / scale));
            z = Array.ConvertAll(_z.ToArray(), s => (double)(s / scale));
        }

        static public void saveResultAsWave(string path, double[] w, double[] x, double[] y, double[] z,int Fs)
        {
            double maxVal = w.Select(sample => Math.Abs(sample)).Max();
            if (maxVal < 1) maxVal = 1;

            WaveFormat format = WaveFormat.CreateIeeeFloatWaveFormat(Fs, 4);
            WaveFileWriter writer = new WaveFileWriter(path, format);
            for (int i = 0; i < w.Length; i++)
            {

                writer.WriteSample((float)(w[i] / maxVal));
                writer.WriteSample((float)(x[i] / maxVal));
                writer.WriteSample((float)(y[i] / maxVal));
                writer.WriteSample((float)(z[i] / maxVal));
            }

            writer.WriteSample((float)(1 / maxVal));
            writer.Close();
        }

        static public bool getSavePath(ref string path)
        {
            //Get File Path
            SaveFileDialog SaveDialog = new SaveFileDialog();
            SaveDialog.Filter = "wav files (*.wav)|*.wav";

            if (SaveDialog.ShowDialog() == true)
            {
                path = SaveDialog.FileName;
                return true;
            }
            return false;
        }

        static public bool getLoadPaths(ref string[] paths)
        {
            //Get File Path
            OpenFileDialog OpenDialog = new OpenFileDialog();
            OpenDialog.Filter = "wav files (*.wav)|*.wav";
            OpenDialog.Multiselect = true;

            if (OpenDialog.ShowDialog() == true)
            {
                paths = OpenDialog.FileNames;
                return true;
            }
            return false;
        }
    }
}
