﻿using HelixToolkit.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace _3DVisualizerNI.Model
{
    class Scene3D
    {
        public Model3DGroup model { get; set; }

        public Scene3D()
        {
            model = new Model3DGroup();
        }

        public void LoadModel()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                String path = openFileDialog.FileName;
                ModelImporter import = new ModelImporter();

                Model3DGroup group = import.Load(path);
                foreach (Model3D part in group.Children)
                {
                    ((GeometryModel3D)part).Material = null;
                }

                model = group;
                
            }
        }
    }
}