using HB.Framework.Common.Utility;
using HB.Infrastructure.Magick;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MagickServiceCollectionExtensions
    {
        public static IServiceCollection AddImageMagick(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IDrawingHelper, DrawingHelper>();
            return serviceCollection;
        }
    }
}
