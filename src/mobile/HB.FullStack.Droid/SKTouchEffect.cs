#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

using Android.Views;

using HB.FullStack.XamarinForms.Effects.Touch;

using Microsoft.Extensions.Logging;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportEffect(typeof(HB.FullStack.Droid.Effects.SKTouchEffect), nameof(HB.FullStack.XamarinForms.Effects.Touch.SKTouchEffect))]

namespace HB.FullStack.Droid.Effects
{
    public class SKTouchEffect : PlatformEffect
    {
        Android.Views.View? _view;
        Element? _formsElement;
        HB.FullStack.XamarinForms.Effects.Touch.SKTouchEffect? _libTouchEffect;
        Func<double, double>? _fromPixels;
        readonly int[] _twoIntArray = new int[2];

        protected override void OnAttached()
        {
            // Get the Android View corresponding to the Element that the effect is attached to
            _view = Control ?? Container;

            // Get access to the TouchEffect class in the .NET Standard library
            HB.FullStack.XamarinForms.Effects.Touch.SKTouchEffect touchEffect =
                (HB.FullStack.XamarinForms.Effects.Touch.SKTouchEffect)Element.Effects.
                    FirstOrDefault(e => e is HB.FullStack.XamarinForms.Effects.Touch.SKTouchEffect);

            if (touchEffect != null && _view != null)
            {
                _formsElement = Element;

                _libTouchEffect = touchEffect;

                // Save fromPixels function
                _fromPixels = _view.Context.FromPixels;

                // Set event handler on View
                _view.Touch += OnTouch;
            }
        }

        protected override void OnDetached()
        {
            if (_view != null)
            {
                _view.Touch -= OnTouch;
            }
        }

        void OnTouch(object sender, Android.Views.View.TouchEventArgs args)
        {
            if(_libTouchEffect!.EnableTouchEventPropagation)
            {
                args.Handled = false;
            }

            if (!_libTouchEffect!.Enable)
            {
                return;
            }

            GlobalSettings.Logger.LogDebug($"在安卓中， sender: {sender.GetType().Name}");

            // Two object common to all the events
            Android.Views.View? senderView = sender as Android.Views.View;
            MotionEvent? motionEvent = args.Event;

            if (senderView == null || motionEvent == null)
            {
                return;
            }

            // Get the pointer index
            int pointerIndex = motionEvent.ActionIndex;

            // Get the id that identifies a finger over the course of its progress
            int id = motionEvent.GetPointerId(pointerIndex);


            senderView.GetLocationOnScreen(_twoIntArray);
            Point screenPointerCoords = new Point(_twoIntArray[0] + motionEvent.GetX(pointerIndex),
                                                  _twoIntArray[1] + motionEvent.GetY(pointerIndex));


            GlobalSettings.Logger.LogDebug($"在安卓中， sender: {sender.GetType().Name}, Touch Events: {motionEvent.ActionMasked}, Id : {id}, at : {screenPointerCoords}");

            // Use ActionMasked here rather than Action to reduce the number of possibilities
            switch (motionEvent.ActionMasked)
            {
                case MotionEventActions.Down:
                case MotionEventActions.PointerDown:
                    
                    args.Handled = FireEvent(this, id, TouchActionType.Pressed, screenPointerCoords, true);

                    GlobalSettings.Logger.LogDebug($"在安卓中 加入Dict, {id}:{sender.GetType().Name}");

                    break;

                case MotionEventActions.Move:
                    // Multiple Move events are bundled, so handle them in a loop
                    for (pointerIndex = 0; pointerIndex < motionEvent.PointerCount; pointerIndex++)
                    {
                        id = motionEvent.GetPointerId(pointerIndex);

                        senderView.GetLocationOnScreen(_twoIntArray);

                        screenPointerCoords = new Point(_twoIntArray[0] + motionEvent.GetX(pointerIndex),
                                                        _twoIntArray[1] + motionEvent.GetY(pointerIndex));

                        args.Handled = FireEvent(this, id, TouchActionType.Moved, screenPointerCoords, true);
                    }
                    break;

                case MotionEventActions.Up:
                case MotionEventActions.Pointer1Up:
                    args.Handled = FireEvent(this, id, TouchActionType.Released, screenPointerCoords, false);
                    GlobalSettings.Logger.LogDebug($"在安卓中 移除Dict, {id}:{sender.GetType().Name}");
                    break;

                case MotionEventActions.Cancel:
                    args.Handled = FireEvent(this, id, TouchActionType.Cancelled, screenPointerCoords, false);
                    GlobalSettings.Logger.LogDebug($"在安卓中 加入Dict, {id}:{sender.GetType().Name}");
                    break;
            }
        }

        bool FireEvent(SKTouchEffect touchEffect, int id, TouchActionType actionType, Point pointerLocation, bool isInContact)
        {
            // Get the method to call for firing events
            Action<Element, TouchActionEventArgs> onTouchAction = touchEffect._libTouchEffect!.OnTouchAction;

            // Get the location of the pointer within the view
            touchEffect._view!.GetLocationOnScreen(_twoIntArray);
            double x = pointerLocation.X - _twoIntArray[0];
            double y = pointerLocation.Y - _twoIntArray[1];
            Point point = new Point(_fromPixels!(x), _fromPixels(y));

            // Call the method
            TouchActionEventArgs args = new TouchActionEventArgs(id, actionType, point, isInContact);

            onTouchAction(touchEffect._formsElement!, args);
            
            return args.IsHandled;
        }
    }
}
#nullable restore