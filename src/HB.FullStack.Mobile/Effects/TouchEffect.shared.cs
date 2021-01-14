using System;
using Xamarin.Forms;

namespace HB.FullStack.Mobile.Effects
{
#pragma warning disable CA1003 // Use generic event handler instances
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
    public delegate void TouchActionEventHandler(object? sender, TouchActionEventArgs args);
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
#pragma warning restore CA1003 // Use generic event handler instances

    public class TouchEffect : RoutingEffect
    {
        //TODO: use WeakEventManager
        public event TouchActionEventHandler? TouchAction;

        public TouchEffect() : base($"{Consts.EffectsGroupName}.{nameof(TouchEffect)}")
        {
        }

        /// <summary>
        /// true:表示，即使move出控件范围，依然捕捉move事件，适合挪动控件位置。false表示：当move出控件范围时，就激发Exit事件。
        /// </summary>
        public bool Capture { set; get; }

        public void OnTouchAction(Element element, TouchActionEventArgs args)
        {
            TouchAction?.Invoke(element, args);
        }
    }

    public class TouchActionEventArgs : EventArgs
    {
        public TouchActionEventArgs(long id, TouchActionType type, Point location, bool isInContact)
        {
            Id = id;
            Type = type;
            Location = location;
            IsInContact = isInContact;
        }

        public long Id { private set; get; }

        public TouchActionType Type { private set; get; }

        public Point Location { private set; get; }

        public bool IsInContact { private set; get; }
    }

    public enum TouchActionType
    {
        Entered,
        Pressed,
        Moved,
        Released,
        Exited,
        Cancelled,

        HitFailed // hittest is false
    }
}
