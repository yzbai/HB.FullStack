using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HB.FullStack.Mobile.Extensions
{

    [ContentProperty(nameof(Source))]
    public class ImageResourceMarkupExtension : IMarkupExtension<ImageSource>
    {
        public string? Source { get; set; }

        /// <summary>
        /// ProvideValue
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Ignore.</exception>
        public ImageSource ProvideValue(IServiceProvider serviceProvider)
        {
            if (Source.IsNotNullOrEmpty())
            {
                if (!Uri.TryCreate(Source, UriKind.Absolute, out Uri result) || !(result.Scheme != "file"))
                {
                    return ImageSource.FromFile(Source);
                }

                return new AuthUriImageSource { Uri = result };
            }

            throw new InvalidOperationException($"Cannot convert \"{Source}\" into {typeof(ImageSource)}");
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        {
            return ProvideValue(serviceProvider);
        }
    }
}
