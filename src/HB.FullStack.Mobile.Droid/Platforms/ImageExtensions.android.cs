using System;
using System.Threading.Tasks;
using Android.Graphics;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.Android;

namespace Xamarin.Forms
{
    public static class ImageExtensions
    {
        public static IImageSourceHandler GetHandler(this ImageSource imageSource)
        {
            if (imageSource == null || imageSource.IsEmpty)
                return null;

            return Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(imageSource);

            //if (imageSource is UriImageSource)
            //{
            //    return new ImageLoaderSourceHandler();
            //}
            //else if (imageSource is FileImageSource)
            //{
            //    return new FileImageSourceHandler();
            //}
            //else if (imageSource is StreamImageSource)
            //{
            //    return new StreamImagesourceHandler();
            //}
            //else
            //{
                
            //}
        }

        public static Task<Bitmap> GetBitMapAsync(this ImageSource source)
        {
            return source?.GetHandler()?.LoadImageAsync(source, Xamarin.Essentials.Platform.CurrentActivity);
        }

        public static Bitmap ScaleTo(this Bitmap bitmap, int maxHeight, int maxWidth)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;

            if (width > height)
            {
                float ratio = (float)width / maxWidth;
                width = maxWidth;
                height = (int)(height / ratio);
            }
            else if (height > width)
            {
                float ratio = (float)height / maxHeight;
                height = maxHeight;
                width = (int)(width / ratio);
            }
            else
            {
                width = maxWidth;
                height = maxHeight;
            }

            return Bitmap.CreateScaledBitmap(bitmap, width, height, true);
        }
    }
}