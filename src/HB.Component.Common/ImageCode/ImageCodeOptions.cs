using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Compnent.Common.ImageCode
{
    public class ImageCodeOptions : IOptions<ImageCodeOptions>
    {
        public ImageCodeOptions Value { get => this; }

        public int CodeLength { get; set; } = 6;

        public bool OnlyNumberic { get; set; } = true;

        public string ImageContentType { get; set; } = "image/png";
    }
}
