using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using _3DVisualizerNI.Model;
using _3DVisualizerNI.Model.MeasurementTools;
using _3DVisualizerNI.Views;

namespace _3DVisualizerNI.ViewModel
{
    class MeasurementListViewModel : ViewModelBase
    {
        private Project project;
        public RelayCommand<SpatialMeasurement> SelectMeasuremetCommand { get; private set; }
        public RelayCommand<SpatialMeasurement> SaveResultAsWaveCommand { get; private set; }
        public RelayCommand<SpatialMeasurement> OpenTimeSeriesCommand { get; private set; }

        public ObservableCollection<SpatialMeasurement> MeasurementList
        {
            get { if(project!=null) return project.MeasurementList;
                return null;
            }
            set { project.MeasurementList = value; }
        }

        public MeasurementListViewModel()
        {
            Messenger.Default.Register<Project>
           (
               this,
               (proj) => ReceiveProject(proj)
           );

            SelectMeasuremetCommand = new RelayCommand<SpatialMeasurement>(SelectMeasurement);
            SaveResultAsWaveCommand = new RelayCommand<SpatialMeasurement>(SaveResultAsWave);
            OpenTimeSeriesCommand = new RelayCommand<SpatialMeasurement>(OpenTimeSeries);
        }

       
        private object ReceiveProject(Project proj)
        {
            project = proj;
            RaisePropertyChanged("MeasurementList");
            return null;
        }

        private void SelectMeasurement(SpatialMeasurement meas)
        {
            Messenger.Default.Send<SpatialMeasurement>(meas);
        }

        private void SaveResultAsWave(SpatialMeasurement meas)
        {
            meas.saveWaveResult();
        }

        private void OpenTimeSeries(SpatialMeasurement meas)
        {

            var timevector = Tools.getTimeVector(meas.measurementData.getLength(), meas.measurementData.Fs);
            AcceptResultsWindow resultsWindow = new AcceptResultsWindow();
            AcceptResultsViewModel vm = new AcceptResultsViewModel();
            resultsWindow.DataContext = vm;
            vm.PlotData(meas.measurementData.getAmplitudeArray(), meas.measurementData.getX(), meas.measurementData.getY(), meas.measurementData.getZ(), timevector);
            resultsWindow.ShowDialog();
        }
    }
}
