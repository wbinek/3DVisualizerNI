using Microsoft.Win32;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xceed.Wpf.Toolkit;

namespace _3DVisualizerNI.Model.Utilities
{
    static
    class ArrayToTxtExporter
    {

        static public void SaveListAsTxt(string path, List<List<object>> array, List<string> headers = null)
        {

            using (StreamWriter sw = File.CreateText(path))
            {
                if (headers != null)
                {
                    sw.WriteLine(String.Join(" ", headers));
                }
                foreach(List<object> row in array){
                    sw.WriteLine(String.Join(" ", row.ToArray()));
                }
            }
        }

        static public bool getSavePath(ref string path)
        {
            //Get File Path
            SaveFileDialog SaveDialog = new SaveFileDialog();
            SaveDialog.Filter = "txt files (*.txt)|*.txt";

            if (SaveDialog.ShowDialog() == true)
            {
                path = SaveDialog.FileName;
                return true;
            }
            return false;
        }
    }
}
