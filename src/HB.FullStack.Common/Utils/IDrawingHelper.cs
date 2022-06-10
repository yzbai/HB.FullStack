

using System.IO;

namespace System
{
    public interface IDrawingHelper
    {
        void WriteImageToStream(Stream target, string imageContentType, int width, int height, string code);
    }
}

