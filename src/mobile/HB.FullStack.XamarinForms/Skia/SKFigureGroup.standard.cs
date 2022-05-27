
using HB.FullStack.Common;
using HB.FullStack.Common.Figures;

using SkiaSharp;
using SkiaSharp.Views.Forms;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Linq;

using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace HB.FullStack.XamarinForms.Skia
{
    public interface ISKFigureGroup
    {
        bool AutoBringToFront { get; set; }

        bool EnableMultipleSelected { get; set; }

        bool EnableMultipleLongSelected { get; set; }

        bool EnableUnSelectedByHitFailed { get; set; }

        void OnDrawInfoOrCanvasSizeChanged();
    }

    public interface IFigureFactory<TData> where TData : FigureData
    {
        SKDataFigure<TData> Create();
    }

    public abstract class SKFigureGroup<TDrawInfo, TData> : SKFigure, ISKFigureGroup
        //where TFigure : SKFigure<EmptyDrawInfo, TData>
        where TDrawInfo : FigureDrawInfo
        where TData : FigureData
    {
        public static BindableProperty DrawInfoProperty = BindableProperty.Create(
               nameof(DrawInfo),
               typeof(TDrawInfo),
               typeof(SKFigureGroup<TDrawInfo, TData>),
               null,
               BindingMode.OneWay,
               propertyChanged: (b, oldValues, newValues) => ((SKFigureGroup<TDrawInfo, TData>)b).OnBaseDrawDatasChanged());

        public static BindableProperty InitDatasProperty = BindableProperty.Create(
            nameof(InitDatas),
            typeof(IList<TData>),
            typeof(SKFigureGroup<TDrawInfo, TData>),
            null,
            BindingMode.OneWay,
            propertyChanged: (b, oldValues, newValues) => ((SKFigureGroup<TDrawInfo, TData>)b).OnInitDatasChanged((IList<TData>?)oldValues, (IList<TData>?)newValues));


        public static BindableProperty ResultDatasProperty = BindableProperty.Create(
            nameof(ResultDatas),
            typeof(IList<TData>),
            typeof(SKFigureGroup<TDrawInfo, TData>),
            null,
            BindingMode.OneWayToSource);

        private readonly IFigureFactory<TData> _figureFactory;

        public bool AutoBringToFront { get; set; } = true;

        public bool EnableMultipleSelected { get; set; }

        public bool EnableMultipleLongSelected { get; set; }

        public bool EnableUnSelectedByHitFailed { get; set; } = true;

        public TDrawInfo? DrawInfo { get => (TDrawInfo?)GetValue(DrawInfoProperty); set => SetValue(DrawInfoProperty, value); }

        public IList<TData?>? InitDatas { get => (IList<TData?>)GetValue(InitDatasProperty); set => SetValue(InitDatasProperty, value); }

        public IList<TData?>? ResultDatas { get => (IList<TData?>)GetValue(ResultDatasProperty); set => SetValue(ResultDatasProperty, value); }

        protected IList<SKDataFigure<TData>> Figures { get; } = new List<SKDataFigure<TData>>();

        protected Dictionary<long, SKDataFigure<TData>> HittingFigures { get; } = new Dictionary<long, SKDataFigure<TData>>();

        public IList<SKDataFigure<TData>> SelectedFigures { get; } = new List<SKDataFigure<TData>>();

        public FigureVisualState SelectedFiguresState { get; private set; }

        protected SKFigureGroup(IFigureFactory<TData> figureFactory)
        {
            _figureFactory = figureFactory;

            Pressed += OnPressed;
            Tapped += OnTapped;
            LongTapped += OnLongTapped;
            OneFingerDragged += OnDragged;
            Cancelled += OnCancelled;
            HitFailed += OnHitFailed;
        }

        private void OnInitDatasChanged(IList<TData>? oldValues, IList<TData>? newValues)
        {
            //Create Figures
            ResumeFigures();

            if (oldValues is ObservableCollection<TData> collection)
            {
                collection.CollectionChanged -= OnInitDatasCollectionChanged;
            }

            if (newValues is ObservableCollection<TData> newCollection)
            {
                newCollection.CollectionChanged += OnInitDatasCollectionChanged;
            }

            InvalidateMatrixAndSurface();
        }

        private void OnInitDatasCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ResumeFigures();

            InvalidateMatrixAndSurface();
        }

        private void OnBaseDrawDatasChanged()
        {
            OnDrawInfoOrCanvasSizeChanged();

            InvalidateMatrixAndSurface();
        }

        public abstract void OnDrawInfoOrCanvasSizeChanged();

        public override void OnPaint(SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            if (CanvasSize != info.Size)
            {
                CanvasSize = info.Size;
                CanvasSizeChanged = true;
                HitTestPathNeedUpdate = true;

                OnDrawInfoOrCanvasSizeChanged();
            }
            else
            {
                CanvasSizeChanged = false;
            }

            for (int i = 0; i < Figures.Count; ++i)
            {
                using (new SKAutoCanvasRestore(canvas))
                {
                    try
                    {
                        Figures[i].OnPaint(e);
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        GlobalSettings.ExceptionHandler(ex);
                    }
                }
            }
        }

        public override bool OnHitTest(SKPoint location, long fingerId)
        {
            bool founded = false;

            for (int i = Figures.Count - 1; i >= 0; i--)
            {
                SKDataFigure<TData> figure = Figures[i];

                if (!founded && figure.OnHitTest(location, fingerId))
                {
                    founded = true;

                    HittingFigures[fingerId] = figure;
                }
                else
                {
                    figure.ProcessUnTouchAction(fingerId, location);
                }
            }

            return founded;
        }

        private void ResumeFigures()
        {
            ClearFigures();

            if (InitDatas.IsNullOrEmpty())
            {
                return;
            }

            ResultDatas = new ObservableCollection<TData?>(Enumerable.Repeat<TData?>(null, InitDatas.Count));

            for (int i = 0; i < InitDatas.Count; ++i)
            {
                SKDataFigure<TData> figure = _figureFactory.Create();
                figure.Parent = this;
                figure.CanvasView = this.CanvasView;

                //figure.SetBinding(SKFigure<TDrawInfo, TData>.DrawInfoProperty, new Binding(nameof(DrawInfo), source: this));
                figure.SetBinding(SKDataFigure<TData>.InitDataProperty, new Binding($"{nameof(InitDatas)}[{i}]", source: this));
                figure.SetBinding(SKDataFigure<TData>.ResultDataProperty, new Binding($"{nameof(ResultDatas)}[{i}]", source: this));

                Figures.Add(figure);
            }
        }

        public void ClearFigures()
        {
            HittingFigures.Clear();
            SelectedFigures.Clear();

            //InitDatas?.Clear();
            //ResultDatas?.Clear();

            //InitDatas = null;
            //ResultDatas = null;

            foreach (SKDataFigure<TData> figure in Figures)
            {
                figure.Dispose();
            }

            Figures.Clear();
        }

        public void UnSelect(SKDataFigure<TData> figure)
        {
            SelectedFigures.Remove(figure);

            figure.SetState(FigureVisualState.None);
        }

        public void UnSelectAll()
        {
            foreach (SKFigure f in SelectedFigures)
            {
                f.SetState(FigureVisualState.None);
            }

            SelectedFigures.Clear();
        }

        private void CheckSelected(SKDataFigure<TData> figure)
        {
            if (figure.CurrentState != FigureVisualState.Selected && figure.CurrentState != FigureVisualState.LongSelected)
            {
                return;
            }

            if (SelectedFiguresState != figure.CurrentState
                || (figure.CurrentState == FigureVisualState.Selected && !EnableMultipleSelected)
                || (figure.CurrentState == FigureVisualState.LongSelected && !EnableMultipleLongSelected))
            {
                UnSelectAllExcept(figure);
            }
            else
            {
                SelectedFigures.Add(figure);
            }

            void UnSelectAllExcept(SKDataFigure<TData> figure)
            {
                foreach (SKFigure sf in SelectedFigures)
                {
                    if (sf == figure)
                    {
                        continue;
                    }

                    sf.SetState(FigureVisualState.None);
                }

                SelectedFigures.Clear();
                SelectedFiguresState = figure.CurrentState;
                SelectedFigures.Add(figure);
            }
        }

        #region 事件派发

        private void OnPressed(object? sender, SKFigureTouchEventArgs info)
        {
            if (!HittingFigures.TryGetValue(info.FingerId, out SKDataFigure<TData>? figure))
            {
                return;
            }

            figure.OnPressed(info);

            //Bring To Frong
            if (AutoBringToFront && Figures.Remove(figure))
            {
                Figures.Add(figure);
            }
        }

        private void OnDragged(object? sender, SKFigureTouchEventArgs info)
        {
            if (!HittingFigures.TryGetValue(info.FingerId, out SKDataFigure<TData>? figure))
            {
                return;
            }

            figure.OnOneFingerDragged(info);

            CheckSelected(figure);

            if (info.IsOver)
            {
                HittingFigures.Remove(info.FingerId);
            }
        }

        private void OnLongTapped(object? sender, SKFigureTouchEventArgs info)
        {
            if (!HittingFigures.TryGetValue(info.FingerId, out SKDataFigure<TData>? figure))
            {
                return;
            }

            figure.OnLongTapped(info);

            CheckSelected(figure);

            if (info.IsOver)
            {
                HittingFigures.Remove(info.FingerId);
            }
        }

        private void OnTapped(object? sender, SKFigureTouchEventArgs info)
        {
            if (!HittingFigures.TryGetValue(info.FingerId, out SKDataFigure<TData>? figure))
            {
                return;
            }

            figure.OnTapped(info);

            CheckSelected(figure);

            if (info.IsOver)
            {
                HittingFigures.Remove(info.FingerId);
            }
        }

        private void OnCancelled(object? sender, SKFigureTouchEventArgs info)
        {
            if (!HittingFigures.TryGetValue(info.FingerId, out SKDataFigure<TData>? figure))
            {
                return;
            }

            figure.OnCancelled(info);

            //CheckSelected(figure);

            if (info.IsOver)
            {
                HittingFigures.Remove(info.FingerId);
            }
        }

        private void OnHitFailed(object? sender, EventArgs e)
        {
            HittingFigures.Clear();

            if (EnableUnSelectedByHitFailed)
            {
                UnSelectAll();
            }

            foreach (SKDataFigure<TData> figure in Figures)
            {
                figure.OnHitFailed();
            }
        }

        #endregion

        #region Dispose Pattern

        private bool _disposed;


        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!_disposed)
            {
                if (disposing)
                {
                    // managed
                    ClearFigures();
                }

                //unmanaged

                _disposed = true;
            }
        }

        #endregion
    }
}
