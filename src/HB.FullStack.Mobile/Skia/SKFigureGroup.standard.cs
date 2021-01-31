using HB.FullStack.Mobile.Effects.Touch;

using SkiaSharp;
using SkiaSharp.Views.Forms;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HB.FullStack.Mobile.Skia
{
    public abstract class SKFigureGroup : SKFigure
    {
        public bool AutoBringToFront { get; set; } = true;

        public bool EnableMultipleSelected { get; set; }

        public bool EnableUnSelectedByHitFailed { get; set; } = true;
    }

    //EnableMultipleSelected
    public class SKFigureGroup<T> : SKFigureGroup where T : SKFigure
    {
        private readonly Dictionary<long, T> _hittingFigures = new Dictionary<long, T>();

        public IList<T> SelectedFigures { get; } = new List<T>();

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

        protected override void OnDraw(SKImageInfo info, SKCanvas canvas) { }

        public override bool OnHitTest(SKPoint skPoint, long touchId)
        {
            bool founded = false;

            for (int i = Figures.Count - 1; i >= 0; i--)
            {
                T figure = Figures[i];

                if (!founded && figure.OnHitTest(skPoint, touchId))
                {
                    founded = true;

                    _hittingFigures[touchId] = figure;
                }
                else
                {
                    TouchActionEventArgs unTouchArgs = new TouchActionEventArgs(touchId, TouchActionType.HitFailed, SKUtil.ToPoint(skPoint), true);
                    figure.ProcessTouchAction(unTouchArgs);
                }
            }

            return founded;
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

        private void Select(T figure)
        {
            if (!EnableMultipleSelected)
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
            }

            SelectedFigures.Add(figure);
        }

        #region 事件派发

        private void OnPressed(object? sender, SKTouchInfoEventArgs info)
        {
            if (!_hittingFigures.TryGetValue(info.TouchEventId, out T? figure))
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

        private void OnDragged(object? sender, SKTouchInfoEventArgs info)
        {
            if (!_hittingFigures.TryGetValue(info.TouchEventId, out T? figure))
            {
                return;
            }

            figure.OnDragged(info);

            Select(figure);

            if (info.IsOver)
            {
                _hittingFigures.Remove(info.TouchEventId);
            }
        }

        private void OnLongTapped(object? sender, SKTouchInfoEventArgs info)
        {
            if (!_hittingFigures.TryGetValue(info.TouchEventId, out T? figure))
            {
                return;
            }

            figure.OnLongTapped(info);

            Select(figure);

            if (info.IsOver)
            {
                _hittingFigures.Remove(info.TouchEventId);
            }
        }

        private void OnTapped(object? sender, SKTouchInfoEventArgs info)
        {
            if (!_hittingFigures.TryGetValue(info.TouchEventId, out T? figure))
            {
                return;
            }

            figure.OnTapped(info);

            Select(figure);

            if (info.IsOver)
            {
                _hittingFigures.Remove(info.TouchEventId);
            }
        }

        private void OnCancelled(object? sender, SKTouchInfoEventArgs info)
        {
            if (!_hittingFigures.TryGetValue(info.TouchEventId, out T? figure))
            {
                return;
            }

            figure.OnCancelled(info);

            if (info.IsOver)
            {
                _hittingFigures.Remove(info.TouchEventId);
            }
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
