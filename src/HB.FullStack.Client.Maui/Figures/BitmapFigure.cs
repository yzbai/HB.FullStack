using SkiaSharp;


using System.IO;

namespace HB.FullStack.Client.Maui.Figures
{

    /// <summary>
    /// 默认以NewCoordinateOriginalRatioPoint为DestRect中心
    /// </summary>
    public class BitmapFigure : SKFigure
    {
        private readonly float _widthRatio;
        private readonly float _heightRatio;

        private SKRect _destRect;
        private SKBitmap? _bitmap;
        private DrawBitmapResult? _drawBitmapResult;

        public SKStretchMode StretchMode { get; set; } = SKStretchMode.AspectFit;

        public SKAlignment HorizontalAlignment { get; set; } = SKAlignment.Center;

        public SKAlignment VerticalAlignment { get; set; } = SKAlignment.Center;

        public TouchManipulationMode ManipulationMode { get; set; } = TouchManipulationMode.IsotropicScale;

        public BitmapFigure(float widthRatio, float heightRatio, Stream? bitmapStream)
        {
            _widthRatio = widthRatio;
            _heightRatio = heightRatio;

            if (bitmapStream != null)
            {
                SetBitmap(bitmapStream);
            }

            OneFingerDragged += OnOneFingerDragged;
            TwoFingerDragged += OnTwoFingerDragged;
        }

        public void SetBitmap(Stream stream)
        {
            SKBitmap bitmap = SKBitmap.Decode(stream);

            SetBitmap(bitmap);
        }

        public void SetBitmap(SKBitmap bitmap)
        {
            _bitmap?.Dispose();

            _bitmap = bitmap;

            Reset();
        }

        public void Reset()
        {
            _rotatedDegrees = 0;

            Matrix = SKMatrix.CreateIdentity();

            CanvasView?.InvalidateSurface();
        }

        private float _rotatedDegrees;

        public void Rotate90(bool left)
        {
            float degree = left ? -90 : 90;
            _rotatedDegrees += degree;

            Matrix = Matrix.PostConcat(SKMatrix.CreateRotationDegrees(degree));

            CanvasView?.InvalidateSurface();
        }

        public SKBitmap Crop(SKRect cropRect)
        {
            SKMatrix invertedMatrix = Matrix.Invert();

            SKRect mappedCropRect = invertedMatrix.MapRect(cropRect);

            //由于任意旋转下，矩阵旋转后，不能再用SKRect表示，这里不用任意旋转

            float sourceX = (mappedCropRect.Left - _drawBitmapResult!.DisplayRect.Left) / _drawBitmapResult.WidthScale;
            float sourceY = (mappedCropRect.Top - _drawBitmapResult.DisplayRect.Top) / _drawBitmapResult.HeightScale;
            float sourceWidth = mappedCropRect.Width / _drawBitmapResult.WidthScale;
            float sourceHeight = mappedCropRect.Height / _drawBitmapResult.HeightScale;

            //得到原始图片上的原始区域
            SKRect sourceRect = SKRect.Create(sourceX, sourceY, sourceWidth, sourceHeight);

            //将SourceRect区域投射到新的canvas

            SKBitmap croppedBitmap = new SKBitmap((int)cropRect.Width, (int)cropRect.Height);

            using SKCanvas newCanvas = new SKCanvas(croppedBitmap);

            newCanvas.RotateDegrees(_rotatedDegrees, croppedBitmap.Width / 2f, croppedBitmap.Height / 2f);

            SKRect newDestRect = SKRect.Create(0, 0, croppedBitmap.Width, croppedBitmap.Height);

            newCanvas.DrawBitmap(_bitmap, sourceRect, newDestRect);

            return croppedBitmap;

            //SetBitmap(croppedBitmap);
        }

        protected override void OnDrawInfoIntialized()
        {
            //新坐标系下的。
            _destRect = SKRect.Create(
                _widthRatio * CanvasSize.Width / -2,
                _heightRatio * CanvasSize.Height / -2,
                _widthRatio * CanvasSize.Width,
                _heightRatio * CanvasSize.Height);
        }

        protected override void OnDraw(SKImageInfo info, SKCanvas canvas)
        {
            if (_bitmap != null)
            {
                SKRect mappedDestRect = Matrix.Invert().MapRect(_destRect);

                SKPaint paint = new SKPaint { IsStroke = true, StrokeWidth = 10, Color = SKColors.Red };

                canvas.DrawRect(mappedDestRect, paint);

                _drawBitmapResult = canvas.DrawBitmap(_bitmap, _destRect, StretchMode, HorizontalAlignment, VerticalAlignment);

                paint.Dispose();
            }
        }

        protected override SKPath CaculateHitTestPath(SKImageInfo info)
        {
            SKPath path = new SKPath();

            path.AddRect(_destRect);

            return path;
        }

        protected override void CaculateOutput()
        {

        }

        private void OnTwoFingerDragged(object? sender, SKFigureTouchEventArgs args)
        {
            SKPoint previousPivotedPoint = args.PreviousPoint;
            SKPoint currentPivotedPoint = args.CurrentPoint;
            SKPoint pivotPivotedPoint = args.PivotPoint;

            SKMatrix changedMatrix = SKUtil.CaculateTwoFingerDraggedMatrix(previousPivotedPoint, currentPivotedPoint, pivotPivotedPoint, ManipulationMode);

            Matrix = Matrix.PostConcat(changedMatrix);
        }

        private void OnOneFingerDragged(object? sender, SKFigureTouchEventArgs args)
        {
            SKPoint previousPivotedPoint = args.PreviousPoint;
            SKPoint currentPivotedPoint = args.CurrentPoint;
            SKPoint pivotPivotedPoint = Matrix.MapPoint(0, 0); //新坐标系下，图片的原点

            SKMatrix changedMatrix = SKUtil.CaculateOneFingerDraggedMatrix(previousPivotedPoint, currentPivotedPoint, pivotPivotedPoint, ManipulationMode);

            Matrix = Matrix.PostConcat(changedMatrix);
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
                    _bitmap?.Dispose();
                }

                //unmanaged

                _disposed = true;
            }
        }

        #endregion Dispose Pattern
    }
}