using System;

using HB.FullStack.Mobile;
using HB.FullStack.Mobile.Skia;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace HB.FullStack.Mobile.Controls.Clock
{
    public class TicksFigure : SKFigure
    {
        private readonly SKPaint? _hourMarkPaint;
        private readonly SKPaint? _minuteMarkPaint;
        private readonly SKPaint? _hourNumPaint;
        private readonly SKPaint? _minuteNumPaint;
        private readonly SKRatioPoint pivotPoint;
        private readonly float handLengthRatio;
        private SKSizeI _previousCanvasSize = SKSizeI.Empty;

        private float _radius;

        private float _singleHourNumWidth;
        private float _singleMinuteNumWidth;

        public TicksFigure(SKRatioPoint pivotPoint, float handLengthRatio)
        {
            _hourMarkPaint = new SKPaint { Color = ColorUtil.RandomColor().Color.ToSKColor(), Style = SKPaintStyle.Fill };
            _minuteMarkPaint = new SKPaint { Color = ColorUtil.RandomColor().Color.ToSKColor(), Style = SKPaintStyle.Fill };

            _hourNumPaint = new SKPaint { Color = ColorUtil.RandomColor().Color.ToSKColor(), Style = SKPaintStyle.Fill };
            _minuteNumPaint = new SKPaint { Color = ColorUtil.RandomColor().Color.ToSKColor(), Style = SKPaintStyle.Fill };
            this.pivotPoint = pivotPoint;
            this.handLengthRatio = handLengthRatio;
        }

        public override void Paint(SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            if (_previousCanvasSize != info.Size)
            {
                _radius = Math.Min(info.Height * handLengthRatio, info.Width * handLengthRatio) / 2f;

                _singleHourNumWidth = _radius / 11;
                _singleMinuteNumWidth = _radius / 15;

                _hourNumPaint!.SetTextSizeByWidth(_singleHourNumWidth);
                _minuteNumPaint!.SetTextSizeByWidth(_singleMinuteNumWidth);

                _previousCanvasSize = info.Size;
            }

            float hourMarkWidth = 25;
            float hourMarkHeight = 20;

            float minuteMarkWidth = 15;
            float minuteMarkHeight = 15;

            float margin = 10;

            //float hourNumBottomY = -_radius - margin;
            //float hourMarkTopY = hourNumBottomY - _hourNumPaint.TextSize - margin - hourMarkHeight;
            //float minuteMarkTopY = hourMarkTopY;
            //float minuteNumBottomY = minuteMarkTopY - margin;

            float minuteNumBottomY = -_radius + _minuteNumPaint!.TextSize;
            float minuteMarkTopY = minuteNumBottomY + margin;
            float hourMarkTopY = minuteMarkTopY;
            float hourNumBottomY = hourMarkTopY + hourMarkHeight + margin + _hourNumPaint!.TextSize;

            canvas.Translate(info.Width / 2f, info.Height / 2f);

            for (int i = 1; i <= 60; ++i)
            {
                canvas.RotateDegrees(6);

                if (i % 5 == 0)
                {
                    //小时数字
                    string hourNum = (i / 5).ToString(GlobalSettings.Culture);

                    float hourWidth = _hourNumPaint.MeasureText(hourNum);

                    canvas.DrawText(hourNum, -hourWidth / 2f, hourNumBottomY, _hourNumPaint);

                    //小时刻度

                    SKRect hourMarkRect = SKRect.Create(-hourMarkWidth / 2f, hourMarkTopY, hourMarkWidth, hourMarkHeight);

                    canvas.DrawRect(hourMarkRect, _hourMarkPaint);

                    //分钟数字
                    string minuteNum = i.ToString(GlobalSettings.Culture);

                    float minuteWidth = _minuteNumPaint.MeasureText(minuteNum);

                    canvas.DrawText(minuteNum, -minuteWidth / 2f, minuteNumBottomY, _minuteNumPaint);
                }
                else
                {
                    //分钟刻度

                    SKRect minuteMarkRect = SKRect.Create(-minuteMarkWidth / 2f, minuteMarkTopY, minuteMarkWidth, minuteMarkHeight);
                    canvas.DrawRect(minuteMarkRect, _minuteMarkPaint);
                }
            }
        }

        #region IDisposable

        private bool _isDisposed;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!_isDisposed)
            {
                if (disposing)
                {
                    //managed
                    _hourNumPaint?.Dispose();
                    _hourMarkPaint?.Dispose();

                    _minuteNumPaint?.Dispose();
                    _minuteMarkPaint?.Dispose();
                }
                else
                {
                    //native
                }

                _isDisposed = true;
            }
        }

        //~TicksFigure()
        //{
        //    Dispose(disposing: false);
        //}

        #endregion IDisposable
    }
}