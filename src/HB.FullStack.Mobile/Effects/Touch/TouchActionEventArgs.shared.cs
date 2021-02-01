using System;

using Xamarin.Forms;

namespace HB.FullStack.Mobile.Effects.Touch
{
    public class TouchActionEventArgs : EventArgs
    {
        public TouchActionEventArgs(long id, TouchActionType type, Point location, bool isInContact)
        {
            Id = id;
            Type = type;
            Location = location;
            IsInContact = isInContact;
        }

        /// <summary>
        /// 第几个指头
        /// </summary>
        public long Id { private set; get; }

        public TouchActionType Type { private set; get; }

        /// <summary>
        /// 由底层传来的以左上角为原点的像素坐标
        /// </summary>
        public Point Location { private set; get; }

        public bool IsInContact { private set; get; }
    }
}
