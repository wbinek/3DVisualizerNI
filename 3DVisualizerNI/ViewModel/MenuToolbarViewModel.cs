using _3DVisualizerNI.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Win32;
using _3DVisualizerNI.Views;
using _3DVisualizerNI.Model.Utilities;

namespace _3DVisualizerNI.ViewModel
{
    /// <summary>
    /// View Model connecting menu toolbar with appropriate classes.
    /// </summary>
    public class MenuToolbarViewModel : ViewModelBase
    {
        private Project project;
        private Scene3D scene3D;
        private SpatialMeasurement spatialMeasurement;
        private IntersectionPoints intersectionPoints;

        public MenuToolbarViewModel()
        {
            this.LoadModelCommand = new RelayCommand(this.LoadModel);
            this.LoadMeasurementCommand = new RelayCommand(this.LoadMeasurement);
            this.MakeMeasurementCommand = new RelayCommand(this.MakeMeasurement);
            this.NewProjectCommand = new RelayCommand(this.NewProject);
            this.SaveProjectCommand = new RelayCommand(this.SaveProject);
            this.LoadProjectCommand = new RelayCommand(this.LoadProject);
            this.SaveProjectIntersecionPointsCommand = new RelayCommand(this.SaveProjectIntersecionPoints);

            Messenger.Default.Register<IntersectionPoints>
            (
                this,
                (ip) => ReceiveIntersectionPoints(ip)
            );
        }

        public RelayCommand LoadMeasurementCommand { get; private set; }
        public RelayCommand MakeMeasurementCommand { get; private set; }
        public RelayCommand LoadModelCommand { get; private set; }
        public RelayCommand NewProjectCommand { get; private set; }
        public RelayCommand SaveProjectCommand { get; private set; }
        public RelayCommand LoadProjectCommand { get; private set; }
        public RelayCommand SaveProjectIntersecionPointsCommand { get; private set; }

        public void LoadMeasurement()
        {
            string[] paths = new string[0];
            if (waveSaveRead.getLoadPaths(ref paths) == true)
            {
                foreach (var path in paths)
                {
                    spatialMeasurement = new SpatialMeasurement();
                    spatialMeasurement.importWaveResult(path);
                    Messenger.Default.Send<SpatialMeasurement>(spatialMeasurement, "AddToList");
                }              
            }         
        }

        public void SaveProject()
        {
            //Get File Path
            string path;
            SaveFileDialog SaveDialog = new SaveFileDialog();
            SaveDialog.Filter = "rmni files (*.rmni)|*.rmni";

            if (SaveDialog.ShowDialog() == true)
            {
                path = SaveDialog.FileName;
                project.WriteToBinaryFile(path);
            }
        }

        public void SaveProjectIntersecionPoints()
        {
            if(intersectionPoints!=null)
                intersectionPoints.SaveIntersectionPointsAsTxt();
        }

        public void LoadProject()
        {
            //Get File Path
            OpenFileDialog OpenDialog = new OpenFileDialog();
            OpenDialog.Filter = "rmni files (*.rmni)|*.rmni";
            OpenDialog.Multiselect = false;

            if (OpenDialog.ShowDialog() == true)
            {
                project = new Project();
                project.LoadFromBinaryFile(OpenDialog.FileName);
                Messenger.Default.Send<Project>(project);
            }
        }

        public void LoadModel()
        {
            scene3D = new Scene3D();
            scene3D.LoadModel();

            Messenger.Default.Send<Scene3D>(scene3D);
        }

        public void NewProject()
        {
            project = new Project();
            Messenger.Default.Send<Project>(project);
        }

        public void MakeMeasurement()
        {
            MakeMeasurementWindow measWindow = new MakeMeasurementWindow();
            MakeMeasurementViewModel measViewModel = new MakeMeasurementViewModel();
            measWindow.DataContext = measViewModel;
            measWindow.Show();
        }

        public void ReceiveIntersectionPoints(IntersectionPoints ip)
        {
            intersectionPoints = ip;
        }
    }
}