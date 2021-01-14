using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using SkiaSharp;

namespace HB.FullStack.Mobile.Skia
{
    public static class BitmapExtensions
    {
        /// <summary>
        /// LoadBitmapResource
        /// </summary>
        /// <param name="type"></param>
        /// <param name="resourceID"></param>
        /// <returns></returns>
        /// <exception cref="MobileException"></exception>
        public static SKBitmap LoadBitmapResource(Type type, string resourceID)
        {
            Assembly assembly = type.GetTypeInfo().Assembly;

            using Stream stream = assembly.GetManifestResourceStream(resourceID) ?? throw new MobileException(MobileErrorCode.ResourceNotFound, $"ResourceId:{resourceID}");
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

        public static void DrawBitmap(this SKCanvas canvas, SKBitmap bitmap, SKRect dest,
                                      SKStretch stretch,
                                      SKAlignment horizontal = SKAlignment.Center,
                                      SKAlignment vertical = SKAlignment.Center,
                                      SKPaint? paint = null)
        {
            if (stretch == SKStretch.Fill)
            {
                canvas.DrawBitmap(bitmap, dest, paint);
            }
            else
            {
                float scale = 1;

                switch (stretch)
                {
                    case SKStretch.None:
                        break;

                    case SKStretch.AspectFit:
                        scale = Math.Min(dest.Width / bitmap.Width, dest.Height / bitmap.Height);
                        break;

                    case SKStretch.AspectFill:
                        scale = Math.Max(dest.Width / bitmap.Width, dest.Height / bitmap.Height);
                        break;
                }

                SKRect display = CalculateDisplayRect(dest, scale * bitmap.Width, scale * bitmap.Height,
                                                      horizontal, vertical);

                canvas.DrawBitmap(bitmap, display, paint);
            }
        }

        public static void DrawBitmap(this SKCanvas canvas, SKBitmap bitmap, SKRect source, SKRect dest,
                                      SKStretch stretch,
                                      SKAlignment horizontal = SKAlignment.Center,
                                      SKAlignment vertical = SKAlignment.Center,
                                      SKPaint? paint = null)
        {
            if (stretch == SKStretch.Fill)
            {
                canvas.DrawBitmap(bitmap, source, dest, paint);
            }
            else
            {
                float scale = 1;

                switch (stretch)
                {
                    case SKStretch.None:
                        break;

                    case SKStretch.AspectFit:
                        scale = Math.Min(dest.Width / source.Width, dest.Height / source.Height);
                        break;

                    case SKStretch.AspectFill:
                        scale = Math.Max(dest.Width / source.Width, dest.Height / source.Height);
                        break;
                }

                SKRect display = CalculateDisplayRect(dest, scale * source.Width, scale * source.Height,
                                                      horizontal, vertical);

                canvas.DrawBitmap(bitmap, source, display, paint);
            }
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

    public enum SKStretch
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
