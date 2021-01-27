using HB.FullStack.Mobile;
using HB.FullStack.Mobile.Skia;

using SkiaSharp;
using SkiaSharp.Extended;
using SkiaSharp.Views.Forms;

namespace HB.FullStack.Mobile.Controls.Clock
{
    public class DialOuterRingFigure : SKFigure
    {
        private readonly float _innerRadius;
        private readonly float _outerRadius;

        private readonly SKPath _path;
        private readonly SKPaint _paint;

        public DialOuterRingFigure(int innerRadiusInDP, int outerRadiusInDP)
        {
            _innerRadius = (float)SKUtil.ToPx(innerRadiusInDP);
            _outerRadius = (float)SKUtil.ToPx(outerRadiusInDP);

            _path = SKGeometry.CreateSectorPath(0, 1, _outerRadius, _innerRadius);

            _paint = new SKPaint { Style = SKPaintStyle.Fill, Color = ColorUtil.RandomColor().Color.ToSKColor() };
        }

        protected override void OnDraw(SKImageInfo info, SKCanvas canvas)
        {
 

            canvas.Save();

            canvas.Translate(info.Width / 2f, info.Height / 2f);
            canvas.DrawPath(_path, _paint);

            canvas.Restore();
        }

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
                    _path?.Dispose();
                    _paint?.Dispose();
                }

                //unmanaged

                _disposed = true;
            }
        }

        protected override void OnUpdateHitTestPath(SKImageInfo info)
        {
            throw new System.NotImplementedException();
        }

        protected override void OnCaculateOutput()
        {
            throw new System.NotImplementedException();
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~DialOuterRingFigure ()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        #endregion Dispose Pattern
    }
}