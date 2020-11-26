using System;
using System.Collections.Generic;
using System.Text;
using SkiaSharp;

namespace SkiaSharp
{
    public static class SkiaExtensions
    {
        public static void SetTextSizeByWidth(this SKPaint paint, float width, char sample = '1')
        {
            paint.TextSize = width * paint.TextSize / paint.MeasureText(sample.ToString());
        }
    }
}
