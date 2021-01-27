using System;

using HB.FullStack.Mobile.Skia;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace HB.FullStack.Mobile.Controls.Clock
{
    /// <summary>
    /// 分针，时针，秒针
    /// </summary>
    public class DialHandFigure : SKFigure
    {
        private readonly SKRatioPoint _pivotPoint;
        private readonly float _hourHandLengthRatio;
        private readonly float _minuteHandLengthRatio;
        private readonly float _secondHandLengthRatio;

        public int HourResult { get; private set; }

        public int MinuteResult { get; private set; }

        public int SecondResult { get; private set; }

        public bool IsAM { get; private set; }

        public DialHandFigure(SKRatioPoint pivotPoint, float hourHandLengthRatio, float minuteHandLengthRatio, float secondHandLengthRatio)
        {
            _pivotPoint = pivotPoint;
            _hourHandLengthRatio = hourHandLengthRatio;
            _minuteHandLengthRatio = minuteHandLengthRatio;
            _secondHandLengthRatio = secondHandLengthRatio;
            
            EnableDrag = false;
            EnableTouch = false;
        }

        public override void OnPaint(SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            DateTimeOffset now = DateTimeOffset.UtcNow;

            HourResult = now.Hour;
            MinuteResult = now.Minute;
            SecondResult = now.Second;
            IsAM = now.Hour < 12;

            canvas.Translate(info.Width * _pivotPoint.XRatio, info.Height * _pivotPoint.YRatio);

            //Minute
            DrawMinuteHand(e);

            //Hour
            DrawHourHand(e);

            //Second
            DrawSecondHand(e);
        }

        private readonly SKPaint _secondPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 5,
            Color = SKColors.Red,
            IsAntialias = true
        };

        private readonly SKPaint _minutePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 10,
            Color = SKColors.Blue,
            IsAntialias = true
        };

        private readonly SKPaint _hourPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 15,
            Color = SKColors.Brown,
            IsAntialias = true
        };
        
        private void DrawSecondHand(SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            using (new SKAutoCanvasRestore(canvas))
            {
                float secondHandLength = Math.Min(info.Width, info.Height) * _secondHandLengthRatio;

                double radian = SKUtil.SecondToRadian(SecondResult);

                canvas.RotateRadians((float)radian);

                canvas.DrawLine(0, 0, 0, -secondHandLength, _secondPaint);
            }
        }

        private void DrawMinuteHand(SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            using (new SKAutoCanvasRestore(canvas))
            {
                //float minuteHandLength = GetFigureWidth(info.Size) * 0.35f;
                float minuteHandLength = Math.Min(info.Width, info.Height)* _minuteHandLengthRatio;

                double radian = SKUtil.MinuteToRadian(MinuteResult);

                canvas.RotateRadians((float)radian);

                canvas.DrawLine(0, 0, 0, -minuteHandLength, _minutePaint);
            }
        }

        private void DrawHourHand(SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            using (new SKAutoCanvasRestore(canvas))
            {
                //float hourHandLength = GetFigureWidth(info.Size) * 0.25f;
                float hourHandLength = Math.Min(info.Width, info.Height) * _hourHandLengthRatio;

                double radian = SKUtil.HourToRadian(HourResult, MinuteResult);

                canvas.RotateRadians((float)radian);

                canvas.DrawLine(0, 0, 0, -hourHandLength, _hourPaint);
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
                    _secondPaint?.Dispose();
                    _minutePaint?.Dispose();
                    _hourPaint?.Dispose();
                }

                //unmanaged

                _disposed = true;
            }
        }

        #endregion Dispose Pattern
    }
}