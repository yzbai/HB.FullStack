using System;

using HB.FullStack.Mobile.Skia;

using SkiaSharp;
using SkiaSharp.Views.Forms;

using Xamarin.Forms;

namespace HB.FullStack.Mobile.Controls.Clock
{
    public class DragableMinuteHandFigure : SKFigure
    {
        private int _currentMinute;

        private float _handLength;

        private readonly WeakReference<DragableHourHandFigure> _houHand;

        private SKSize _previousCanvasSize;

        public int MinuteResult { get; set; }
        public bool CanAntiClockwise { get; set; }

        public DragableMinuteHandFigure(SKRatioPoint pivotPoint, float handLengthRatio, int initMinute, DragableHourHandFigure houHand)
        {
            EnableTouch = true;
            EnableDrag = true;

            _houHand = new WeakReference<DragableHourHandFigure>(houHand);

            Dragged += MinuteHandFigure_Dragged;

            SetMinute(initMinute);
            this.initMinute = initMinute;
            this.houHand = houHand;
            this.pivotPoint = pivotPoint;
            this.handLengthRatio = handLengthRatio;
        }

        public void SetMinute(int initMinute)
        {
            //初始化起始时间
            MinuteResult = _currentMinute = initMinute;

            Matrix = SKUtil.MinuteToMatrix(_currentMinute);

            if (_houHand.TryGetTarget(out DragableHourHandFigure hourhand))
            {
                hourhand.AdjustBySetMinute(_currentMinute);
            }
        }

        private readonly SKPaint _minutePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 5,
            Color = Color.Blue.ToSKColor(),
            IsAntialias = true
        };
        private readonly int initMinute;
        private readonly DragableHourHandFigure houHand;
        private readonly SKRatioPoint pivotPoint;
        private readonly float handLengthRatio;

        public override void OnPaint(SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            _previousCanvasSize = info.Size;

            CaculateTimeResult();

            canvas.Translate(info.Width / 2, info.Height / 2);

            _handLength = Math.Min(info.Height, info.Width) * handLengthRatio;

            canvas.Concat(ref Matrix);

            canvas.DrawLine(0, 0, 0, -_handLength, _minutePaint);
        }

        private void CaculateTimeResult()
        {
            MinuteResult = SKUtil.MatrixToDisplayMinuteResult(ref Matrix);
        }

        private void MinuteHandFigure_Dragged(object sender, SKTouchInfoEventArgs info)
        {
            SKPoint previousPoint = SKUtil.TranslatePointToCenter(info.PreviousPoint, _previousCanvasSize);
            SKPoint currentPoint = SKUtil.TranslatePointToCenter(info.CurrentPoint, _previousCanvasSize);

            double rotatedRadian = SKUtil.CaculateRotatedRadian(previousPoint, currentPoint, new SKPoint(0, 0));

            //_logger.LogDebug($"MinuteHandFigure_Dragged. rotatedRadian:{rotatedRadian}, previousPoint:{previousPoint}, currentPoint:{currentPoint}");

            if (rotatedRadian > 0 || CanAntiClockwise)
            {
                SKMatrix rotatedMatrix = SKMatrix.CreateRotation((float)rotatedRadian, 0, 0);

                if (rotatedMatrix.SkewY >= 0 || CanAntiClockwise)
                {
                    Matrix = Matrix.PostConcat(rotatedMatrix);

                    if (_houHand.TryGetTarget(out DragableHourHandFigure houHand))
                    {
                        houHand.AdjustHourByAddRadian(rotatedRadian / 12);
                    }
                }
            }

            if (info.IsOver)
            {
                double actuallyMinute = SKUtil.MatrixToMinute(CaculateType.Actually, ref Matrix);

                int adjustMinute = AdjustMinuteToPoint();

                if (_houHand.TryGetTarget(out DragableHourHandFigure houHand))
                {
                    houHand.AdjustHourAsMinuteChanged(actuallyMinute, adjustMinute);
                }
            }
        }

        private int AdjustMinuteToPoint()
        {
            int roundMinute = (int)SKUtil.MatrixToMinute(CaculateType.Round, ref Matrix); //每1分钟为一段,一共60段。

            Matrix = SKUtil.MinuteToMatrix(roundMinute);

            return roundMinute;
        }

        public override bool OnHitTest(SKPoint skPoint, long touchId)
        {
            SKPoint transedPoint = SKUtil.TranslatePointToCenter(skPoint, _previousCanvasSize);

            // Invert the matrix

            if (Matrix.TryInvert(out SKMatrix inverseMatrix))
            {
                // Transform the point using the inverted matrix
                SKPoint mappedPoint = inverseMatrix.MapPoint(transedPoint);

                // Check if it's in the untransformed bitmap rectangle
                SKRect rect = new SKRect(-25, -(_handLength * 3 / 2), 25, -(_handLength / 2));
                return rect.Contains(mappedPoint);
            }
            return false;
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

        #endregion Dispose Pattern
    }
}