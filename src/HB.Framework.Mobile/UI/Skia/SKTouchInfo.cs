using SkiaSharp;
using System.Diagnostics;
using System.Threading.Tasks;

namespace HB.Framework.Client.UI.Skia
{
    public class SKTouchInfo
    {
        public long TouchEventId { get; set; }

        public bool IsOver { get; set; }

        public SKPoint StartPoint { get; set; }

        public SKPoint PreviousPoint { get; set; }

        public SKPoint CurrentPoint { get; set; }

        public bool LongPressHappend { get; set; }

        public Task? LongPressTask { get; set; }
    }
}
