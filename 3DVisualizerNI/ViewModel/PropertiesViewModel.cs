﻿using _3DVisualizerNI.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace _3DVisualizerNI.ViewModel
{
    public class PropertiesViewModel : ViewModelBase
    {
        SpatialMeasurement spatialMeasurement;

        Scene3D model;
        IntersectionPoints intersectionPoints;

        public ObservableCollection<int> ResolutionList { get; set; }
        public int ResolutionSelected {
            get { if(spatialMeasurement != null) return spatialMeasurement.resolution;
                return 5;
            }
            set
            {
                spatialMeasurement.resolution = value;
                Messenger.Default.Send<SpatialMeasurement>(spatialMeasurement);
            }
        }
        public double pX
        {
            get
            {
                if (spatialMeasurement != null) return spatialMeasurement.position.X;
                return 0;
            }
            set
            {
                Vector3D newPos = spatialMeasurement.position;
                newPos.X = value;
                spatialMeasurement.position = newPos;
                Messenger.Default.Send<SpatialMeasurement>(spatialMeasurement);
            }
        }
        public double pY
        {
            get
            {
                if (spatialMeasurement != null) return spatialMeasurement.position.Y;
                return 0;
            }
            set
            {
                Vector3D newPos = spatialMeasurement.position;
                newPos.Y = value;
                spatialMeasurement.position = newPos;
                Messenger.Default.Send<SpatialMeasurement>(spatialMeasurement);
            }
        }
        public double pZ
        {
            get
            {
                if (spatialMeasurement != null) return spatialMeasurement.position.Z;
                return 0;
            }
            set
            {
                Vector3D newPos = spatialMeasurement.position;
                newPos.Z = value;
                spatialMeasurement.position = newPos;
                Messenger.Default.Send<SpatialMeasurement>(spatialMeasurement);
            }
        }
        public int ImpulseScale
        {
            get
            {
                if (spatialMeasurement != null) return spatialMeasurement.scale;
                return 5;
            }
            set
            {
                spatialMeasurement.scale = value;
                Messenger.Default.Send<SpatialMeasurement>(spatialMeasurement);
            }
        }

        public RelayCommand CalculateIPCommand { get; private set; }

        public PropertiesViewModel()
        {
            ResolutionList = new ObservableCollection<int>();
            ResolutionList.Add(1);
            ResolutionList.Add(2);
            ResolutionList.Add(5);
            ResolutionList.Add(10);
            ResolutionList.Add(15);

            this.CalculateIPCommand = new RelayCommand(this.CalculateIP);

            Messenger.Default.Register<SpatialMeasurement>
            (
                this,
                (sm) => ReceiveResponse(sm)
            );

            Messenger.Default.Register<Scene3D>
            (
                this,
                (model) => ReceiveModel(model)
            );

            Messenger.Default.Register<IntersectionPoints>
            (
                this,
                (intersectionPoints) => ReceiveIntersectionPoints(intersectionPoints)
            );
        }

        private object ReceiveResponse(SpatialMeasurement sm)
        {
            spatialMeasurement = sm;
            return null;
        }

        private object ReceiveModel(Scene3D _model)
        {
            model = _model;
            return null;
        }

        private object ReceiveIntersectionPoints(IntersectionPoints ip)
        {
            intersectionPoints = ip;
            return null;
        }

        private void CalculateIP()
        {
            intersectionPoints = new IntersectionPoints();
            intersectionPoints.calculateIntersectionPoints(model.model, spatialMeasurement);
            intersectionPoints.bilidIntersectionModel();

            Messenger.Default.Send<IntersectionPoints>(intersectionPoints);
        }


    }
}
