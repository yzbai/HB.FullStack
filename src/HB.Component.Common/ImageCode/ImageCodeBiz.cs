using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HB.Framework.Common.Utility;
using HB.Framework.Common;
using System.IO;

namespace HB.Compnent.Common.ImageCode
{
    public class ImageCodeBiz : IImageCodeBiz
    {
        private ImageCodeOptions _options;
        private IDrawingHelper _drawer;
        private ILogger _logger;

        public ImageCodeBiz(IOptions<ImageCodeOptions> options, IDrawingHelper drawingHelper, ILogger<ImageCodeBiz> logger) 
        {
            _options = options.Value;
            _drawer = drawingHelper;
            _logger = logger;
        }

        public ImageCodeResult Create(int width, int height)
        {
            ImageCodeResult result = new ImageCodeResult();

            result.Code = _options.OnlyNumberic ? SecurityHelper.CreateRandomNumbericString(_options.CodeLength) : SecurityHelper.CreateRandomString(_options.CodeLength);
            result.ContentType = _options.ImageContentType;
            result.Image = getImageBytes(result.Code, width, height);


            return result;
        }

        private byte[] getImageBytes(string text, int width, int height)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                _drawer.WriteImageToStream(stream, _options.ImageContentType, width, height, text);

                return stream.ToArray();
            }
        }
    }
}
