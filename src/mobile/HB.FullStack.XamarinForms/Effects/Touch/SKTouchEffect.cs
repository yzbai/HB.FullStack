using System;

using Xamarin.Forms;

namespace HB.FullStack.XamarinForms.Effects.Touch
{
    public class SKTouchEffect : RoutingEffect
    {
        private readonly WeakEventManager _weakEventManager = new WeakEventManager();

        public event EventHandler<TouchActionEventArgs>? TouchAction
        {
            add => _weakEventManager.AddEventHandler(value, nameof(TouchAction));
            remove => _weakEventManager.RemoveEventHandler(value, nameof(TouchAction));
        }

        /// <summary>
        /// 控制是否启用
        /// </summary>
        public bool Enable { get; set; } = true;

        public bool EnableTouchEventPropagation { get; set; }

        public SKTouchEffect() : base($"{Consts.EffectsGroupName}.{nameof(SKTouchEffect)}")
        {
        }

        public void OnTouchAction(Element element, TouchActionEventArgs args)
        {
            _weakEventManager.HandleEvent(element, args, nameof(TouchAction));
        }
    }
}