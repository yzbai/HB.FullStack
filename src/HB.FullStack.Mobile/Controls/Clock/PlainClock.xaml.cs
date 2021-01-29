using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using HB.FullStack.Mobile.Base;
using HB.FullStack.Mobile.Skia;

using SkiaSharp.Views.Forms;

using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HB.FullStack.Mobile.Controls.Clock
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "所有BaseSkiaContentView，Figures都在RemoveFigures中调用")]
    public partial class PlainClock : BaseSkiaContentView
    {
        public static readonly BindableProperty DescriptionProperty = BindableProperty.Create(nameof(Description), typeof(ClockDescription), typeof(PlainClock), new ClockDescription(), propertyChanged: (b, o, n) => { ((PlainClock)b).OnDescriptionChanged(); });
        public static readonly BindableProperty TimeBlockDrawInfosProperty = BindableProperty.Create(nameof(TimeBlockDrawInfos), typeof(ObservableRangeCollection<TimeBlockDrawInfo>), typeof(PlainClock), new ObservableRangeCollection<TimeBlockDrawInfo>(), propertyChanged: (b, o, n) => { ((PlainClock)b).OnTimeBlockDrawInfosChanged((ObservableRangeCollection<TimeBlockDrawInfo>)o, (ObservableRangeCollection<TimeBlockDrawInfo>)n); });

        #region Privates

        private TimeBlockFigureGroup? _currentTimeBlockFigureGroup;
        private Time24Hour? _selectedTimeBlockStartTime;

        //因为Databinding, 只能public
        public ObservableRangeCollection<SKFigure> BackgroundFigures { get; private set; } = new ObservableRangeCollection<SKFigure>();

        public ObservableRangeCollection<SKFigure> ForegroundFigures { get; private set; } = new ObservableRangeCollection<SKFigure>();

        #endregion Privates

        public PlainClock()
        {
            InitializeComponent();
        }

        #region Inputs

        public ClockDescription Description { get => (ClockDescription)GetValue(DescriptionProperty); set => SetValue(DescriptionProperty, value); }
        public ObservableRangeCollection<TimeBlockDrawInfo> TimeBlockDrawInfos { get => (ObservableRangeCollection<TimeBlockDrawInfo>)GetValue(TimeBlockDrawInfosProperty); set => SetValue(TimeBlockDrawInfosProperty, value); }

        #endregion Inputs

        #region Outputs

        public Time24Hour? SelectedTimeBlockStartTime
        {
            get { return _selectedTimeBlockStartTime; }
            private set { _selectedTimeBlockStartTime = value; OnPropertyChanged(); }
        }

        #endregion Outputs

        public override IList<IBaseContentView?>? GetAllCustomerControls()
        {
            return new List<IBaseContentView?> { BackgroundCanvasView, ForegroundCanvasView };
        }

        protected override void DisposeFigures()
        {
            _currentTimeBlockFigureGroup?.Dispose();
            _currentTimeBlockFigureGroup = null;

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
            foreach (var f in ForegroundFigures)
            {
                f?.Dispose();
            }
            ForegroundFigures.Clear();

            List<SKFigure> toAdds = new List<SKFigure>();

            //TimeBlock
            _currentTimeBlockFigureGroup?.Dispose();
            _currentTimeBlockFigureGroup = new TimeBlockFigureGroup();

            foreach (var drawInfo in TimeBlockDrawInfos)
            {
#pragma warning disable CA2000 // Dispose objects before losing scope
                TimeBlockFigure timeBlockFigure = new TimeBlockFigure(0.3f, 0.5f, drawInfo);
#pragma warning restore CA2000 // Dispose objects before losing scope
                _currentTimeBlockFigureGroup.AddFigure(timeBlockFigure);
            }

            toAdds.Add(_currentTimeBlockFigureGroup);

            //Dial Hand
            DialHandFigure dialHandFigure = new DialHandFigure(new SKRatioPoint(0.5f, 0.5f), Description.DialHandRatio, Description.DialHandRatio, Description.DialHandRatio);
            toAdds.Add(dialHandFigure);

            //End
            ForegroundFigures.AddRange(toAdds);
        }

        private void ReAddBackgroundFigures()
        {
            foreach (var f in BackgroundFigures)
            {
                f?.Dispose();
            }
            BackgroundFigures.Clear();

            List<SKFigure> backgroundToAdds = new List<SKFigure>();

            if (!string.IsNullOrEmpty(Description.DialBackgroundGifResourceName))
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

        private void OnDescriptionChanged()
        {
            ReAddFigures();
        }

        private void OnForegroundCanvasViewPainted(object sender, SKPaintSurfaceEventArgs e)
        {
            TimeBlockFigure? selectedTimeBlock = _currentTimeBlockFigureGroup?.SelectedFigures.FirstOrDefault();

            if (selectedTimeBlock != null)
            {
                _selectedTimeBlockStartTime = selectedTimeBlock.CurrentStartTime;
            }
            else
            {
                _selectedTimeBlockStartTime = null;
            }
        }

        private void OnTimeBlockDrawInfosChanged(ObservableRangeCollection<TimeBlockDrawInfo> o, ObservableRangeCollection<TimeBlockDrawInfo> n)
        {
            ReAddForegroundFigures();

            if (o != null)
            {
                o.CollectionChanged -= OnTimeBlockDrawInfosCollectionChanged;
            }

            if (n != null)
            {
                n.CollectionChanged += OnTimeBlockDrawInfosCollectionChanged;
            }
        }

        private void OnTimeBlockDrawInfosCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ReAddForegroundFigures();
        }
    }
}