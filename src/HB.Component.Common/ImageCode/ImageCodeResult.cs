using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Compnent.Common.ImageCode
{
    public class ImageCodeResult
    {
        public string Code { get; set; }

        public string ContentType { get; set; }

        public byte[] Image { get; set; }
    }
}
