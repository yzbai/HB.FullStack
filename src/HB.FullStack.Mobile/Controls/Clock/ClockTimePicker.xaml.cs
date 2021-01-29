using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

using HB.FullStack.Mobile.Base;
using HB.FullStack.Mobile.Skia;

using SkiaSharp.Views.Forms;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HB.FullStack.Mobile.Controls.Clock
{
    /// <summary>
    /// 指针移动逻辑
    /// 时针：
    /// 1，时针可正反向双向移动
    /// 2，只有时针非常靠近某整点时，才纠正
    /// 分针：
    /// 1，只能正向移动
    /// 2，移动同时，时针跟着移动
    /// 3，移动结束，纠正时针，分针靠向最接近的刻度
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "在OnDisappearing中，会Dispose")]
    public partial class ClockTimePicker : BaseSkiaContentView
    {
        #region Binding OneWay

        public static readonly BindableProperty DescriptionProperty = BindableProperty.Create(nameof(Description), typeof(ClockDescription), typeof(ClockTimePicker), new ClockDescription(), BindingMode.OneWay, propertyChanged: (b, o, n) => { ((ClockTimePicker)b).OnDescriptionChanged(); });
        public static readonly BindableProperty DragedCommandProperty = BindableProperty.Create(nameof(DragedCommand), typeof(ICommand), typeof(ClockTimePicker), null, BindingMode.OneWay, null);
        public static readonly BindableProperty DragedCommandParameterProperty = BindableProperty.Create(nameof(DragedCommandParameter), typeof(object), typeof(ClockTimePicker), null, BindingMode.OneWay, null);
        public static readonly BindableProperty IsDragableProperty = BindableProperty.Create(nameof(IsDragable), typeof(bool), typeof(ClockTimePicker), true, BindingMode.OneWay, propertyChanged: (bindable, oldValue, newValue) => { ((ClockTimePicker)bindable).OnDragableChanged((bool)newValue); });
        public static readonly BindableProperty TimeBlockDrawInfosProperty = BindableProperty.Create(nameof(TimeBlockDrawInfos), typeof(ObservableCollection<TimeBlockDrawInfo>), typeof(ClockTimePicker), new ObservableCollection<TimeBlockDrawInfo>(), BindingMode.OneWay, propertyChanged: (bindable, oldValue, newValue) => { ((ClockTimePicker)bindable).OnTimeBlockDrawInfosChanged((ObservableCollection<TimeBlockDrawInfo>)oldValue, (ObservableCollection<TimeBlockDrawInfo>)newValue); });
        public static readonly BindableProperty InProgressTimeBlockDrawInfosProperty = BindableProperty.Create(nameof(InProgressTimeBlockDrawInfos), typeof(ObservableCollection<TimeBlockDrawInfo>), typeof(ClockTimePicker), new ObservableCollection<TimeBlockDrawInfo>(), BindingMode.OneWay, propertyChanged: (bindable, oldValue, newValue) => { ((ClockTimePicker)bindable).OnTimeBlockDrawInfosChanged((ObservableCollection<TimeBlockDrawInfo>)oldValue, (ObservableCollection<TimeBlockDrawInfo>)newValue); });

        #endregion Binding OneWay

        #region Binding OneWayToSource

        public static readonly BindableProperty IsAMProperty = BindableProperty.Create(nameof(IsAM), typeof(bool), typeof(ClockTimePicker), true, BindingMode.OneWayToSource);
        public static readonly BindableProperty HourResultProperty = BindableProperty.Create(nameof(HourResult), typeof(int), typeof(ClockTimePicker), 0, BindingMode.OneWayToSource);
        public static readonly BindableProperty MinuteResultProperty = BindableProperty.Create(nameof(MinuteResult), typeof(int), typeof(ClockTimePicker), 0, BindingMode.OneWayToSource);

        #endregion Binding OneWayToSource

        #region Privates

        private HourHandFigure? _hourHandFigure;
        private MinuteHandFigure? _minuteHandFigure;

        //因为Databinding, 只能public
        public ObservableCollection<SKFigure> BackgroundFigures { get; private set; } = new ObservableCollection<SKFigure>();

        public ObservableCollection<SKFigure> ForegroundFigures { get; private set; } = new ObservableCollection<SKFigure>();

        #endregion Privates

        public ClockTimePicker()
        {
            InitializeComponent();
        }

        #region Inputs

        public int InitialHour { get; set; }
        public int InitialMinute { get; set; }
        public bool CanHourHandAntiClockwise { get; set; }
        public bool CanMinuteHandAntiClockwise { get; set; }

        public ClockDescription Description { get => (ClockDescription)GetValue(DescriptionProperty); set => SetValue(DescriptionProperty, value); }

        public IEnumerable<TimeBlockDrawInfo>? TimeBlockDrawInfos { get { return (IEnumerable<TimeBlockDrawInfo>)GetValue(TimeBlockDrawInfosProperty); } set { SetValue(TimeBlockDrawInfosProperty, value); } }
        public IEnumerable<TimeBlockDrawInfo>? InProgressTimeBlockDrawInfos { get { return (IEnumerable<TimeBlockDrawInfo>)GetValue(InProgressTimeBlockDrawInfosProperty); } set { SetValue(InProgressTimeBlockDrawInfosProperty, value); } }

        public ICommand DragedCommand { get { return (ICommand)GetValue(DragedCommandProperty); } set { SetValue(DragedCommandProperty, value); } }
        public object DragedCommandParameter { get { return GetValue(DragedCommandParameterProperty); } set { SetValue(DragedCommandParameterProperty, value); } }
        public bool IsDragable { get { return (bool)GetValue(IsDragableProperty); } set { SetValue(IsDragableProperty, value); } }

        #endregion Inputs

        #region Outpus

        public bool IsAM { get { return (bool)GetValue(IsAMProperty); } set { SetValue(IsAMProperty, value); OnPropertyChanged(nameof(TimeString)); } }
        public int HourResult { get { return (int)GetValue(HourResultProperty); } set { SetValue(HourResultProperty, value); OnPropertyChanged(nameof(TimeString)); } }
        public int MinuteResult { get { return (int)GetValue(MinuteResultProperty); } set { SetValue(MinuteResultProperty, value); OnPropertyChanged(nameof(TimeString)); } }
        public string TimeString { get { string ampm = IsAM ? "AM" : "PM"; return $"{ampm}{HourResult}:{MinuteResult:d2}"; } }

        #endregion Outpus

        public override IList<IBaseContentView?>? GetAllCustomerControls()
        {
            return new List<IBaseContentView?> { BackgroundCanvasView, ForegroundCanvasView };
        }

        protected override void DisposeFigures()
        {
            if (_hourHandFigure != null)
            {
                _hourHandFigure.Dragged -= OnHandFigureDragged;
                _hourHandFigure.Dispose();
                _hourHandFigure = null;
            }

            if (_minuteHandFigure != null)
            {
                _minuteHandFigure.Dragged -= OnHandFigureDragged;
                _minuteHandFigure.Dispose();
                _minuteHandFigure = null;
            }

            foreach (var f in ForegroundFigures)
            {
                f?.Dispose();
            }
            ForegroundFigures.Clear();

            foreach (var f in BackgroundFigures)
            {
                f?.Dispose();
            }
            BackgroundFigures.Clear();
        }

        protected override void ReAddFigures()
        {
            ReAddBackgroundFigures();
            ReAddForegroundFigures();
        }

        private void ReAddForegroundFigures()
        {
            List<SKFigure> toAdds = new List<SKFigure>();

            if (TimeBlockDrawInfos != null)
            {
                foreach (var drawInfo in TimeBlockDrawInfos)
                {
                    toAdds.Add(new TimeBlockFigure(0.3f, 0.5f, drawInfo));
                }
            }

            if (InProgressTimeBlockDrawInfos != null)
            {
                foreach (var drawInfo in InProgressTimeBlockDrawInfos)
                {
                    toAdds.Add(new TimeBlockFigure(0.3f, 0.5f, drawInfo));
                }
            }

            _hourHandFigure = new HourHandFigure(Description.DialHandRatio, InitialHour) { PivotRatioPoint = new SKRatioPoint(0.5f, 0.5f) };
            _minuteHandFigure = new MinuteHandFigure(Description.DialHandRatio, InitialMinute, _hourHandFigure) { PivotRatioPoint = new SKRatioPoint(0.5f, 0.5f) };

            //_hourHandFigure.SetHour(InitialHour);
            //_minuteHandFigure.SetMinute(InitialMinute);

            _hourHandFigure.CanAntiClockwise = CanHourHandAntiClockwise;
            _minuteHandFigure.CanAntiClockwise = CanMinuteHandAntiClockwise;

            _hourHandFigure.Dragged += OnHandFigureDragged;
            _minuteHandFigure.Dragged += OnHandFigureDragged;

            toAdds.Add(_hourHandFigure);
            toAdds.Add(_minuteHandFigure);

            ForegroundFigures.AddRange(toAdds);
        }

        private void ReAddBackgroundFigures()
        {
            BackgroundFigures.Clear();

            List<SKFigure> backgroundToAdds = new List<SKFigure>();

            if (Description.DialBackgroundGifResourceName.IsNotNullOrEmpty())
            {
                GifCircleDialBackgroundFigure dialBackgroundFigure = new GifCircleDialBackgroundFigure(
                    new SKRatioPoint(0.5f, 0.5f),
                    Description.DialBackgroundRatio,
                    Description.DialBackgroundRatio,
                    Description.DialBackgroundGifResourceName!);

                backgroundToAdds.Add(dialBackgroundFigure);
            }

            TicksFigure ticksFigure = new TicksFigure(new SKRatioPoint(0.5f, 0.5f), Description.TicksRatio);

            backgroundToAdds.Add(ticksFigure);

            BackgroundFigures.AddRange(backgroundToAdds);
        }

        private void OnTimeBlockDrawInfosChanged(ObservableCollection<TimeBlockDrawInfo> oldValue, ObservableCollection<TimeBlockDrawInfo> newValue)
        {
            ReAddForegroundFigures();

            if (oldValue != null)
            {
                oldValue.CollectionChanged -= OnTimeBlockDrawInfosCollectionChanged;
            }

            if (newValue != null)
            {
                newValue.CollectionChanged += OnTimeBlockDrawInfosCollectionChanged;
            }
        }

        private void OnTimeBlockDrawInfosCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ReAddForegroundFigures();
        }

        private void OnDragableChanged(bool newValue)
        {
            if (_hourHandFigure != null)
            {
                _hourHandFigure.EnableDrag = newValue;
            }

            if (_minuteHandFigure != null)
            {
                _minuteHandFigure.EnableDrag = newValue;
            }
        }

        private void OnDescriptionChanged()
        {
            ReAddFigures();
        }

        private void OnForegroundCanvasViewPainted(object sender, SKPaintSurfaceEventArgs e)
        {
            //Map 各个Figure的值出来
            HourResult = _hourHandFigure == null ? 0 : _hourHandFigure.HourResult;
            MinuteResult = _minuteHandFigure == null ? 0 : _minuteHandFigure.MinuteResult;
            IsAM = _hourHandFigure == null || _hourHandFigure.IsAM;
        }

        private void OnHandFigureDragged(object sender, SKTouchInfoEventArgs e)
        {
            if (DragedCommand != null)
            {
                if (DragedCommand.CanExecute(DragedCommandParameter))
                {
                    DragedCommand.Execute(DragedCommandParameter);
                }
            }
        }
    }
}