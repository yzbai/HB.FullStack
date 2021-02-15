using System;

using HB.FullStack.XamarinForms.Skia;

using SkiaSharp;

namespace HB.FullStack.XamarinForms.Controls.Clock
{
    public class HourHandFigure : SKFigure
    {
        private int _previousHour;
        private float _handLength;
        private readonly float _handLengthRatio;

        public int HourResult { get; private set; }
        public bool IsAM { get; private set; } = true;
        public bool CanAntiClockwise { get; set; }

        public HourHandFigure(float handLengthRatio, int initHour24)
        {
            _handLengthRatio = handLengthRatio;

            OneFingerDragged += OnDragged;

            InitMatrix(initHour24);
        }

        private void InitMatrix(int initHour24)
        {
            int hour = initHour24;

            if (hour >= 12)
            {
                IsAM = false;
                hour -= 12;
            }

            HourResult = _previousHour = hour;

            Matrix = SKUtil.HourToMatrix(_previousHour);
        }

        private readonly SKPaint _hourPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 10,
            Color = SKColors.Brown,
            IsAntialias = true
        };

        protected override void OnDraw(SKImageInfo info, SKCanvas canvas)
        {
            if (CanvasSizeChanged)
            {
                _handLength = Math.Min(info.Height, info.Width) / 2f * _handLengthRatio;
            }

            canvas.DrawLine(0, 0, 0, -_handLength, _hourPaint);
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
            HourResult = SKUtil.MatrixToDisplayHourResult(ref Matrix);

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
        }

        #region CaculateMatrixByTouch

        private void OnDragged(object sender, SKFigureTouchEventArgs info)
        {
            //SKPoint previousPoint = GetNewCoordinatedPoint(info.PreviousPoint);    //新坐标系下的点
            //SKPoint currentPoint = GetNewCoordinatedPoint(info.CurrentPoint);      //新坐标系下的点
            SKPoint piovtPoint = new SKPoint(0, 0);                         //围绕新坐标系的原点

            double rotatedRadian = SKUtil.CaculateRotatedRadian(info.PreviousPoint, info.CurrentPoint, piovtPoint);

            if (rotatedRadian > 0 || CanAntiClockwise)
            {
                SKMatrix rotatedMatrix = SKMatrix.CreateRotation((float)rotatedRadian, piovtPoint.X, piovtPoint.Y);

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
            double roudHour = SKUtil.MatrixToHour(EstimateType.Round, ref Matrix);
            double actuallyHour = SKUtil.MatrixToHour(EstimateType.Actually, ref Matrix);

            if (Math.Abs(actuallyHour - roudHour) < (11 / 60f)) //纠正的精度，12/60f 表示一个分钟内纠正
            {
                Matrix = SKUtil.HourToMatrix((int)roudHour);
            }
        }

        public void AdjustHourAsMinuteChanged(double actuallyMinute, int adjustMinute)
        {
            //时针纠正：根据分针，调整时针的准确位置

            int hour = (int)SKUtil.MatrixToHour(EstimateType.Floor, ref Matrix);
            int roundHour = (int)SKUtil.MatrixToHour(EstimateType.Round, ref Matrix);

            //自动调整，超过了0刻度，小时加1
            if (adjustMinute == 0 && 59 <= actuallyMinute && hour != roundHour) // hour != roundHour 是为了解决，分针可以轻微逆时针旋转的bug
            {
                hour++;
            }

            Matrix = SKUtil.HourToMatrix(hour, adjustMinute);
        }

        #endregion

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