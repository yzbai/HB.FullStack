
using HB.FullStack.Common;

using SkiaSharp;
using SkiaSharp.Views.Forms;

using System;
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

    //EnableMultipleSelected
    public class SKFigureGroup<TFigure, TDrawData> : SKFigureGroup where TFigure : SKFigure<TDrawData>, new() where TDrawData : FigureDrawData
    {
        public static BindableProperty InitDrawDatasProperty = BindableProperty.Create(
               nameof(InitDrawDatas),
               typeof(IList<TDrawData>),
               typeof(SKFigureGroup<TFigure, TDrawData>),
               null,
               BindingMode.OneWay,
               propertyChanged: (b, oldValues, newValues) => ((SKFigureGroup<TFigure, TDrawData>)b).OnInitDrawDatasChanged((IList<TDrawData>?)oldValues, (IList<TDrawData>?)newValues));

        public static BindableProperty ResultDrawDatasProperty = BindableProperty.Create(
            nameof(ResultDrawDatas),
            typeof(IList<TDrawData>),
            typeof(SKFigureGroup<TFigure, TDrawData>),
            null,
            BindingMode.OneWayToSource);

        public IList<TDrawData>? InitDrawDatas { get => (IList<TDrawData>?)GetValue(InitDrawDatasProperty); set => SetValue(InitDrawDatasProperty, value); }
        public IList<TDrawData?>? ResultDrawDatas { get => (IList<TDrawData?>?)GetValue(ResultDrawDatasProperty); set => SetValue(ResultDrawDatasProperty, value); }
        private void OnInitDrawDatasChanged(IList<TDrawData>? oldValues, IList<TDrawData>? newValues)
        {
            //Create and Add Figures
            ResumeFigures();

            if (oldValues is ObservableCollection<TDrawData> oldCollection)
            {
                oldCollection.CollectionChanged -= OnInitDrawDatasCollectionChanged;
            }

            if (newValues is ObservableCollection<TDrawData> newCollection)
            {
                newCollection.CollectionChanged += OnInitDrawDatasCollectionChanged;
            }

            InvalidateMatrixAndSurface();
        }

        private void ResumeFigures()
        {
            ClearFigures();

            if (InitDrawDatas == null)
            {
                return;
            }

            ResultDrawDatas = new ObservableRangeCollection<TDrawData?>(Enumerable.Repeat<TDrawData?>(null, InitDrawDatas.Count));

            for (int i = 0; i < InitDrawDatas.Count; ++i)
            {
                TFigure figure = new TFigure();
                figure.SetBinding(SKFigure<TDrawData>.InitDrawDataProperty, new Binding($"{nameof(InitDrawDatas)}[{i}]", source: this));
                figure.SetBinding(SKFigure<TDrawData>.ResultDrawDataProperty, new Binding($"{nameof(ResultDrawDatas)}[{i}]", source: this));

                AddFigure(figure);
            }
        }

        private void OnInitDrawDatasCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ResumeFigures();

            InvalidateMatrixAndSurface();
        }

        private readonly Dictionary<long, TFigure> _hittingFigures = new Dictionary<long, TFigure>();

        public FigureState SelectedFiguresState { get; private set; }

        public IList<TFigure> SelectedFigures { get; } = new List<TFigure>();

        //TODO: make this obserable, and to notify repaint
        protected IList<TFigure> Figures { get; } = new List<TFigure>();

        public SKFigureGroup()
        {
            Pressed += OnPressed;
            Tapped += OnTapped;
            LongTapped += OnLongTapped;
            OneFingerDragged += OnDragged;
            Cancelled += OnCancelled;
            HitFailed += OnHitFailed;
        }

        public override void OnPaint(SKPaintSurfaceEventArgs e)
        {
            SKCanvas canvas = e.Surface.Canvas;

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

                    _hittingFigures[fingerId] = figure;
                }
                else
                {
                    figure.ProcessUnTouchAction(fingerId, location);
                }
            }

            return founded;
        }

        public void AddFigure(TFigure figure)
        {
            figure.Parent = this;
            figure.CanvasView = this.CanvasView;

            Figures.Add(figure);
        }

        public void AddFigures(params TFigure[] figures)
        {
            foreach (TFigure f in figures)
            {
                f.Parent = this;
                f.CanvasView = this.CanvasView;
            }

            Figures.AddRange(figures);
        }

        public bool RemoveFigure(TFigure figure)
        {
            figure.Dispose();

            _hittingFigures
                .Where(p => p.Value == figure)
                .ToList()
                .ForEach(p => _hittingFigures.Remove(p.Key));

            SelectedFigures.Remove(figure);

            return Figures.Remove(figure);
        }

        public void ClearFigures()
        {
            _hittingFigures.Clear();
            SelectedFigures.Clear();

            foreach (TFigure figure in Figures)
            {
                figure.Dispose();
            }

            Figures.Clear();
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
            if (!_hittingFigures.TryGetValue(info.FingerId, out TFigure? figure))
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
            if (!_hittingFigures.TryGetValue(info.FingerId, out TFigure? figure))
            {
                return;
            }

            figure.OnOneFingerDragged(info);

            CheckSelected(figure);

            if (info.IsOver)
            {
                _hittingFigures.Remove(info.FingerId);
            }
        }

        private void OnLongTapped(object? sender, SKFigureTouchEventArgs info)
        {
            if (!_hittingFigures.TryGetValue(info.FingerId, out TFigure? figure))
            {
                return;
            }

            figure.OnLongTapped(info);

            CheckSelected(figure);

            if (info.IsOver)
            {
                _hittingFigures.Remove(info.FingerId);
            }
        }

        private void OnTapped(object? sender, SKFigureTouchEventArgs info)
        {
            if (!_hittingFigures.TryGetValue(info.FingerId, out TFigure? figure))
            {
                return;
            }

            figure.OnTapped(info);

            CheckSelected(figure);

            if (info.IsOver)
            {
                _hittingFigures.Remove(info.FingerId);
            }
        }

        private void OnCancelled(object? sender, SKFigureTouchEventArgs info)
        {
            if (!_hittingFigures.TryGetValue(info.FingerId, out TFigure? figure))
            {
                return;
            }

            figure.OnCancelled(info);

            //CheckSelected(figure);

            if (info.IsOver)
            {
                _hittingFigures.Remove(info.FingerId);
            }
        }

        private void OnHitFailed(object? sender, EventArgs e)
        {
            _hittingFigures.Clear();

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
