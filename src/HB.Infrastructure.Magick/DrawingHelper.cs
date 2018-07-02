using System;
using System.Text;
using System.IO;
using ImageMagick;
using HB.Framework.Common.Utility;

namespace HB.Infrastructure.Magick
{
    public class DrawingHelper : IDrawingHelper
    {
        public Int32 FontMinSize { get; set; } = 15;

        public Int32 FontMaxSize { get; set; } = 20;

        
        //TODO: 修改
        public void WriteImageToStream(Stream target, string imageContentType, int width, int height, string code)
        {
            int codeLenght = code.Length;
            Random random = new Random();

            int fontSize = random.Next(FontMinSize, FontMaxSize);
            int fontX = random.Next(5, width / codeLenght);
            int fontY = random.Next(height / 5 + fontSize / 2, height - fontSize / 2);

            //如果当前字符X坐标小于字体的二分之一大小
            if (fontX < fontSize / 2)
            {
                fontX += fontSize / 2;
            }

            //如果当前字符X坐标大于图片宽度，就减去字体的宽度
            if (fontX > (width - fontSize / 2))
            {
                fontX = width - fontSize / 2;
            }

            using (MagickImage image = new MagickImage(MagickColor.FromRgb(255, 255, 255), width + 1, height = height + 1))
            {
                new Drawables()
                    .FontPointSize(new Random().Next(FontMinSize, FontMaxSize))
                    .Font("Comic Sans")
                    .StrokeColor(new MagickColor("red"))
                    //.FillColor(MagickColors.Orange)
                    //.TextAlignment(TextAlignment.Center)
                    .Text(15, 15, code)
                    //.StrokeColor(new MagickColor(0, Quantum.Max, 0))
                    //.FillColor(MagickColors.SaddleBrown)
                    //.Ellipse(256, 96, 192, 8, 0, 360)
                    .Draw(image);

                MagickFormat magickFormat = convertToMagickFormat(imageContentType);

                image.Write(target, magickFormat);
            }
        }

        private static MagickFormat convertToMagickFormat(string imageContentType)
        {
            MagickFormat magickFormat = MagickFormat.Png;

            if ("image/gif".Equals(imageContentType))
            {
                magickFormat = MagickFormat.Gif;
            }
            else if ("image/png".Equals(imageContentType))
            {
                magickFormat = MagickFormat.Png;
            }
            else if ("image/jpeg".Equals(imageContentType))
            {
                magickFormat = MagickFormat.Jpeg;
            }

            return magickFormat;
        }
    }
}