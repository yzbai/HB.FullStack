using System;

using HB.FullStack.Mobile.Skia;

using SkiaSharp;
using SkiaSharp.Views.Forms;

using Xamarin.Forms;

namespace HB.FullStack.Mobile.Controls.Clock
{
    public class MinuteHandFigure : SKFigure
    {
        private int _previousMinute;
        private float _handLength;
        private readonly float _handLengthRatio;
        private readonly WeakReference<HourHandFigure> _houHand;

        public int MinuteResult { get; set; }

        public bool CanAntiClockwise { get; set; }

        public MinuteHandFigure(float handLengthRatio, int initMinute, HourHandFigure houHand)
        {
            _houHand = new WeakReference<HourHandFigure>(houHand);
            _handLengthRatio = handLengthRatio;

            OneFingerDragged += OnDragged;

            InitMatrix(initMinute);
        }

        public void InitMatrix(int initMinute)
        {
            MinuteResult = _previousMinute = initMinute;

            Matrix = SKUtil.MinuteToMatrix(_previousMinute);

            if (_houHand.TryGetTarget(out HourHandFigure hourhand))
            {
                hourhand.AdjustBySetMinute(_previousMinute);
            }
        }

        private readonly SKPaint _minutePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 5,
            Color = Color.Blue.ToSKColor(),
            IsAntialias = true
        };

        protected override void OnDraw(SKImageInfo info, SKCanvas canvas)
        {
            if (CanvasSizeChanged)
            {
                _handLength = Math.Min(info.Height, info.Width) / 2f * _handLengthRatio;
            }

            canvas.DrawLine(0, 0, 0, -_handLength, _minutePaint);
        }

        protected override void OnUpdateHitTestPath(SKImageInfo info)
        {
            if (CanvasSizeChanged)
            {
                HitTestPath = new SKPath();
                SKRect rect = new SKRect(-25, -(_handLength * 3 / 2), 25, -(_handLength / 2));
                HitTestPath.AddRect(rect);
            }
        }

        protected override void OnCaculateOutput()
        {
            MinuteResult = SKUtil.MatrixToDisplayMinuteResult(ref Matrix);
        }

        private void OnDragged(object sender, SKFigureTouchEventArgs info)
        {
            //SKPoint previousPoint = GetNewCoordinatedPoint(info.PreviousPoint);
            //SKPoint currentPoint = GetNewCoordinatedPoint(info.CurrentPoint);
            SKPoint previousPoint = info.PreviousPoint;
            SKPoint currentPoint = info.CurrentPoint;

            double rotatedRadian = SKUtil.CaculateRotatedRadian(previousPoint, currentPoint, new SKPoint(0, 0));

            //_logger.LogDebug($"MinuteHandFigure_Dragged. rotatedRadian:{rotatedRadian}, previousPoint:{previousPoint}, currentPoint:{currentPoint}");

            if (rotatedRadian > 0 || CanAntiClockwise)
            {
                SKMatrix rotatedMatrix = SKMatrix.CreateRotation((float)rotatedRadian, 0, 0);

                if (rotatedMatrix.SkewY >= 0 || CanAntiClockwise)
                {
                    Matrix = Matrix.PostConcat(rotatedMatrix);

                    if (_houHand.TryGetTarget(out HourHandFigure houHand))
                    {
                        houHand.AdjustHourByAddRadian(rotatedRadian / 12);
                    }
                }
            }

            if (info.IsOver)
            {
                double actuallyMinute = SKUtil.MatrixToMinute(EstimateType.Actually, ref Matrix);

                int adjustMinute = AdjustMinuteToPoint();

                if (_houHand.TryGetTarget(out HourHandFigure houHand))
                {
                    houHand.AdjustHourAsMinuteChanged(actuallyMinute, adjustMinute);
                }
            }
        }

        private int AdjustMinuteToPoint()
        {
            int roundMinute = (int)SKUtil.MatrixToMinute(EstimateType.Round, ref Matrix); //每1分钟为一段,一共60段。

            Matrix = SKUtil.MinuteToMatrix(roundMinute);

            return roundMinute;
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
                    _minutePaint?.Dispose();
                }

                _disposed = true;
            }
        }

        #endregion
    }
}