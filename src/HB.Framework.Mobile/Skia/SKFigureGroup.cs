using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.Client.Skia
{
    public class SKFigureGroup<T> : SKFigure where T : SKFigure
    {
        public bool AutoBringToFront { get; set; } = true;

        protected List<T> Figures { get; } = new List<T>();

        private readonly Dictionary<long, T> _hittedFigures = new Dictionary<long, T>();

        public SKFigureGroup()
        {
            Pressed += SKFigureGroup_Pressed;
            Tapped += SKFigureGroup_Tapped;
            LongTapped += SKFigureGroup_LongTapped;
            Dragged += SKFigureGroup_Dragged;
            Cancelled += SKFigureGroup_Cancelled;
            HitFailed += SKFigureGroup_HitFailed;
        }

        public override void Paint(SKPaintSurfaceEventArgs e)
        {
            SKCanvas canvas = e.Surface.Canvas;

            foreach (T figure in Figures)
            {
                using (new SKAutoCanvasRestore(canvas))
                {
                    figure.Paint(e);
                }
            }
        }

        public override bool HitTest(SKPoint skPoint, long touchId)
        {
            for (int i = Figures.Count - 1; i >= 0; i--)
            {
                T figure = Figures[i];

                if (figure.HitTest(skPoint, touchId))
                {
                    _hittedFigures[touchId] = figure;

                    return true;
                }
            }

            return false;
        }

        public void AddFigure(T figure)
        {
            figure.Parent = this;

            Figures.Add(figure);
        }

        public bool RemoveFigure(T figure)
        {
            if (figure is IDisposable disposable)
            {
                disposable.Dispose();
            }

            return Figures.Remove(figure);
        }

        public void Clear()
        {
            foreach (T figure in Figures)
            {
                if (figure is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            Figures.Clear();
        }

        #region 事件派发

        private void SKFigureGroup_Pressed(object sender, SKTouchInfo info)
        {
            //Bring to Front

            if (!AutoBringToFront)
            {
                return;
            }

            if (_hittedFigures.TryGetValue(info.TouchEventId, out T figure))
            {
                if (Figures.Remove(figure))
                {
                    Figures.Add(figure);
                }
            }
        }

        private void SKFigureGroup_Dragged(object sender, SKTouchInfo info)
        {
            if (_hittedFigures.TryGetValue(info.TouchEventId, out T figure))
            {
                figure.OnDragged(info);

                if (info.IsOver)
                {
                    _hittedFigures.Remove(info.TouchEventId);
                }
            }
        }

        private void SKFigureGroup_LongTapped(object sender, SKTouchInfo info)
        {
            if (_hittedFigures.TryGetValue(info.TouchEventId, out T figure))
            {
                figure.OnLongTapped(info);

                if (info.IsOver)
                {
                    _hittedFigures.Remove(info.TouchEventId);
                }
            }
        }

        private void SKFigureGroup_Tapped(object sender, SKTouchInfo info)
        {
            if (_hittedFigures.TryGetValue(info.TouchEventId, out T figure))
            {
                figure.OnTapped(info);

                if (info.IsOver)
                {
                    _hittedFigures.Remove(info.TouchEventId);
                }
            }
        }

        private void SKFigureGroup_Cancelled(object sender, SKTouchInfo info)
        {
            if (_hittedFigures.TryGetValue(info.TouchEventId, out T figure))
            {
                figure.OnCancelled(info);

                if (info.IsOver)
                {
                    _hittedFigures.Remove(info.TouchEventId);
                }
            }
        }

        private void SKFigureGroup_HitFailed(object sender, EventArgs e)
        {
            _hittedFigures.Clear();

            foreach (T figure in Figures)
            {
                figure.OnHitFailed();
            }
        }

        #endregion

        #region Dispose Pattern

        private bool _disposed;

        protected override void Dispose(bool disposeManaged)
        {
            base.Dispose(disposeManaged);

            if (!_disposed)
            {
                if (disposeManaged)
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
