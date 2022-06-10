using System;
using System.Collections.Generic;
using System.Text;

using HB.FullStack.Mobile.Behaviors;

using Xamarin.Forms;

namespace HB.FullStack.Mobile.Controls
{
    public class RoundImage : Image
    {
        public RoundImage()
        {
            CircleClipBehavior circleClipBehavior = new CircleClipBehavior();
            Behaviors.Add(circleClipBehavior);
        }
    }
}
