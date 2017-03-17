using _3DVisualizerNI.Model;
using _3DVisualizerNI.Views;
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

    /// <summary>
    /// ViewModel connecting properties bar with models
    /// </summary>
    public class PropertiesViewModel : ViewModelBase
    {
        #region Private Fields

        private bool _animationStartEnabled;
        private bool _isIntersectionPointsDisplayEnabled;
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

            this.CalculateIPCommand = new RelayCommand(this.CalculateIntersectionPoints);
            this.ShowIPCommand = new RelayCommand(this.ShowIntersectionPoints);
            this.StartAnimationCommand = new RelayCommand(this.StartAnimation);
            this.StopAnimationCommand = new RelayCommand(this.StopAnimation);
            this.AddDataColorCommand = new RelayCommand(AddDataColor);
            this.RemoveDataColorCommand = new RelayCommand<IList>(RemoveDataColor);
            this.PeakDetectionCommand = new RelayCommand(PeakDetection);

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
        public RelayCommand CalculateIPCommand { get; private set; }

        public ObservableCollection<string> colorModes
        {
            get
            {
                if (intersectionPoints != null) return intersectionPoints.DisplayProperties.colorDisplayMode;
                return null;
            }
        }

        public string colorModeSelected
        {
            get
            {
                if (intersectionPoints != null) return intersectionPoints.DisplayProperties.selectedColorDisplayMode;
                return "";
            }
            set
            {
                intersectionPoints.DisplayProperties.selectedColorDisplayMode = value;
                RaisePropertyChanged("dataColors");
            }
        }

        public bool constantSizeSelected
        {
            get
            {
                if (intersectionPoints != null) return intersectionPoints.DisplayProperties.constantMarkerSize;
                return false;
            }
            set
            {
                intersectionPoints.DisplayProperties.constantMarkerSize = value;
            }
        }

        public ObservableCollection<DataColour> dataColors
        {
            get
            {
                if (intersectionPoints != null) return intersectionPoints.DisplayProperties.currentColorSet;
                return null;
            }
            set
            {
                intersectionPoints.DisplayProperties.currentColorSet = value;
            }
        }

        public double directTime
        {
            get
            {
                if (spatialMeasurement != null)
                {
                    return (double)spatialMeasurement.measurementData.getMaxIdx() / spatialMeasurement.measurementData.Fs;
                }
                else return 0;
            }
        }

        public int ImpulseScale
        {
            get
            {
                if (spatialMeasurement != null) return spatialMeasurement.measurementScale;
                return 5;
            }
            set
            {
                spatialMeasurement.measurementScale = value;
            }
        }

        public double intEndTime
        {
            get
            {
                if (intersectionPoints != null) return intersectionPoints.ResponseProperties.respEndTime;
                return 0;
            }
            set
            {
                intersectionPoints.ResponseProperties.respEndTime = value;
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
                if (intersectionPoints != null) return intersectionPoints.ResponseProperties.respStartTime;
                return 0;
            }
            set
            {
                intersectionPoints.ResponseProperties.respStartTime = value;
                RaisePropertyChanged("intStartTime");
                RaisePropertyChanged("intLength");
                RaisePropertyChanged("maxTimeSlider");
            }
        }

        public bool isAnimationStartEnabled
        {
            get { return _animationStartEnabled; }
            set
            {
                _animationStartEnabled = value;
                RaisePropertyChanged("isAnimationStartEnabled");
                RaisePropertyChanged("isAnimationStopEnabled");
            }
        }

        public bool isAnimationStopEnabled
        {
            get
            {
                return !isAnimationStartEnabled;
            }
        }

        public bool isIntersectionPointsDisplayEnabled
        {
            get
            {
                return _isIntersectionPointsDisplayEnabled;
            }
            set
            {
                isAnimationStartEnabled = value;
                _isIntersectionPointsDisplayEnabled = value;
                RaisePropertyChanged("isIntersectionPointsDisplayEnabled");
                RaisePropertyChanged("isIntersectionPropertiesEnabled");
                RaisePropertyChanged("isAnimationStartEnabled");
            }
        }

        public bool isCalculateInstersecitionPointsEnabled
        {
            get
            {
                if (spatialMeasurement != null && model != null) return true;
                return false;
            }
        }

        public bool isIntersectionPropertiesEnabled
        {
            get
            {
                if (spatialMeasurement != null && model != null && isIntersectionPointsDisplayEnabled) return true;
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
                    return MeasurementUtils.todB(spatialMeasurement.measurementData.getMax());
                }
                else return 0;
            }
        }

        public double totalLevel
        {
            get
            {
                if (spatialMeasurement != null)
                {
                    return spatialMeasurement.measurementData.getTotalLevel();
                }
                else return 0;
            }
        }

        public double maxTimeSlider
        {
            get
            {
                if (intersectionPoints != null) return (intersectionPoints.ResponseProperties.respLength / intersectionPoints.ResponseProperties.Fs) - (intEndTime - intStartTime);
                return 0;
            }
        }

        public bool peaksOnlySelected
        {
            get
            {
                if (intersectionPoints != null) return intersectionPoints.DisplayProperties.showPeaksOnly;
                return false;
            }
            set
            {
                intersectionPoints.DisplayProperties.showPeaksOnly = value;
            }
        }

        public Vector3D MeasurementPosition
        {
            get
            {
                if (spatialMeasurement != null) return spatialMeasurement.measurementPosition;
                return new Vector3D(0,0,0);
            }
            set
            {
                spatialMeasurement.measurementPosition = value;
            }
        }

        public RelayCommand<IList> RemoveDataColorCommand { get; private set; }
        public ObservableCollection<int> ResolutionList { get; set; }
        public int ResolutionSelected
        {
            get
            {
                if (spatialMeasurement != null) return spatialMeasurement.measurementResolution;
                return 5;
            }
            set
            {
                spatialMeasurement.measurementResolution = value;
            }
        }
        public RelayCommand ShowIPCommand { get; private set; }

        public bool showPeaksOnly
        {
            get
            {
                if (intersectionPoints != null) return intersectionPoints.DisplayProperties.constantMarkerSize;
                return false;
            }
            set
            {
                intersectionPoints.DisplayProperties.constantMarkerSize = value;
            }
        }

        public RelayCommand StartAnimationCommand { get; private set; }

        public RelayCommand StopAnimationCommand { get; private set; }

        public RelayCommand PeakDetectionCommand { get; private set; }

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
            vm.ShowIntersectionPoints();
            vm.animationParams.frame++;
            if (vm.animationParams.frame == vm.animationParams.framesNo - 1)
            {
                vm.aTimer.Stop();
            }
        }

        private void CalculateIntersectionPoints()
        {
            intersectionPoints = new IntersectionPoints();
            intersectionPoints.calculateIntersectionPoints(model.model, spatialMeasurement);

            intStartTime = intersectionPoints.ResponseProperties.respStartTime + (directTime - 0.005);
            intEndTime = intersectionPoints.ResponseProperties.respEndTime + directTime;

            RaisePropertyChanged("intStartTime");
            RaisePropertyChanged("intEndTime");
            RaisePropertyChanged("maxTimeSlider");
            RaisePropertyChanged("timeSlider");
            RaisePropertyChanged("dataColors");
            RaisePropertyChanged("colorModes");
            RaisePropertyChanged("colorModeSelected");

            isIntersectionPointsDisplayEnabled = true;
        }

        private object ReceiveModel(Scene3D _model)
        {
            model = _model;
            RaisePropertyChanged("isCalculateInstersecitionPointsEnabled");
            RaisePropertyChanged("isIntersectionPropertiesEnabled");
            return null;
        }

        private object ReceiveResponse(SpatialMeasurement sm)
        {
            spatialMeasurement = sm;
            RaisePropertyChanged("directTime");
            RaisePropertyChanged("maxLevel");
            RaisePropertyChanged("totalLevel");
            RaisePropertyChanged("isResponsePropertiesEnabled");
            RaisePropertyChanged("isCalculateInstersecitionPointsEnabled");
            RaisePropertyChanged("isIntersectionPropertiesEnabled");
            RaisePropertyChanged("MeasurementPosition");
            RaisePropertyChanged("ImpulseScale");

            isIntersectionPointsDisplayEnabled = false;
            return null;
        }

        private void StartAnimation()
        {
            animationParams = new AnimationParams();

            aTimer = new DispatcherTimer();
            aTimer.Tick += (sender, e) => MyElapsedMethod(sender, e, this);
            aTimer.Interval = TimeSpan.FromSeconds(0.5);
            aTimer.Start();

            animationParams.frame = 0;
            animationParams.dt = intEndTime - intStartTime;
            animationParams.framesNo = (int)(maxTimeSlider / animationParams.dt);

            isAnimationStartEnabled = false;
        }

        private void ShowIntersectionPoints()
        {
            intersectionPoints.builidIntersectionModel();
            Messenger.Default.Send<IntersectionPoints>(intersectionPoints);
        }

        private void StopAnimation()
        {
            aTimer.Stop();
            isAnimationStartEnabled = true;
        }

        private void PeakDetection()
        {
            PeakFindWindow pfWindow = new PeakFindWindow();
            PeakFindViewModel pfViewModel = new PeakFindViewModel(intersectionPoints.PeakFindResults);
            pfViewModel.amplitudes = spatialMeasurement.measurementData.getAmplitudeArray();
            pfViewModel.Fs = spatialMeasurement.measurementData.Fs;
            pfWindow.DataContext = pfViewModel;
            ((PeakFindViewModel)pfWindow.DataContext).amplitudes = spatialMeasurement.measurementData.getAmplitudeArray();
            ((PeakFindViewModel)pfWindow.DataContext).Fs = spatialMeasurement.measurementData.Fs;
            ((PeakFindViewModel)pfWindow.DataContext).InitPlotModel();


            pfWindow.Show();           
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