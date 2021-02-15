
using System;
using System.Collections.Generic;


namespace HB.FullStack.XamarinForms.Controls.Clock
{
    public enum TimeBlockSpanType
    {
        Inner,//6~17
        Outter,//0-5,18-23
        AmCross,//example, am5:00 - am7:00
        PmCross //example, pm5:00 - pm7:00
    }
}