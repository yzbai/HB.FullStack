using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HB.Framework.Common.Utility
{
    public interface IDrawingHelper
    {
        void WriteImageToStream(Stream target, string imageContentType, int width, int height, string code);
    }
}
