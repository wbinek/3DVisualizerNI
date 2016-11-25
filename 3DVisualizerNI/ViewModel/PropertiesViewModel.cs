using _3DVisualizerNI.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace _3DVisualizerNI.ViewModel
{
    public class PropertiesViewModel : ViewModelBase
    {
        #region Private Fields

        private bool _animationStartEnabled;
        private bool _ipDisplayEnabled;
        private AnimationParams animationParams;
        private DispatcherTimer aTimer;
        private IntersectionPoints intersectionPoints;
        private Scene3D model;
        private SpatialMeasurement spatialMeasurement;

        #endregion Private Fields

        #region Public Constructors

        public PropertiesViewModel()
        {
            ResolutionList = new ObservableCollection<int>();
            ResolutionList.Add(1);
            ResolutionList.Add(2);
            ResolutionList.Add(5);
            ResolutionList.Add(10);
            ResolutionList.Add(15);

            this.CalculateIPCommand = new RelayCommand(this.CalculateIP);
            this.ShowIPCommand = new RelayCommand(this.ShowIP);
            this.StartAnimationCommand = new RelayCommand(this.ShowAnimation);
            this.StopAnimationCommand = new RelayCommand(this.StopAnimation);
            this.AddDataColorCommand = new RelayCommand(AddDataColor);
            this.RemoveDataColorCommand = new RelayCommand<IList>(RemoveDataColor);

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

            //Messenger.Default.Register<IntersectionPoints>
            //(
            //    this,
            //    (intersectionPoints) => ReceiveIntersectionPoints(intersectionPoints)
            //);
        }

        #endregion Public Constructors

        #region Public Properties

        public RelayCommand AddDataColorCommand { get; private set; }
        public bool animationStartEnabled
        {
            get { return _animationStartEnabled; }
            set
            {
                _animationStartEnabled = value;
                RaisePropertyChanged("animationStartEnabled");
                RaisePropertyChanged("animationStopEnabled");
            }
        }

        public bool animationStopEnabled
        {
            get
            {
                if (ipDisplayEnabled == false) return false;
                return !animationStartEnabled;
            }
        }

        public RelayCommand CalculateIPCommand { get; private set; }
        public ObservableCollection<string> colorModes
        {
            get
            {
                if (intersectionPoints != null) return intersectionPoints.colorDisplayMode;
                return null;
            }
        }

        public string colorModeSelected
        {
            get
            {
                if (intersectionPoints != null) return intersectionPoints.sellectedColorDisplayMode;
                return "";
            }
            set
            {
                intersectionPoints.sellectedColorDisplayMode = value;
                RaisePropertyChanged("dataColors");
            }
        }

        public ObservableCollection<DataColour> dataColors
        {
            get
            {
                if (intersectionPoints != null) return intersectionPoints.currentColorSet;
                return null;
            }
            set
            {
                intersectionPoints.currentColorSet = value;
            }
        }

        public double directTime
        {
            get
            {
                if (spatialMeasurement != null)
                {
                    return (double)spatialMeasurement.getMaxIdx() / spatialMeasurement.Fs;
                }
                else return 0;
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
            }
        }

        public double intEndTime
        {
            get
            {
                if (intersectionPoints != null) return intersectionPoints.endTime;
                return 0;
            }
            set
            {
                intersectionPoints.endTime = value;
                RaisePropertyChanged("intEndTime");
                RaisePropertyChanged("intLength");
                RaisePropertyChanged("maxTimeSlider");
            }
        }

        public double intLength
        {
            get
            {
                return intEndTime - intStartTime;
            }
            set
            {
                intEndTime = intStartTime + value;
            }
        }

        public double intStartTime
        {
            get
            {
                if (intersectionPoints != null) return intersectionPoints.startTime;
                return 0;
            }
            set
            {
                intersectionPoints.startTime = value;
                RaisePropertyChanged("intStartTime");
                RaisePropertyChanged("intLength");
                RaisePropertyChanged("maxTimeSlider");
            }
        }

        public bool ipDisplayEnabled
        {
            get
            {
                return _ipDisplayEnabled;
            }
            set
            {
                animationStartEnabled = value;
                _ipDisplayEnabled = value;
                RaisePropertyChanged("ipDisplayEnabled");
                RaisePropertyChanged("isIntersectionPropertiesEnabled");
            }
        }

        public bool isIntersectionPropertiesEnabled
        {
            get
            {
                if (spatialMeasurement != null && model != null && ipDisplayEnabled) return true;
                return false;
            }
        }

        public bool isResponsePropertiesEnabled
        {
            get
            {
                if (spatialMeasurement != null) return true;
                return false;
            }
        }

        public double maxLevel
        {
            get
            {
                if (spatialMeasurement != null)
                {
                    return MeasurementUtils.todB(spatialMeasurement.getMax());
                }
                else return 0;
            }
        }

        public double maxTimeSlider
        {
            get
            {
                if (intersectionPoints != null) return (intersectionPoints.respLength / intersectionPoints.Fs) - (intEndTime - intStartTime);
                return 0;
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
            }
        }

        public RelayCommand<IList> RemoveDataColorCommand { get; private set; }
        public ObservableCollection<int> ResolutionList { get; set; }

        public int ResolutionSelected
        {
            get
            {
                if (spatialMeasurement != null) return spatialMeasurement.resolution;
                return 5;
            }
            set
            {
                spatialMeasurement.resolution = value;
            }
        }
        public RelayCommand ShowIPCommand { get; private set; }

        public RelayCommand StartAnimationCommand { get; private set; }

        public RelayCommand StopAnimationCommand { get; private set; }

        public double timeSlider
        {
            get
            {
                return intStartTime;
            }
            set
            {
                double diff = intEndTime - intStartTime;
                intEndTime = value + diff;
                intStartTime = value;
                RaisePropertyChanged("intStartTime");
                RaisePropertyChanged("intEndTime");

                //ShowIP(); //Uncomment to update display when slider is moved (very slow);
            }
        }

        #endregion Public Properties

        #region Public Methods

        public void AddDataColor()
        {
            DataColour newColor = new DataColour();
            dataColors.Add(newColor);
            return;
        }

        public void RemoveDataColor(IList toRemove)
        {
            var collection = toRemove.Cast<DataColour>();
            List<DataColour> copy = new List<DataColour>(collection);

            foreach (DataColour color in copy)
            {
                dataColors.Remove(color);
            }
        }

        #endregion Public Methods

        #region Private Methods

        private static void MyElapsedMethod(object sender, EventArgs e, PropertiesViewModel vm)
        {
            vm.intStartTime = vm.animationParams.frame * vm.animationParams.dt;
            vm.intEndTime = (vm.animationParams.frame + 1) * vm.animationParams.dt;
            vm.ShowIP();
            vm.animationParams.frame++;
            if (vm.animationParams.frame == vm.animationParams.framesNo - 1)
            {
                vm.aTimer.Stop();
            }
        }

        private void CalculateIP()
        {
            intersectionPoints = new IntersectionPoints();
            intersectionPoints.calculateIntersectionPoints(model.model, spatialMeasurement);

            intStartTime = intersectionPoints.startTime + (directTime - 0.005);
            intEndTime = intersectionPoints.endTime + directTime;

            RaisePropertyChanged("intStartTime");
            RaisePropertyChanged("intEndTime");
            RaisePropertyChanged("maxTimeSlider");
            RaisePropertyChanged("timeSlider");
            RaisePropertyChanged("dataColors");
            RaisePropertyChanged("colorModes");

            ipDisplayEnabled = true;
        }

        private object ReceiveModel(Scene3D _model)
        {
            model = _model;
            RaisePropertyChanged("isIntersectionPropertiesEnabled");
            return null;
        }

        private object ReceiveResponse(SpatialMeasurement sm)
        {
            spatialMeasurement = sm;
            RaisePropertyChanged("directTime");
            RaisePropertyChanged("maxLevel");
            RaisePropertyChanged("isResponsePropertiesEnabled");
            RaisePropertyChanged("isIntersectionPropertiesEnabled");

            ipDisplayEnabled = false;
            RaisePropertyChanged("ipDisplayEnabled");
            return null;
        }

        private void ShowAnimation()
        {
            animationParams = new AnimationParams();

            aTimer = new DispatcherTimer();
            aTimer.Tick += (sender, e) => MyElapsedMethod(sender, e, this);
            aTimer.Interval = TimeSpan.FromSeconds(0.5);
            aTimer.Start();

            animationParams.frame = 0;
            animationParams.dt = intEndTime - intStartTime;
            animationParams.framesNo = (int)(maxTimeSlider / animationParams.dt);

            animationStartEnabled = false;
        }

        //private object ReceiveIntersectionPoints(IntersectionPoints ip)
        //{
        //    intersectionPoints = ip;
        //    return null;
        //}
        private void ShowIP()
        {
            intersectionPoints.builidIntersectionModel();
            Messenger.Default.Send<IntersectionPoints>(intersectionPoints);
        }

        private void StopAnimation()
        {
            aTimer.Stop();
            animationStartEnabled = true;
        }

        #endregion Private Methods

        #region Private Classes

        private class AnimationParams
        {
            #region Public Properties

            public double dt { get; set; }
            public int frame { get; set; }
            public int framesNo { get; set; }

            #endregion Public Properties
        }

        #endregion Private Classes
    }
}