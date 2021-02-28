using System;

using Xamarin.Forms;

namespace HB.FullStack.XamarinForms.Effects.Touch
{
    public class TouchEffect : RoutingEffect
    {
        private readonly WeakEventManager _weakEventManager = new WeakEventManager();

        public event EventHandler<TouchActionEventArgs>? TouchAction
        {
            add => _weakEventManager.AddEventHandler(value, nameof(TouchAction));
            remove => _weakEventManager.RemoveEventHandler(value, nameof(TouchAction));
        }

        /// <summary>
        /// true:表示，即使move出控件范围，依然捕捉move事件，适合挪动控件位置。false表示：当move出控件范围时，就激发Exit事件。
        /// </summary>
        public bool Capture { set; get; } = true;

        /// <summary>
        /// 控制是否启用
        /// </summary>
        public bool Enable { get; set; } = true;

        public TouchEffect() : base($"{Consts.EffectsGroupName}.{nameof(TouchEffect)}")
        {
        }

        public void OnTouchAction(Element element, TouchActionEventArgs args)
        {
            _weakEventManager.HandleEvent(element, args, nameof(TouchAction));
        }
    }
}