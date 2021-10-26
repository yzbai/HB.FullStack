#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;

using HB.FullStack.XamarinForms.Controls;
using HB.FullStack.XamarinForms.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.Android;

[assembly: ExportImageSourceHandler(typeof(AuthUriImageSource), typeof(AuthImageSourceHandler))]

namespace HB.FullStack.XamarinForms.Droid
{
	public class AuthImageSourceHandler : IImageSourceHandler
	{
		/// <summary>
		/// LoadImageAsync
		/// </summary>
		/// <param name="imagesource"></param>
		/// <param name="context"></param>
		/// <param name="cancelationToken"></param>
		/// <returns></returns>
		/// <exception cref="OperationCanceledException"></exception>
		/// <exception cref="Exception"></exception>
		public async Task<Bitmap?> LoadImageAsync(ImageSource imagesource, Context context, CancellationToken cancelationToken = default(CancellationToken))
		{
			var imageLoader = imagesource as AuthUriImageSource;
			Bitmap? bitmap = null;
			if (imageLoader?.Uri != null)
			{
				using (Stream imageStream = await imageLoader.GetStreamAsync(cancelationToken).ConfigureAwait(false))
					bitmap = await BitmapFactory.DecodeStreamAsync(imageStream).ConfigureAwait(false);
			}

			if (bitmap == null)
			{
				Log.Warning(nameof(AuthUriImageSource), "Could not retrieve image or image data was invalid: {0}", imageLoader);
			}

			return bitmap;
		}

		public Task<IFormsAnimationDrawable> LoadImageAnimationAsync(ImageSource imagesource, Context context, CancellationToken cancelationToken = default, float scale = 1)
		{
			return FormsAnimationDrawable.LoadImageAnimationAsync(imagesource, context, cancelationToken);
		}
	}
}
#nullable restore