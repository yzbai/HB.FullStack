
using HB.FullStack.XamarinForms.Effects.Touch;

using Microsoft;

using SkiaSharp;

using System;
using System.Diagnostics.CodeAnalysis;

using Xamarin.Forms;

namespace HB.FullStack.XamarinForms.Skia
{
    public enum EstimateType
    {
        Floor,
        Round,
        Ceiling,
        Actually
    }

    public static class SKConsts
    {
        public const int MINUTES_ONE_DAY = 24 * 60;

        public const float HALF_PI = (float)(0.5 * Math.PI);
        public const float TWO_PI = (float)(2 * Math.PI);

        public const float RADIANS_ONE_DEGREE = (float)(Math.PI / 180);
        //public const float RADIANS_ONE_DAY = (float)(4 * Math.PI);
        //public const float RADIANS_ONE_HOUR = (float)Math.PI / 6;
        //public const float RADIANS_ONE_MINUTE = (float)(Math.PI / 360);
        public const float DEGREES_ONE_RADIAN = (float)(180 / Math.PI);
    }

    public static class SKUtil
    {
        #region Touch

        public static SKMatrix CaculateOneFingerDraggedMatrix(SKPoint prevPoint, SKPoint curPoint, SKPoint pivotPoint, TouchManipulationMode mode)
        {
            if (mode == TouchManipulationMode.None)
            {
                return SKMatrix.CreateIdentity();
            }

            SKMatrix touchMatrix = SKMatrix.CreateIdentity();
            SKPoint delta = curPoint - prevPoint;

            if (mode == TouchManipulationMode.ScaleDualRotate)  // One-finger rotation
            {
                SKPoint oldVector = prevPoint - pivotPoint;
                SKPoint newVector = curPoint - pivotPoint;

                // Avoid rotation if fingers are too close to center
                if (Magnitude(newVector) > 25 && Magnitude(oldVector) > 25)
                {
                    float prevAngle = (float)Math.Atan2(oldVector.Y, oldVector.X);
                    float newAngle = (float)Math.Atan2(newVector.Y, newVector.X);

                    // Calculate rotation matrix
                    float angle = newAngle - prevAngle;
                    touchMatrix = SKMatrix.CreateRotation(angle, pivotPoint.X, pivotPoint.Y);

                    // Effectively rotate the old vector
                    float magnitudeRatio = Magnitude(oldVector) / Magnitude(newVector);
                    oldVector.X = magnitudeRatio * newVector.X;
                    oldVector.Y = magnitudeRatio * newVector.Y;

                    // Recalculate delta
                    delta = newVector - oldVector;
                }
            }

            // Multiply the rotation matrix by a translation matrix
            touchMatrix = touchMatrix.PostConcat(SKMatrix.CreateTranslation(delta.X, delta.Y));

            return touchMatrix;
        }

        public static SKMatrix CaculateTwoFingerDraggedMatrix(SKPoint prevPoint, SKPoint curPoint, SKPoint pivotPoint, TouchManipulationMode mode)
        {
            SKMatrix touchMatrix = SKMatrix.CreateIdentity();

            SKPoint oldVector = prevPoint - pivotPoint;
            SKPoint newVector = curPoint - pivotPoint;

            if (mode == TouchManipulationMode.ScaleRotate ||
                mode == TouchManipulationMode.ScaleDualRotate)
            {
                // Find angles from pivot point to touch points
                float oldAngle = (float)Math.Atan2(oldVector.Y, oldVector.X);
                float newAngle = (float)Math.Atan2(newVector.Y, newVector.X);

                // Calculate rotation matrix
                float angle = newAngle - oldAngle;
                touchMatrix = SKMatrix.CreateRotation(angle, pivotPoint.X, pivotPoint.Y);

                // Effectively rotate the old vector
                float magnitudeRatio = Magnitude(oldVector) / Magnitude(newVector);
                oldVector.X = magnitudeRatio * newVector.X;
                oldVector.Y = magnitudeRatio * newVector.Y;
            }

            float scaleX = 1;
            float scaleY = 1;

            if (mode == TouchManipulationMode.AnisotropicScale)
            {
                scaleX = newVector.X / oldVector.X;
                scaleY = newVector.Y / oldVector.Y;

            }
            else if (mode == TouchManipulationMode.IsotropicScale ||
                     mode == TouchManipulationMode.ScaleRotate ||
                     mode == TouchManipulationMode.ScaleDualRotate)
            {
                scaleX = scaleY = Magnitude(newVector) / Magnitude(oldVector);
            }

            if (!float.IsNaN(scaleX) && !float.IsInfinity(scaleX) &&
                !float.IsNaN(scaleY) && !float.IsInfinity(scaleY))
            {
                touchMatrix = touchMatrix.PostConcat(SKMatrix.CreateScale(scaleX, scaleY, pivotPoint.X, pivotPoint.Y));
            }

            return touchMatrix;
        }

        public static float Magnitude(SKPoint point)
        {
            return (float)Math.Sqrt(Math.Pow(point.X, 2) + Math.Pow(point.Y, 2));
        }

        #endregion

        #region 弧度 & 角度 & 时间

        public static double CaculateRotatedRadian(SKPoint prevPoint, SKPoint newPoint, SKPoint pivotPoint)
        {
            SKPoint oldVector = prevPoint - pivotPoint;
            SKPoint newVector = newPoint - pivotPoint;

            double prevAngle = Math.Atan2(oldVector.Y, oldVector.X);
            double newAngle = Math.Atan2(newVector.Y, newVector.X);

            double rotatedRadian = newAngle - prevAngle;


            //纠正，要那个转动的小角度，而不是大角度。
            //有时候，在处理Touch时，move时，系统偶尔会飞点，即，转动方向突然变化，由正变负，或者由负变正
            if (rotatedRadian > Math.PI)
            {
                rotatedRadian -= 2 * Math.PI;
            }
            else if (rotatedRadian < -Math.PI)
            {
                rotatedRadian += 2 * Math.PI;
            }

            return rotatedRadian;
        }

        public static SKMatrix RadianToMatrix(double radian)
        {
            SKMatrix newMatrix = SKMatrix.CreateIdentity();

            newMatrix.ScaleX = (float)Math.Cos(radian);
            newMatrix.ScaleY = newMatrix.ScaleX;
            newMatrix.SkewY = (float)Math.Sin(radian);
            newMatrix.SkewX = -newMatrix.SkewY;

            return newMatrix;
        }

        public static double MatrixToRadian(ref SKMatrix martrix, float pivotX = 0, float pivotY = 0)
        {
            SKPoint minuteFirstPoint = new SKPoint(1, 0);
            SKPoint minuteMapPoint = martrix.MapPoint(minuteFirstPoint);

            return CaculateRotatedRadian(minuteFirstPoint, minuteMapPoint, new SKPoint(pivotX, pivotY));
        }

        public static double RadianToDegree(double radian)
        {
            return (radian < 0 ? radian + 2 * Math.PI : radian) * 180 / Math.PI;
        }

        public static double SecondToRadian(int second)
        {
            return second * 2.0 * Math.PI / 60;
        }

        public static double HourToRadian(int hour)
        {
            return HourToRadian(hour, 0);
        }

        public static double MinuteToRadian(int minute)
        {
            return minute * 2.0 * Math.PI / 60;
        }

        public static double RadianToMinute(double radian)
        {
            return 60 * radian / (2 * Math.PI);
        }

        public static double MatrixToMinute(EstimateType caculateType, ref SKMatrix minuteMatrix)
        {
            double minuteRadian = MatrixToRadian(ref minuteMatrix);

            double positiveRadian = minuteRadian < 0 ? minuteRadian + 2 * Math.PI : minuteRadian;

            if (caculateType == EstimateType.Ceiling)
            {
                int roundMinute = (int)Math.Ceiling(positiveRadian * 60 / 2 / Math.PI);

                return roundMinute == 60 ? 0 : roundMinute;
            }
            else if (caculateType == EstimateType.Round)
            {
                int roundMinute = (int)Math.Round(positiveRadian * 60 / 2 / Math.PI);

                return roundMinute == 60 ? 0 : roundMinute;
            }
            else if (caculateType == EstimateType.Floor)
            {
                int roundMinute = (int)Math.Floor(positiveRadian * 60 / 2 / Math.PI);

                return roundMinute == 60 ? 0 : roundMinute;
            }
            else //Actually
            {
                return positiveRadian * 60 / 2 / Math.PI;
            }
        }

        public static double RadianToHour(double rotatedRadian)
        {
            return 12 * rotatedRadian / (2 * Math.PI);
        }

        public static double MatrixToHour(EstimateType caculateType, ref SKMatrix hourMatrix)
        {
            double hourRadian = MatrixToRadian(ref hourMatrix);// + Math.PI; //时针从6点初始

            double positiveRadian = hourRadian < 0 ? hourRadian + 2 * Math.PI : hourRadian;

            if (caculateType == EstimateType.Floor)
            {
                return (int)Math.Floor(positiveRadian * 12 / 2 / Math.PI);
            }
            else if (caculateType == EstimateType.Round)
            {
                return (int)Math.Round(positiveRadian * 12 / 2 / Math.PI);
            }
            else if (caculateType == EstimateType.Ceiling)
            {
                return (int)Math.Ceiling(positiveRadian * 12 / 2 / Math.PI);
            }
            else
            {
                return positiveRadian * 12 / 2 / Math.PI;
            }
        }

        public static SKMatrix HourToMatrix(int hour)
        {
            double radian = HourToRadian(hour);

            if (radian > Math.PI)
            {
                radian -= 2 * Math.PI;
            }

            return RadianToMatrix(radian);
        }

        public static SKMatrix MinuteToMatrix(int minute)
        {
            double radian = MinuteToRadian(minute);

            if (radian > Math.PI)
            {
                radian -= 2 * Math.PI;
            }

            return RadianToMatrix(radian);
        }

        public static int MatrixToDisplayHourResult(ref SKMatrix matrix)
        {
            double acturallyHour = MatrixToHour(EstimateType.Actually, ref matrix);
            double cellingHour = MatrixToHour(EstimateType.Ceiling, ref matrix);

            //比如8点，roudHour = 8, reCaculateActuallyHour = 7.99999998515693, 取Floor后，直接变成7
            //分别在2，5，8，11会出现这样的情况
            //1.99999998515693
            //4.99999998515693
            //7.99999998515693
            //10.9999999851569
            //Hour纠正

            if (cellingHour - acturallyHour < 0.000000015)
            {
                return (int)cellingHour;
            }
            else
            {
                return (int)Math.Floor(acturallyHour);
            }
        }

        public static int MatrixToDisplayMinuteResult(ref SKMatrix matrix)
        {
            return (int)MatrixToMinute(EstimateType.Round, ref matrix);
        }

        public static double HourToRadian(int hour, int minute)
        {
            double radian = (hour + minute / 60f) * (2 * Math.PI / 12);

            //if (radian > Math.PI)
            //{
            //    radian -= 2 * Math.PI;
            //}

            return radian;
        }

        public static SKMatrix HourToMatrix(int hour, int minute)
        {
            double radian = HourToRadian(hour, minute);

            return RadianToMatrix(radian);
        }

        #endregion

        #region 点 & 距离

        public static double Density { get; } = Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Density;

        public static double ToPx(double dp)
        {
            return dp * Density;
        }

        public static double ToDp(double px)
        {
            return px / Density;
        }

        public static SKPoint ToSKPoint(Point point)
        {
            return new SKPoint((float)ToPx(point.X), (float)ToPx(point.Y));
        }

        public static Point ToPoint(SKPoint sKPoint)
        {
            return new Point(ToDp(sKPoint.X), ToDp(sKPoint.Y));
        }

        public static double Distance(Point a, Point b)
        {
            return Math.Sqrt(Math.Pow(b.Y - a.Y, 2) + Math.Pow(b.X - a.X, 2));
        }

        public static double Distance(SKPoint a, SKPoint b)
        {
            return Math.Sqrt(Math.Pow(b.Y - a.Y, 2) + Math.Pow(b.X - a.X, 2));
        }

        /// <summary>
        /// 将以左上角为坐标系的点，转换为以中心为坐标系上的点。同一个点。
        /// </summary>
        //public static SKPoint PivotPointToCenter(SKPoint skPoint, SKSize canvasSize)
        //{
        //    return new SKPoint(skPoint.X - canvasSize.Width / 2, skPoint.Y - canvasSize.Height / 2);
        //}

        #endregion
    }

    public static class SKExtensions
    {
        public static void SetTextSizeByWidth(this SKPaint paint, float width, char sample = '1')
        {
            paint.TextSize = width * paint.TextSize / paint.MeasureText(sample.ToString());
        }

        public static bool IsNullOrEmpty([ValidatedNotNull][NotNullWhen(false)] this SKPath? path)
        {
            return path == null || path.IsEmpty;
        }

        public static bool IsNotNullOrEmpty([ValidatedNotNull][NotNullWhen(true)] this SKPath? path)
        {
            return path != null && !path.IsEmpty;
        }
    }

}
