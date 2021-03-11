
using HB.FullStack.Common;

using SkiaSharp;
using SkiaSharp.Views.Forms;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace HB.FullStack.XamarinForms.Skia
{
    public abstract class SKFigureGroup : SKFigure
    {
        public bool AutoBringToFront { get; set; } = true;

        public bool EnableMultipleSelected { get; set; }

        public bool EnableMultipleLongSelected { get; set; }

        public bool EnableUnSelectedByHitFailed { get; set; } = true;
    }

    public abstract class SKFigureGroup<TFigure, TDrawInfo> : SKFigureGroup<TFigure, TDrawInfo, EmptyData>
    where TFigure : SKFigure<TDrawInfo>, new()
    where TDrawInfo : FigureData
    {

    }


    public abstract class SKFigureGroup<TFigure, TDrawInfo, TData> : SKFigureGroup
        where TFigure : SKFigure<TDrawInfo, TData>, new()
        where TDrawInfo : FigureData
        where TData : FigureData
    {
        public static BindableProperty DrawDataProperty = BindableProperty.Create(
               nameof(DrawData),
               typeof(TDrawInfo),
               typeof(SKFigureGroup<TFigure, TDrawInfo, TData>),
               null,
               BindingMode.OneWay,
               propertyChanged: (b, oldValues, newValues) => ((SKFigureGroup<TFigure, TDrawInfo, TData>)b).OnDrawDatasChanged());

        public static BindableProperty InitDatasProperty = BindableProperty.Create(
            nameof(InitDatas),
            typeof(IList<TData>),
            typeof(SKFigureGroup<TFigure, TDrawInfo, TData>),
            null,
            BindingMode.OneWay,
            propertyChanged: (b, oldValues, newValues) => ((SKFigureGroup<TFigure, TDrawInfo, TData>)b).OnInitDatasChanged((IList<TData>?)oldValues, (IList<TData>?)newValues));



        public static BindableProperty ResultDatasProperty = BindableProperty.Create(
            nameof(ResultDatas),
            typeof(IList<TData>),
            typeof(SKFigureGroup<TFigure, TDrawInfo, TData>),
            null,
            BindingMode.OneWayToSource);

        public TDrawInfo? DrawData { get => (TDrawInfo?)GetValue(DrawDataProperty); set => SetValue(DrawDataProperty, value); }

        public IList<TData?>? InitDatas { get => (IList<TData?>)GetValue(InitDatasProperty); set => SetValue(InitDatasProperty, value); }

        public IList<TData?>? ResultDatas { get => (IList<TData?>)GetValue(ResultDatasProperty); set => SetValue(ResultDatasProperty, value); }

        protected SKFigureGroup()
        {
            Pressed += OnPressed;
            Tapped += OnTapped;
            LongTapped += OnLongTapped;
            OneFingerDragged += OnDragged;
            Cancelled += OnCancelled;
            HitFailed += OnHitFailed;
        }

        private void OnDrawDatasChanged()
        {
            InvalidateMatrixAndSurface();
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
                TFigure figure = new TFigure();
                figure.Parent = this;
                figure.CanvasView = this.CanvasView;

                figure.SetBinding(SKFigure<TDrawInfo, TData>.DrawInfoProperty, new Binding(nameof(DrawData), source: this));
                figure.SetBinding(SKFigure<TDrawInfo, TData>.InitDataProperty, new Binding($"{nameof(InitDatas)}[{i}]", source: this));
                figure.SetBinding(SKFigure<TDrawInfo, TData>.ResultDataProperty, new Binding($"{nameof(ResultDatas)}[{i}]", source: this));

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

            foreach (TFigure figure in Figures)
            {
                figure.Dispose();
            }

            Figures.Clear();
        }

        private IList<TFigure> Figures { get; } = new List<TFigure>();

        protected Dictionary<long, TFigure> HittingFigures { get; } = new Dictionary<long, TFigure>();

        public IList<TFigure> SelectedFigures { get; } = new List<TFigure>();

        public FigureState SelectedFiguresState { get; private set; }

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
            }
            else
            {
                CanvasSizeChanged = false;
            }

            foreach (TFigure figure in Figures)
            {
                using (new SKAutoCanvasRestore(canvas))
                {
                    figure.OnPaint(e);
                }
            }
        }

        public override bool OnHitTest(SKPoint location, long fingerId)
        {
            bool founded = false;

            for (int i = Figures.Count - 1; i >= 0; i--)
            {
                TFigure figure = Figures[i];

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

        public void UnSelect(TFigure figure)
        {
            SelectedFigures.Remove(figure);

            figure.SetState(FigureState.None);
        }

        public void UnSelectAll()
        {
            foreach (SKFigure f in SelectedFigures)
            {
                f.SetState(FigureState.None);
            }

            SelectedFigures.Clear();
        }

        private void CheckSelected(TFigure figure)
        {
            if (figure.State != FigureState.Selected && figure.State != FigureState.LongSelected)
            {
                return;
            }

            if (SelectedFiguresState != figure.State
                || (figure.State == FigureState.Selected && !EnableMultipleSelected)
                || (figure.State == FigureState.LongSelected && !EnableMultipleLongSelected))
            {
                UnSelectAllExcept(figure);
            }
            else
            {
                SelectedFigures.Add(figure);
            }

            void UnSelectAllExcept(TFigure figure)
            {
                foreach (SKFigure sf in SelectedFigures)
                {
                    if (sf == figure)
                    {
                        continue;
                    }

                    sf.SetState(FigureState.None);
                }

                SelectedFigures.Clear();
                SelectedFiguresState = figure.State;
                SelectedFigures.Add(figure);
            }
        }

        #region 事件派发

        private void OnPressed(object? sender, SKFigureTouchEventArgs info)
        {
            if (!HittingFigures.TryGetValue(info.FingerId, out TFigure? figure))
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
            if (!HittingFigures.TryGetValue(info.FingerId, out TFigure? figure))
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
            if (!HittingFigures.TryGetValue(info.FingerId, out TFigure? figure))
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
            if (!HittingFigures.TryGetValue(info.FingerId, out TFigure? figure))
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
            if (!HittingFigures.TryGetValue(info.FingerId, out TFigure? figure))
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

            foreach (TFigure figure in Figures)
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
