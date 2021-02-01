using HB.FullStack.Mobile.Skia;

using SkiaSharp;

using System;

namespace HB.FullStack.Mobile.Controls.Cropper
{
    public class CropperFrameFigure : SKFigure
    {
        private const double CORNER_LENGTH_DP = 10;
        private const double CORNER_TOUCH_RADIUS_DP = 20;
        private const float CROP_RECT_MINIMUM_LENGTH_DP = 40;

        private readonly float _initCropperWidthRatio;
        private readonly float _initCropperHeightRatio;
        private readonly float _outterWidthRatio;
        private readonly float _outterHeightRatio;

        private readonly SKPaint _outterRectPaint;
        private readonly SKPaint _cropperRectPaint;
        private readonly SKPaint _cornerPaint;
        private readonly float _cornerLength;
        private readonly float _cornerTouchRadius;
        private readonly float _cropRectMinimumLength;

        private bool _firstDraw = true;
        private CornerType _hittedCorner = CornerType.None;
        private SKRect _cropRect;
        private SKRect _outterRect;

        public float Transparency { get; set; } = 0.2f;

        public SKRect CropRect => _cropRect;

        public bool IsSquare { get; set; } = true;

        /// <summary>
        /// CropperFrameFigure
        /// </summary>
        /// <param name="initCroperWidthRatio">初始Crop框的比例</param>
        /// <param name="initCropperHeightRatio">初始Crop框的比例</param>
        /// <param name="outterWidthRatio">最大外围</param>
        /// <param name="outterHeightRatio">最大外围</param>
        public CropperFrameFigure(float initCroperWidthRatio, float initCropperHeightRatio, float outterWidthRatio, float outterHeightRatio)
        {
            _cornerLength = (float)SKUtil.ToPx(CORNER_LENGTH_DP);
            _cornerTouchRadius = (float)SKUtil.ToPx(CORNER_TOUCH_RADIUS_DP);
            _cropRectMinimumLength = (float)SKUtil.ToPx(CROP_RECT_MINIMUM_LENGTH_DP);

            _initCropperWidthRatio = initCroperWidthRatio;
            _initCropperHeightRatio = initCropperHeightRatio;
            _outterWidthRatio = outterWidthRatio;
            _outterHeightRatio = outterHeightRatio;

            _outterRectPaint = new SKPaint { Style = SKPaintStyle.Fill, Color = SKColors.DimGray.WithAlpha((byte)(0xFF * (1 - Transparency))) };
            _cropperRectPaint = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 5, Color = SKColors.White };
            _cornerPaint = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 15, Color = SKColors.White };

            OneFingerDragged += CropperFrameFigure_OneFingerDragged;
        }

        public void Reset()
        {
            _firstDraw = true;

            CanvasView?.InvalidateSurface();
        }

        protected override void OnDraw(SKImageInfo info, SKCanvas canvas)
        {
            if (CanvasSizeChanged)
            {
                _outterRect = SKRect.Create(
                    _outterWidthRatio * info.Width / -2,
                    _outterHeightRatio * info.Height / -2,
                    _outterWidthRatio * info.Width,
                    _outterHeightRatio * info.Height);
            }

            if (_firstDraw)
            {
                if (IsSquare)
                {
                    float cropRectLength = Math.Min(_initCropperWidthRatio * info.Width, _initCropperHeightRatio * info.Height);

                    _cropRect = SKRect.Create(cropRectLength / -2, cropRectLength / -2, cropRectLength, cropRectLength);
                }
                else
                {
                    _cropRect = SKRect.Create(
                        _initCropperWidthRatio * info.Width / -2,
                        _initCropperHeightRatio * info.Height / -2,
                        _initCropperWidthRatio * info.Width,
                        _initCropperHeightRatio * info.Height);
                }
                _firstDraw = false;
            }


            //Draw Dim Bg
            using SKRegion bgRegion = new SKRegion(SKRectI.Round(_outterRect));

            bgRegion.Op(SKRectI.Round(_cropRect), SKRegionOperation.Difference);

            canvas.DrawRegion(bgRegion, _outterRectPaint);

            //Draw CropRect
            canvas.DrawRect(_cropRect, _cropperRectPaint);

            //Draw Corner

            using SKPath cornerPath = new SKPath();

            //左上角
            cornerPath.MoveTo(_cropRect.Left + _cornerLength, _cropRect.Top);
            cornerPath.LineTo(_cropRect.Left, _cropRect.Top);
            cornerPath.LineTo(_cropRect.Left, _cropRect.Top + _cornerLength);

            //右上角
            cornerPath.MoveTo(_cropRect.Right - _cornerLength, _cropRect.Top);
            cornerPath.LineTo(_cropRect.Right, _cropRect.Top);
            cornerPath.LineTo(_cropRect.Right, _cropRect.Top + _cornerLength);

            //左下角
            cornerPath.MoveTo(_cropRect.Left + _cornerLength, _cropRect.Bottom);
            cornerPath.LineTo(_cropRect.Left, _cropRect.Bottom);
            cornerPath.LineTo(_cropRect.Left, _cropRect.Bottom - _cornerLength);

            //右下角
            cornerPath.MoveTo(_cropRect.Right - _cornerLength, _cropRect.Bottom);
            cornerPath.LineTo(_cropRect.Right, _cropRect.Bottom);
            cornerPath.LineTo(_cropRect.Right, _cropRect.Bottom - _cornerLength);

            canvas.DrawPath(cornerPath, _cornerPaint);
        }

        public override bool OnHitTest(SKPoint skPoint, long touchId)
        {
            SKPoint hitPoint = GetNewCoordinatedPoint(skPoint);

            //左上角
            SKRect rect = SKRect.Create(_cropRect.Left - _cornerTouchRadius, _cropRect.Top - _cornerTouchRadius, _cornerTouchRadius * 2, _cornerTouchRadius * 2);

            if (rect.Contains(hitPoint))
            {
                _hittedCorner = CornerType.LeftTop;
                return true;
            }

            //右上角
            rect = SKRect.Create(_cropRect.Right - _cornerTouchRadius, _cropRect.Top - _cornerTouchRadius, _cornerTouchRadius * 2, _cornerTouchRadius * 2);

            if (rect.Contains(hitPoint))
            {
                _hittedCorner = CornerType.RightTop;
                return true;
            }

            //左下角
            rect = SKRect.Create(_cropRect.Left - _cornerTouchRadius, _cropRect.Bottom - _cornerTouchRadius, _cornerTouchRadius * 2, _cornerTouchRadius * 2);

            if (rect.Contains(hitPoint))
            {
                _hittedCorner = CornerType.LeftBottom;
                return true;
            }

            //右下角
            rect = SKRect.Create(_cropRect.Right - _cornerTouchRadius, _cropRect.Bottom - _cornerTouchRadius, _cornerTouchRadius * 2, _cornerTouchRadius * 2);

            if (rect.Contains(hitPoint))
            {
                _hittedCorner = CornerType.RightBottom;
                return true;
            }

            _hittedCorner = CornerType.None;
            return false;
        }

        private void CropperFrameFigure_OneFingerDragged(object sender, SKFigureTouchEventArgs e)
        {
            float xOffset = e.CurrentPoint.X - e.PreviousPoint.X;
            float yOffset = e.CurrentPoint.Y - e.PreviousPoint.Y;

            if (IsSquare)
            {
                if (xOffset == 0 || yOffset == 0)
                {
                    //TODO: 解决其中一方不移动
                    //BUG: 会移动成非正方形
                    return;
                }
            }

            //不能超出边界
            //不能太小
            switch (_hittedCorner)
            {
                case CornerType.RightBottom:
                    {
                        float newRight = _cropRect.Right + xOffset;
                        float newBottom = _cropRect.Bottom + yOffset;

                        newRight = Math.Max(Math.Min(newRight, _outterRect.Right), _cropRect.Left + _cropRectMinimumLength);
                        newBottom = Math.Max(Math.Min(newBottom, _outterRect.Bottom), _cropRect.Top + _cropRectMinimumLength);

                        if (IsSquare)
                        {
                            float offset = Math.Min(Math.Abs(newRight - _cropRect.Right), Math.Abs(newBottom - _cropRect.Bottom));
                            _cropRect.Right = _cropRect.Right + offset * Math.Sign(xOffset);
                            _cropRect.Bottom = _cropRect.Bottom + offset * Math.Sign(yOffset);
                        }
                        else
                        {
                            _cropRect.Right = newRight;
                            _cropRect.Bottom = newBottom;
                        }

                        break;
                    }
                case CornerType.LeftBottom:
                    {
                        float newLeft = _cropRect.Left + xOffset;
                        float newBottom = _cropRect.Bottom + yOffset;

                        newLeft = Math.Min(Math.Max(newLeft, _outterRect.Left), _cropRect.Right - _cropRectMinimumLength);
                        newBottom = Math.Max(Math.Min(newBottom, _outterRect.Bottom), _cropRect.Top + _cropRectMinimumLength);

                        if (IsSquare)
                        {
                            float offset = Math.Min(Math.Abs(newLeft - _cropRect.Left), Math.Abs(newBottom - _cropRect.Bottom));
                            _cropRect.Left = _cropRect.Left + offset * Math.Sign(xOffset);
                            _cropRect.Bottom = _cropRect.Bottom + offset * Math.Sign(yOffset);
                        }
                        else
                        {
                            _cropRect.Left = newLeft;
                            _cropRect.Bottom = newBottom;
                        }
                        break;
                    }
                case CornerType.LeftTop:
                    {
                        float newLeft = _cropRect.Left + xOffset;
                        float newTop = _cropRect.Top + yOffset;

                        newLeft = Math.Min(Math.Max(newLeft, _outterRect.Left), _cropRect.Right - _cropRectMinimumLength);
                        newTop = Math.Min(Math.Max(newTop, _outterRect.Top), _cropRect.Bottom - _cropRectMinimumLength);

                        if (IsSquare)
                        {
                            float offset = Math.Min(Math.Abs(newLeft - _cropRect.Left), Math.Abs(newTop - _cropRect.Top));
                            _cropRect.Left = _cropRect.Left + offset * Math.Sign(xOffset);
                            _cropRect.Top = _cropRect.Top + offset * Math.Sign(yOffset);
                        }
                        else
                        {
                            _cropRect.Left = newLeft;
                            _cropRect.Top = newTop;
                        }
                        break;
                    }
                case CornerType.RightTop:
                    {
                        float newRight = _cropRect.Right + xOffset;
                        float newTop = _cropRect.Top + yOffset;

                        newRight = Math.Max(Math.Min(newRight, _outterRect.Right), _cropRect.Left + _cropRectMinimumLength);
                        newTop = Math.Min(Math.Max(newTop, _outterRect.Top), _cropRect.Bottom - _cropRectMinimumLength);

                        if (IsSquare)
                        {
                            float offset = Math.Min(Math.Abs(newRight - _cropRect.Right), Math.Abs(newTop - _cropRect.Top));
                            _cropRect.Right = _cropRect.Right + offset * Math.Sign(xOffset);
                            _cropRect.Top = _cropRect.Top + offset * Math.Sign(yOffset);
                        }
                        else
                        {
                            _cropRect.Right = newRight;
                            _cropRect.Top = newTop;
                        }

                        break;
                    }
                case CornerType.None:
                    break;
            }
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
                    _cropperRectPaint.Dispose();
                    _outterRectPaint.Dispose();
                    _cornerPaint.Dispose();
                }

                //unmanaged

                _disposed = true;
            }
        }

        #endregion Dispose Pattern
    }
}