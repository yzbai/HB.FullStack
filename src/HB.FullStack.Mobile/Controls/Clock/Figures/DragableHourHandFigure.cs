using System;

using HB.FullStack.Mobile.Skia;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace HB.FullStack.Mobile.Controls.Clock
{
    /// <summary>
    /// 可拖拽的时针
    /// </summary>
    public class DragableHourHandFigure : SKFigure
    {
        private int _previousHour;
        private float _handLength;
        private SKSize _previousCanvasSize;

        public int HourResult { get; set; }

        public bool IsAM { get; set; } = true;
        public bool CanAntiClockwise { get; set; }

        public DragableHourHandFigure(int initHour, float ratio, SKAlignment horizontalAlignment, SKAlignment verticalAlignment) : base(ratio, ratio, horizontalAlignment, verticalAlignment)
        {
            EnableDrag = true;
            EnableTouch = true;

            Dragged += HourHandFigure_Dragged;

            SetHour(initHour);
        }

        private readonly SKPaint _hourPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 10,
            Color = SKColors.Brown,
            IsAntialias = true
        };

        public override void Paint(SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            _previousCanvasSize = info.Size;

            CaculateTimeResult();

            canvas.Translate(info.Width / 2f, info.Height / 2f);

            _handLength = GetFigureWidth(info.Size) / 4f;

            canvas.Concat(ref Matrix);

            canvas.DrawLine(0, 0, 0, -_handLength, _hourPaint);
        }

        public override bool HitTest(SKPoint skPoint, long touchId)
        {
            SKPoint transedPoint = SKUtil.TranslatePointToCenter(skPoint, _previousCanvasSize);

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

        private void CaculateTimeResult()
        {
            HourResult = SKUtil.MatrixToDisplayHourResult(ref Matrix);

            #region AM/PM

            if (_previousHour == 11 && (HourResult == 0 || HourResult == 12))
            {
                IsAM = !IsAM;
            }

            if (_previousHour == 0 && HourResult == 11)
            {
                IsAM = !IsAM;
            }

            _previousHour = HourResult;

            if (HourResult == 0 && !IsAM)
            {
                HourResult = 12;
            }

            if (HourResult == 12 && IsAM)
            {
                HourResult = 0;
            }

            #endregion AM/PM
        }

        public void SetHour(int hour)
        {
            if (hour >= 12)
            {
                IsAM = false;
                hour -= 12;
            }

            HourResult = _previousHour = hour;

            Matrix = SKUtil.HourToMatrix(_previousHour);

            //if (invalidateSurface)
            //{
            //    InvalidateSurface();
            //}
        }

        private void HourHandFigure_Dragged(object sender, SKTouchInfoEventArgs info)
        {
            SKPoint previousPoint = SKUtil.TranslatePointToCenter(info.PreviousPoint, _previousCanvasSize);
            SKPoint currentPoint = SKUtil.TranslatePointToCenter(info.CurrentPoint, _previousCanvasSize);

            double rotatedRadian = SKUtil.CaculateRotatedRadian(previousPoint, currentPoint, new SKPoint(0, 0));

            if (rotatedRadian > 0 || CanAntiClockwise)
            {
                SKMatrix rotatedMatrix = SKMatrix.CreateRotation((float)rotatedRadian, 0, 0);

                Matrix = Matrix.PostConcat(rotatedMatrix);
            }

            if (info.IsOver)
            {
                AdjustHourToPoint();
            }
        }

        public void AdjustBySetMinute(int minute)
        {
            Matrix = SKUtil.HourToMatrix(_previousHour, minute);

            //InvalidateSurface();
        }

        /// <summary>
        /// 增加弧度, 有弧度之间运算时，要确保弧度小于PI
        /// </summary>
        /// <param name="addedRadian"></param>
        public void AdjustHourByAddRadian(double addedRadian)
        {
            if (addedRadian == 0)
            {
                return;
            }

            //调整时针跟着转
            double adjustHourRadian = SKUtil.MatrixToRadian(ref Matrix) + addedRadian;

            //_logger.LogDebug($"AdjustHourByAddRadian. addedRadian : {addedRadian}, adjusted:{adjustHourRadian}, hour:{SKUtil.RadianToHour(adjustHourRadian)}");

            Matrix = SKUtil.RadianToMatrix(adjustHourRadian);
        }

        /// <summary>
        /// 足够接近时，调整到整点
        /// </summary>
        private void AdjustHourToPoint()
        {
            //纠正：只有十分接近整点时(一分钟)，才纠正
            double roudHour = SKUtil.MatrixToHour(CaculateType.Round, ref Matrix);
            double actuallyHour = SKUtil.MatrixToHour(CaculateType.Actually, ref Matrix);

            if (Math.Abs(actuallyHour - roudHour) < (11 / 60f)) //纠正的精度，12/60f 表示一个分钟内纠正
            {
                Matrix = SKUtil.HourToMatrix((int)roudHour);
            }
        }

        public void AdjustHourAsMinuteChanged(double actuallyMinute, int adjustMinute)
        {
            //时针纠正：根据分针，调整时针的准确位置

            int hour = (int)SKUtil.MatrixToHour(CaculateType.Floor, ref Matrix);
            int roundHour = (int)SKUtil.MatrixToHour(CaculateType.Round, ref Matrix);

            //自动调整，超过了0刻度，小时加1
            if (adjustMinute == 0 && 59 <= actuallyMinute && hour != roundHour) // hour != roundHour 是为了解决，分针可以轻微逆时针旋转的bug
            {
                hour++;
            }

            Matrix = SKUtil.HourToMatrix(hour, adjustMinute);
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
                    _hourPaint.Dispose();
                }

                //unmanaged

                _disposed = true;
            }
        }

        #endregion Dispose Pattern
    }
}