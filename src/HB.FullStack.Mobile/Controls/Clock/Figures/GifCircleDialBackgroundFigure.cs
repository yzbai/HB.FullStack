using System;

using HB.FullStack.Mobile.Skia;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace HB.FullStack.Mobile.Controls.Clock
{
    /// <summary>
    /// 圆形表盘背景
    /// </summary>
    public class GifCircleDialBackgroundFigure : SKFigure
    {
        private readonly SKGif _backgroundGif;

        private readonly SKPaint _paint = new SKPaint();

        private SKMatrix _shaderTransformMatrix = SKMatrix.Empty;

        private SKSizeI _previousSize = SKSizeI.Empty;

        private float _previousRadius;

        public GifCircleDialBackgroundFigure(float widthRatio, float heightRatio, SKAlignment horizontalAlignment, SKAlignment verticalAlignment, string gifResourceName)
            : base(widthRatio: widthRatio, heightRatio: heightRatio, horizontalAlignment: horizontalAlignment, verticalAlignment: verticalAlignment)
        {
            _backgroundGif = new SKGif(gifResourceName);
        }

        public override void Paint(SKPaintSurfaceEventArgs e)
        {
            if (_backgroundGif == null || !_backgroundGif.IsReady)
            {
                return;
            }

            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            SKSize figureSize = GetFigureSize(info.Size);
            SKBitmap bitmap = _backgroundGif.GetBitmap(CanvasView!.ElapsedMilliseconds);

            if (_previousSize != info.Size)
            {
                _previousRadius = Math.Min(figureSize.Height, figureSize.Width) / 2f;

                _shaderTransformMatrix = GetFilledMatrix(info.Size, new SKSize(bitmap.Width, bitmap.Height));

                _previousSize = info.Size;
            }

            _paint.Shader?.Dispose();
            _paint.Shader = SKShader.CreateBitmap(bitmap, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, _shaderTransformMatrix);

            //默认居中
            //SKPoint center = new SKPoint(info.Width / 2f, info.Height / 2f);
            canvas.DrawCircle(GetFigureCenter(info.Size), _previousRadius, _paint);
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
                    //managed
                    _backgroundGif?.Dispose();
                    _paint?.Dispose();
                }
                else
                {
                    //native
                }

                _disposed = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~DialBackgroundFigure ()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        #endregion Dispose Pattern
    }
}