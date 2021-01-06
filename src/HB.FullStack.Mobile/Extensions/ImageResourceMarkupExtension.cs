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

        public ImageSource ProvideValue(IServiceProvider serviceProvider)
        {
            if (Source.IsNotNullOrEmpty())
            {
                if (!Uri.TryCreate(Source, UriKind.Absolute, out Uri result) || !(result.Scheme != "file"))
                {
                    return ImageSource.FromFile(Source);
                }

                return new UriImageSourceEx { Uri = result };
            }

            throw new InvalidOperationException($"Cannot convert \"{Source}\" into {typeof(ImageSource)}");
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        {
            return ProvideValue(serviceProvider);
        }
    }
}
