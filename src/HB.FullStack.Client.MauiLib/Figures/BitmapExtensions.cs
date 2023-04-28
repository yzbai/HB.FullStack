using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

using SkiaSharp;

namespace SkiaSharp
{
    public class DrawBitmapResult
    {
        public SKRect DisplayRect { get; set; }

        public float WidthScale { get; set; }

        public float HeightScale { get; set; }

        public DrawBitmapResult() { }

        public DrawBitmapResult(SKRect displayRect, float widthScale, float heightScale)
        {
            DisplayRect = displayRect;
            WidthScale = widthScale;
            HeightScale = heightScale;
        }
    }
    public static class BitmapExtensions
    {
 
        public static SKBitmap LoadBitmapResource(Type type, string resourceID)
        {
            Assembly assembly = type.GetTypeInfo().Assembly;

            using Stream stream = assembly.GetManifestResourceStream(resourceID)
                ?? throw ClientExceptions.ResourceNotFound(resourceId: resourceID);

            return SKBitmap.Decode(stream);
        }

        public static uint RgbaMakePixel(byte red, byte green, byte blue, byte alpha = 255)
        {
            return (uint)((alpha << 24) | (blue << 16) | (green << 8) | red);
        }

        public static void RgbaGetBytes(this uint pixel, out byte red, out byte green, out byte blue, out byte alpha)
        {
            red = (byte)pixel;
            green = (byte)(pixel >> 8);
            blue = (byte)(pixel >> 16);
            alpha = (byte)(pixel >> 24);
        }

        public static uint BgraMakePixel(byte blue, byte green, byte red, byte alpha = 255)
        {
            return (uint)((alpha << 24) | (red << 16) | (green << 8) | blue);
        }

        public static void BgraGetBytes(this uint pixel, out byte blue, out byte green, out byte red, out byte alpha)
        {
            blue = (byte)pixel;
            green = (byte)(pixel >> 8);
            red = (byte)(pixel >> 16);
            alpha = (byte)(pixel >> 24);
        }

        /// <summary>
        /// 返回实际占用的Rect 和 scale
        /// </summary>
        public static DrawBitmapResult DrawBitmap(this SKCanvas canvas, SKBitmap bitmap, SKRect dest,
                                      SKStretchMode stretch,
                                      SKAlignment horizontal = SKAlignment.Center,
                                      SKAlignment vertical = SKAlignment.Center,
                                      SKPaint? paint = null)
        {


            if (stretch == SKStretchMode.Fill)
            {
                canvas.DrawBitmap(bitmap, dest, paint);
                return new DrawBitmapResult(dest, dest.Width / bitmap.Width, dest.Height / bitmap.Height);
            }

            float scale = 1;

            switch (stretch)
            {
                case SKStretchMode.None:
                    break;

                case SKStretchMode.AspectFit:
                    scale = Math.Min(dest.Width / bitmap.Width, dest.Height / bitmap.Height);
                    break;

                case SKStretchMode.AspectFill:
                    scale = Math.Max(dest.Width / bitmap.Width, dest.Height / bitmap.Height);
                    break;
            }

            SKRect display = CalculateDisplayRect(dest, scale * bitmap.Width, scale * bitmap.Height, horizontal, vertical);

            canvas.DrawBitmap(bitmap, display, paint);

            return new DrawBitmapResult(display, scale, scale);
        }

        /// <summary>
        /// 返回实际占用的Rect和Scale
        /// </summary>
        public static DrawBitmapResult DrawBitmap(this SKCanvas canvas, SKBitmap bitmap, SKRect source, SKRect dest,
                                      SKStretchMode stretch,
                                      SKAlignment horizontal = SKAlignment.Center,
                                      SKAlignment vertical = SKAlignment.Center,
                                      SKPaint? paint = null)
        {
            if (stretch == SKStretchMode.Fill)
            {
                canvas.DrawBitmap(bitmap, source, dest, paint);
                return new DrawBitmapResult(dest, dest.Width / source.Width, dest.Height / source.Height);
            }

            float scale = 1;

            switch (stretch)
            {
                case SKStretchMode.None:
                    break;

                case SKStretchMode.AspectFit:
                    scale = Math.Min(dest.Width / source.Width, dest.Height / source.Height);
                    break;

                case SKStretchMode.AspectFill:
                    scale = Math.Max(dest.Width / source.Width, dest.Height / source.Height);
                    break;
            }

            SKRect display = CalculateDisplayRect(dest, scale * source.Width, scale * source.Height, horizontal, vertical);

            canvas.DrawBitmap(bitmap, source, display, paint);

            return new DrawBitmapResult(display, scale, scale);
        }

        static SKRect CalculateDisplayRect(SKRect dest, float bmpWidth, float bmpHeight,
                                           SKAlignment horizontal, SKAlignment vertical)
        {
            float x = 0;
            float y = 0;

            switch (horizontal)
            {
                case SKAlignment.Center:
                    x = (dest.Width - bmpWidth) / 2;
                    break;

                case SKAlignment.Start:
                    break;

                case SKAlignment.End:
                    x = dest.Width - bmpWidth;
                    break;
            }

            switch (vertical)
            {
                case SKAlignment.Center:
                    y = (dest.Height - bmpHeight) / 2;
                    break;

                case SKAlignment.Start:
                    break;

                case SKAlignment.End:
                    y = dest.Height - bmpHeight;
                    break;
            }

            x += dest.Left;
            y += dest.Top;

            return new SKRect(x, y, x + bmpWidth, y + bmpHeight);
        }
    }

    public enum SKStretchMode
    {
        None,
        Fill,
        //Uniform,
        //UniformToFill,
        AspectFit, //= Uniform,
        AspectFill// = UniformToFill
    }

    public enum SKAlignment
    {
        Start,
        Center,
        End
    }
}
