using System;

using HB.FullStack.Mobile.Skia;

using Microsoft.Extensions.Logging;

using SkiaSharp;
using SkiaSharp.Extended;
using SkiaSharp.Views.Forms;

namespace HB.FullStack.Mobile.Controls.Clock
{
    public class TimeBlockFigure : SKFigure, IStatedFigure
    {
        private SKRegion? _previousRegion;
        private SKPath? _previousPath;

        private readonly SKColor _color;

        private readonly SKPaint _dotPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Blue,
            StrokeCap = SKStrokeCap.Round,
            PathEffect = SKPathEffect.CreateDash(new float[] { 10, 10 }, 20),
            IsAntialias = true,
            StrokeWidth = 10
        };

        private readonly SKPaint _sectorPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };

        private readonly SKPaint _dotPaint2 = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Brown,
            IsAntialias = true,
            StrokeWidth = 10
        };
        private readonly TimeBlockDrawInfo _drawInfo;
        private readonly float _innerRadiusRatio;
        private readonly float _outterRadiusRatio;

        public TimeBlockFigure(float innerRadiusRatio, float outterRadiusRatio, TimeBlockDrawInfo drawInfo)
        {
            State = FigureState.None;

            PreviousStartTime = CurrentStartTime = drawInfo.StartTime;
            PreviousEndTime = CurrentEndTime = drawInfo.EndTime;

            _color = drawInfo.Color;

            //Tapped += TimeBlockFigure_Tapped;
            //LongTapped += TimeBlockFigure_LongTapped;
            Dragged += TimeBlockFigure_Dragged;
            //Cancelled += TimeBlockFigure_Cancelled;
            //HitFailed += TimeBlockFigure_HitFailed;
            _drawInfo = drawInfo;
            _innerRadiusRatio = innerRadiusRatio;
            _outterRadiusRatio = outterRadiusRatio;
        }

        public FigureState State { get; set; }

        /// <summary>
        /// 最近的已经确定的开始时间
        /// </summary>
        public Time24Hour PreviousStartTime { get; set; }

        /// <summary>
        /// 最近的已经确定的结束时间
        /// </summary>
        public Time24Hour PreviousEndTime { get; set; }

        /// <summary>
        /// 当前拖动后的开始时间
        /// </summary>
        public Time24Hour CurrentStartTime { get; set; }

        /// <summary>
        /// 当前拖动后的结束时间
        /// </summary>
        public Time24Hour CurrentEndTime { get; set; }

        protected override void OnDraw(SKImageInfo info, SKCanvas canvas)
        {
            //canvas.Translate(info.Width / 2f, info.Height / 2f);

            TimeBlockSpanType spanType = GetTimeSpanType(CurrentStartTime.Hour, CurrentEndTime.Hour);

            SKPath? path1 = null;
            SKPath? path2 = null;

            float length = Math.Min(info.Height, info.Width);

            float innerRadius = length * _innerRadiusRatio;
            float outterRadius = length * _outterRadiusRatio;

            switch (spanType)
            {
                case TimeBlockSpanType.Inner:
                    path1 = SKGeometry.CreateSectorPath(
                        (float)GetTimePercent(CurrentStartTime),
                        (float)GetTimePercent(CurrentEndTime),
                        innerRadius);
                    break;

                case TimeBlockSpanType.Outter:
                    path1 = SKGeometry.CreateSectorPath(
                        (float)GetTimePercent(CurrentStartTime),
                        (float)GetTimePercent(CurrentEndTime),
                        outterRadius, innerRadius);
                    break;

                case TimeBlockSpanType.AmCross:
                    path1 = SKGeometry.CreateSectorPath(
                        (float)GetTimePercent(CurrentStartTime),
                        0.5f,
                        outterRadius, innerRadius);
                    path2 = SKGeometry.CreateSectorPath(
                        0.5f,
                        (float)GetTimePercent(CurrentEndTime),
                        innerRadius);
                    break;

                case TimeBlockSpanType.PmCross:
                    path1 = SKGeometry.CreateSectorPath(
                        (float)GetTimePercent(CurrentStartTime),
                        0.5f,
                        innerRadius);
                    path2 = SKGeometry.CreateSectorPath(
                        0.5f,
                        (float)GetTimePercent(CurrentEndTime),
                        outterRadius, innerRadius);
                    break;

                default:
                    break;
            }

            _sectorPaint.Color = _color;

            _previousRegion?.Dispose();
            _previousPath?.Dispose();

            _previousRegion = new SKRegion();
            _previousPath = new SKPath();

            if (path1 != null)
            {
                _previousRegion.SetPath(path1);
                _previousPath.AddPath(path1);
                canvas.DrawPath(path1, _sectorPaint);
            }

            if (path2 != null)
            {
                if (path1 == null)
                {
                    _previousRegion.SetPath(path2);
                    _previousPath.AddPath(path2);
                }
                else
                {
                    _previousRegion.Op(path2, SKRegionOperation.Union);
                    _previousPath = _previousPath.Op(path2, SKPathOp.Union);
                }

                canvas.DrawPath(path2, _sectorPaint);
            }

            if (State == FigureState.Tapped)
            {
                if (path1 != null)
                {
                    canvas.DrawPath(path1, _dotPaint2);
                }

                if (path2 != null)
                {
                    canvas.DrawPath(path2, _dotPaint2);
                }
            }

            if (State == FigureState.LongTapped)
            {
                if (path1 != null)
                {
                    canvas.DrawPath(path1, _dotPaint);
                }

                if (path2 != null)
                {
                    canvas.DrawPath(path2, _dotPaint);
                }
            }

            path1?.Dispose();
            path2?.Dispose();
        }

        protected override void OnUpdateHitTestPath(SKImageInfo info)
        {
            HitTestPath = _previousPath;
        }

        protected override void OnCaculateOutput()
        {

        }

        public void SetState(FigureState state)
        {
            GlobalSettings.Logger.LogDebug(state.ToString());

            if (State == FigureState.LongTapped && state == FigureState.Dragged)
            {
                return;
            }

            State = state;
        }

        //public override bool OnHitTest(SKPoint skPoint, long touchId)
        //{
        //    if (_previousRegion == null)
        //    {
        //        return false;
        //    }

        //    SKPoint transedPoint = SKUtil.PivotPointToCenter(skPoint, _previousCanvasSize);

        //    return _previousRegion.Contains((int)transedPoint.X, (int)transedPoint.Y);
        //}

        private void TimeBlockFigure_Dragged(object sender, SKTouchInfoEventArgs info)
        {
            GlobalSettings.Logger.LogDebug("DDDDDDDDDDDDD_   Dragged");
            if (State != FigureState.LongTapped)
            {
                return;
            }

            SKPoint previousPoint = GetPivotedPoint(info.PreviousPoint);
            SKPoint currentPoint = GetPivotedPoint(info.CurrentPoint);

            double rotatedRadian = SKUtil.CaculateRotatedRadian(previousPoint, currentPoint, new SKPoint(0, 0));

            double actuallyChangedHour = SKUtil.RadianToHour(rotatedRadian);

            ReSetCurrentTime(actuallyChangedHour);

            if (info.IsOver)
            {
                //TODO: 看看需不需要调整到对齐到5的倍数分钟上，比如几点05，几点10，几点15，几点20
            }
        }

        //private void TimeBlockFigure_LongTapped(object sender, SKTouchInfoEventArgs info)
        //{
        //    State = FigureState.LongTapped;

        //    if (Parent is TimeBlockFigureGroup group)
        //    {
        //        group.SetLongTapped(this);
        //    }
        //}

        //private void TimeBlockFigure_Tapped(object sender, SKTouchInfoEventArgs info)
        //{
        //    State = TimeBlockFigureState.Tapped;

        //    if (Parent is TimeBlockFigureGroup group)
        //    {
        //        group.SetTapped(this);
        //    }
        //}

        //private void TimeBlockFigure_Cancelled(object sender, SKTouchInfoEventArgs e)
        //{
        //    if (Parent is TimeBlockFigureGroup group)
        //    {
        //        group.SetAllBackToNormal();
        //    }
        //}

        //private void TimeBlockFigure_HitFailed(object sender, EventArgs e)
        //{
        //    State = TimeBlockFigureState.Normal;
        //}

        private void ReSetCurrentTime(double actuallyChangedHour)
        {
            //TODO: 是否有0，0000000015的问题

            if (actuallyChangedHour == 0)
            {
                return;
            }

            double absActuallyChangedHour = Math.Abs(actuallyChangedHour);

            int changedHour = (int)Math.Floor(absActuallyChangedHour);

            int changedMinute = (int)Math.Round((absActuallyChangedHour - changedHour) * 60);

            if (actuallyChangedHour > 0)
            {
                CurrentStartTime = CurrentStartTime.AddTime(changedHour, changedMinute);
                CurrentEndTime = CurrentEndTime.AddTime(changedHour, changedMinute);
            }
            else
            {
                CurrentStartTime = CurrentStartTime.MinusTime(changedHour, changedMinute);
                CurrentEndTime = CurrentEndTime.MinusTime(changedHour, changedMinute);
            }
        }

        private static double GetTimePercent(Time24Hour time)
        {
            int hour = time.Hour > 12 ? time.Hour - 12 : time.Hour;

            return (hour * 60 + time.Minute) / (12 * 60f);
        }

        private static TimeBlockSpanType GetTimeSpanType(int startHourIn24, int endHourIn24)
        {
            bool isStartHourInner = startHourIn24 >= 6 && startHourIn24 <= 17;
            bool isEndHourInner = endHourIn24 >= 6 && endHourIn24 <= 17;

            if (isStartHourInner == isEndHourInner)
            {
                return isStartHourInner ? TimeBlockSpanType.Inner : TimeBlockSpanType.Outter;
            }
            else
            {
                return isEndHourInner ? TimeBlockSpanType.AmCross : TimeBlockSpanType.PmCross;
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
                    _previousRegion?.Dispose();
                    _previousPath?.Dispose();
                    _dotPaint.Dispose();
                    _dotPaint2.Dispose();
                    _sectorPaint.Dispose();
                }

                //unmanaged

                _disposed = true;
            }
        }

        #endregion Dispose Pattern
    }
}