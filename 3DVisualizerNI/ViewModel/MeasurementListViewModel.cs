﻿using System;
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

namespace _3DVisualizerNI.ViewModel
{
    class MeasurementListViewModel : ViewModelBase
    {
        private Project project;

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
        }

        public RelayCommand<SpatialMeasurement> SelectMeasuremetCommand { get; private set; }
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
    }
}
