
using SkiaSharp;

namespace HB.FullStack.XamarinForms.Controls.Clock
{
    public class TimeBlockDrawInfo
    {
        public SKColor Color { get; set; }

        public Time24Hour StartTime { get; set; }

        public Time24Hour EndTime { get; set; }
    }
}