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
        private readonly SKRatioPoint pivotPoint;
        private readonly float widthRatio;
        private readonly float heightRatio;
        private readonly string gifResourceName;
        private SKMatrix _shaderTransformMatrix = SKMatrix.Empty;

        private SKSizeI _previousSize = SKSizeI.Empty;

        private float _previousRadius;

        public GifCircleDialBackgroundFigure(SKRatioPoint pivotPoint, float widthRatio, float heightRatio, string gifResourceName)
        {
            _backgroundGif = new SKGif(gifResourceName);
            this.pivotPoint = pivotPoint;
            this.widthRatio = widthRatio;
            this.heightRatio = heightRatio;
            this.gifResourceName = gifResourceName;
        }

        protected override void OnDraw(SKImageInfo info, SKCanvas canvas)
        {
            if (_backgroundGif == null || !_backgroundGif.IsReady)
            {
                return;
            }

            SKSize figureSize = new SKSize(info.Width * widthRatio, info.Height * heightRatio);
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
            SKPoint center = new SKPoint(info.Width * pivotPoint.XRatio, info.Height * pivotPoint.YRatio);
            canvas.DrawCircle(center, _previousRadius, _paint);
        }

        /// <summary>
        /// 获取sourceSize大小的bitmap填充Figure的变换矩阵
        /// </summary>
        /// <param name="canvasSize"></param>
        /// <param name="sourceSize"></param>
        /// <param name="stretch"></param>
        /// <returns></returns>
        public SKMatrix GetFilledMatrix(SKSize canvasSize, SKSize sourceSize, SKStretch stretch = SKStretch.AspectFill)
        {
            SKMatrix transToCenterMatrix = GetTransToFigureCenterMatrix(canvasSize, sourceSize);

            float figureWidth = canvasSize.Width * widthRatio;
            float figureHeight = canvasSize.Height * heightRatio;
            float widthScale = figureWidth / sourceSize.Width;
            float heightScale = figureHeight / sourceSize.Height;
            float maxScale = Math.Max(heightScale, widthScale);
            float minScale = Math.Min(heightScale, widthScale);

            SKPoint sourceCenter = new SKPoint(sourceSize.Width / 2f, sourceSize.Height / 2f);

            SKMatrix scaleMatrix = stretch switch
            {
                SKStretch.AspectFill => SKMatrix.CreateScale(maxScale, maxScale, sourceCenter.X, sourceCenter.Y),
                SKStretch.AspectFit => SKMatrix.CreateScale(minScale, minScale, sourceCenter.X, sourceCenter.Y),
                SKStretch.Fill => SKMatrix.CreateScale(widthScale, heightScale, sourceCenter.X, sourceCenter.Y),
                _ => SKMatrix.Identity
            };

            return SKMatrix.Concat(transToCenterMatrix, scaleMatrix);

        }

        public SKMatrix GetTransToFigureCenterMatrix(SKSize canvasSize, SKSize sourceSize)
        {
            SKPoint figureCenter = new SKPoint(canvasSize.Width * pivotPoint.XRatio, canvasSize.Height * pivotPoint.YRatio);

            SKPoint sourceCenter = new SKPoint(sourceSize.Width / 2f, sourceSize.Height / 2f);

            return SKMatrix.CreateTranslation(figureCenter.X - sourceCenter.X, figureCenter.Y - sourceCenter.Y);
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

        protected override void OnUpdateHitTestPath(SKImageInfo info)
        {
            throw new NotImplementedException();
        }

        protected override void OnCaculateOutput()
        {
            throw new NotImplementedException();
        }

        protected override void CaculateMatrixByTime(long elapsedMilliseconds)
        {
            throw new NotImplementedException();
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