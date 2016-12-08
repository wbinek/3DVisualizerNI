using HelixToolkit.Wpf;
using Microsoft.Win32;
using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace _3DVisualizerNI.Model
{
    /// <summary>
    /// Class containing 3D room model
    /// </summary>
    public class Scene3D
    {
        /// <summary>
        /// 3D Model object
        /// </summary>
        public Model3DGroup model { get; set; }

        /// <summary>
        /// Default creator
        /// </summary>
        public Scene3D()
        {
            model = new Model3DGroup();
        }

        /// <summary>
        /// Method for loading model showing dialog
        /// </summary>
        public void LoadModel()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "3ds files (*.3ds)|*.3ds";

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
                model.Children.Add(new AmbientLight(Colors.DimGray));
            }
        }

        /// <summary>
        /// Method for loading model form given path
        /// </summary>
        /// <param name="path">path to load from</param>
        public void LoadModel(string path)
        {
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