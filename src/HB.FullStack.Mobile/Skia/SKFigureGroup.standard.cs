using SkiaSharp;
using SkiaSharp.Views.Forms;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HB.FullStack.Mobile.Skia
{
    //EnableMultipleSelected
    public class SKFigureGroup<T> : SKFigure where T : SKFigure
    {
        private readonly Dictionary<long, T> _hittingFigures = new Dictionary<long, T>();

        public IList<T> SelectedFigures { get; } = new List<T>();

        public bool AutoBringToFront { get; set; } = true;

        public bool EnableMultipleSelected { get; set; }

        public bool EnableUnSelectedByHitFailed { get; set; }

        //TODO: make this obserable, and to notify repaint
        protected IList<T> Figures { get; } = new List<T>();

        public SKFigureGroup()
        {
            Pressed += OnPressed;
            Tapped += OnTapped;
            LongTapped += OnLongTapped;
            Dragged += OnDragged;
            Cancelled += OnCancelled;
            HitFailed += OnHitFailed;
        }

        public override void OnPaint(SKPaintSurfaceEventArgs e)
        {
            SKCanvas canvas = e.Surface.Canvas;

            foreach (T figure in Figures)
            {
                using (new SKAutoCanvasRestore(canvas))
                {
                    figure.OnPaint(e);
                }
            }
        }
        public override bool OnHitTest(SKPoint skPoint, long touchId)
        {
            for (int i = Figures.Count - 1; i >= 0; i--)
            {
                T figure = Figures[i];

                if (figure.OnHitTest(skPoint, touchId))
                {
                    _hittingFigures[touchId] = figure;

                    return true;
                }
            }

            return false;
        }

        public void AddFigure(T figure)
        {
            figure.Parent = this;
            figure.CanvasView = this.CanvasView;

            Figures.Add(figure);
        }

        public void AddFigures(params T[] figures)
        {
            foreach (T f in figures)
            {
                f.Parent = this;
                f.CanvasView = this.CanvasView;
            }

            Figures.AddRange(figures);
        }

        public bool RemoveFigure(T figure)
        {
            figure.Dispose();

            _hittingFigures
                .Where(p => p.Value == figure)
                .ToList()
                .ForEach(p => _hittingFigures.Remove(p.Key));

            SelectedFigures.Remove(figure);

            return Figures.Remove(figure);
        }

        public void Clear()
        {
            _hittingFigures.Clear();
            SelectedFigures.Clear();

            foreach (T figure in Figures)
            {
                figure.Dispose();
            }

            Figures.Clear();
        }

        public void UnSelect(T figure)
        {
            SelectedFigures.Remove(figure);

            if (figure is IStatedFigure statedFigure)
            {
                statedFigure.SetState(FigureState.UnSelected);
            }
        }

        public void Select(T figure, string eventName)
        {
            if (eventName == nameof(OnCancelled))
            {
                return;
            }

            if (!EnableMultipleSelected)
            {
                foreach (SKFigure f in SelectedFigures)
                {
                    if (f == figure)
                    {
                        continue;
                    }

                    if (f is IStatedFigure statedFigure1)
                    {
                        statedFigure1.SetState(FigureState.UnSelected);
                    }
                }

                SelectedFigures.Clear();
            }

            SelectedFigures.Add(figure);

            if (figure is IStatedFigure statedFigure2)
            {
                FigureState figureState = eventName switch
                {
                    nameof(OnTapped) => FigureState.Tapped,
                    nameof(OnLongTapped) => FigureState.LongTapped,
                    nameof(OnDragged) => FigureState.Dragged,
                    _ => FigureState.None
                };

                statedFigure2.SetState(figureState);
            }
        }

        public void UnSelectAll()
        {
            foreach (SKFigure f in SelectedFigures)
            {
                if (f is IStatedFigure statedFigure)
                {
                    statedFigure.SetState(FigureState.UnSelected);
                }
            }

            SelectedFigures.Clear();
        }

        #region 事件派发

        private void OnTouchIsOver(SKTouchInfoEventArgs info, T figure, string eventName)
        {
            if (info.IsOver)
            {
                _hittingFigures.Remove(info.TouchEventId);
            }

            //Selected
            if (info.IsOver || info.LongPressHappend)
            {
                Select(figure, eventName);
            }
        }

        private void OnPressed(object? sender, SKTouchInfoEventArgs info)
        {
            if (!_hittingFigures.TryGetValue(info.TouchEventId, out T? figure))
            {
                return;
            }

            //Bring To Frong
            if (AutoBringToFront && Figures.Remove(figure))
            {
                Figures.Add(figure);
            }
        }

        private void OnDragged(object? sender, SKTouchInfoEventArgs info)
        {
            if (!_hittingFigures.TryGetValue(info.TouchEventId, out T? figure))
            {
                return;
            }

            figure.OnDragged(info);

            OnTouchIsOver(info, figure, nameof(OnDragged));
        }

        private void OnLongTapped(object? sender, SKTouchInfoEventArgs info)
        {
            if (!_hittingFigures.TryGetValue(info.TouchEventId, out T? figure))
            {
                return;
            }

            figure.OnLongTapped(info);

            OnTouchIsOver(info, figure, nameof(OnLongTapped));
        }

        private void OnTapped(object? sender, SKTouchInfoEventArgs info)
        {
            if (!_hittingFigures.TryGetValue(info.TouchEventId, out T? figure))
            {
                return;
            }

            figure.OnTapped(info);

            OnTouchIsOver(info, figure, nameof(OnTapped));
        }

        private void OnCancelled(object? sender, SKTouchInfoEventArgs info)
        {
            if (!_hittingFigures.TryGetValue(info.TouchEventId, out T? figure))
            {
                return;
            }

            figure.OnCancelled(info);

            OnTouchIsOver(info, figure, nameof(OnCancelled));
        }

        private void OnHitFailed(object? sender, EventArgs e)
        {
            _hittingFigures.Clear();

            if (EnableUnSelectedByHitFailed)
            {
                UnSelectAll();
            }

            foreach (T figure in Figures)
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
                    Clear();
                }

                //unmanaged

                _disposed = true;
            }
        }

        #endregion
    }
}
