using SkiaSharp;
using System;
using Xamarin.Forms;

namespace HB.Framework.Client.Skia
{
    public enum CaculateType
    {
        Floor,
        Round,
        Ceiling,
        Actually
    }

    public static class SKUtil
    {
        #region 工具函数

        /// <summary>
        /// 返回转动的弧度
        /// </summary>
        /// <param name="prevPoint"></param>
        /// <param name="newPoint"></param>
        /// <param name="pivotPoint"></param>
        /// <returns></returns>
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

        public static double SecondToRadian(int second)
        {
            return second * 2.0 * Math.PI / 60;
        }

        public static double MatrixToRadian(ref SKMatrix martrix)
        {
            SKPoint minuteFirstPoint = new SKPoint(1, 0);
            SKPoint minuteMapPoint = martrix.MapPoint(minuteFirstPoint);

            return SKUtil.CaculateRotatedRadian(minuteFirstPoint, minuteMapPoint, new SKPoint(0, 0));
        }

        public static double RadianToDegree(double radian)
        {
            return (radian < 0 ? radian + 2 * Math.PI : radian) * 180 / Math.PI;
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

        public static double MatrixToMinute(CaculateType caculateType, ref SKMatrix minuteMatrix)
        {
            double minuteRadian = MatrixToRadian(ref minuteMatrix);

            double positiveRadian = minuteRadian < 0 ? minuteRadian + 2 * Math.PI : minuteRadian;

            if (caculateType == CaculateType.Ceiling)
            {
                int roundMinute = (int)Math.Ceiling(positiveRadian * 60 / 2 / Math.PI);

                return roundMinute == 60 ? 0 : roundMinute;
            }
            else if (caculateType == CaculateType.Round)
            {
                int roundMinute = (int)Math.Round(positiveRadian * 60 / 2 / Math.PI);

                return roundMinute == 60 ? 0 : roundMinute;
            }
            else if (caculateType == CaculateType.Floor)
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

        public static double MatrixToHour(CaculateType caculateType, ref SKMatrix hourMatrix)
        {
            double hourRadian = MatrixToRadian(ref hourMatrix);// + Math.PI; //时针从6点初始

            double positiveRadian = hourRadian < 0 ? hourRadian + 2 * Math.PI : hourRadian;

            if (caculateType == CaculateType.Floor)
            {
                return (int)Math.Floor(positiveRadian * 12 / 2 / Math.PI);
            }
            else if (caculateType == CaculateType.Round)
            {
                return (int)Math.Round(positiveRadian * 12 / 2 / Math.PI);
            }
            else if (caculateType == CaculateType.Ceiling)
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
            double acturallyHour = MatrixToHour(CaculateType.Actually, ref matrix);
            double cellingHour = MatrixToHour(CaculateType.Ceiling, ref matrix);

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
            return (int)MatrixToMinute(CaculateType.Round, ref matrix);
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

        public static double Density { get; set; } = Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Density;
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

        public static double DistanceInDp(Point a, Point b)
        {
            return Math.Sqrt(Math.Pow(b.Y - a.Y, 2) + Math.Pow(b.X - a.X, 2));
        }

        public static SKPoint TranslatePointToCenter(SKPoint skPoint, float canvasWidth, float canvasHeight)
        {
            return new SKPoint(skPoint.X - canvasWidth / 2, skPoint.Y - canvasHeight / 2);
        }
    }
}
