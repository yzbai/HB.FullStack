#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

using Android.Views;

using HB.FullStack.XamarinForms.Effects.Touch;

using Microsoft.Extensions.Logging;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportEffect(typeof(HB.FullStack.XamarinForms.Droid.Effects.TouchEffect), nameof(HB.FullStack.XamarinForms.Effects.Touch.TouchEffect))]

namespace HB.FullStack.XamarinForms.Droid.Effects
{
    public class TouchEffect : PlatformEffect
    {
        Android.Views.View? _view;
        Element? _formsElement;
        HB.FullStack.XamarinForms.Effects.Touch.TouchEffect? _libTouchEffect;
        bool _capture;
        Func<double, double>? _fromPixels;
        readonly int[] _twoIntArray = new int[2];

        static readonly Dictionary<Android.Views.View, TouchEffect> _viewDictionary = new Dictionary<Android.Views.View, TouchEffect>();

        static readonly Dictionary<int, TouchEffect> _idToEffectDictionary = new Dictionary<int, TouchEffect>();

        protected override void OnAttached()
        {
            // Get the Android View corresponding to the Element that the effect is attached to
            _view = Control ?? Container;

            // Get access to the TouchEffect class in the .NET Standard library
            HB.FullStack.XamarinForms.Effects.Touch.TouchEffect touchEffect =
                (HB.FullStack.XamarinForms.Effects.Touch.TouchEffect)Element.Effects.
                    FirstOrDefault(e => e is HB.FullStack.XamarinForms.Effects.Touch.TouchEffect);

            if (touchEffect != null && _view != null)
            {
                _viewDictionary.Add(_view, this);

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
            if (_view != null && _viewDictionary.ContainsKey(_view))
            {
                _viewDictionary.Remove(_view);
                _view.Touch -= OnTouch;
            }
        }

        void OnTouch(object sender, Android.Views.View.TouchEventArgs args)
        {
            if (!_libTouchEffect!.Enable)
            {
                return;
            }

            GlobalSettings.Logger.LogDebug($"在安卓中， sender: {sender.GetType().Name}");

            // Two object common to all the events
            MotionEvent? motionEvent = args.Event;

            if (!(sender is Android.Views.View senderView) || motionEvent == null)
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
                    FireEvent(this, id, TouchActionType.Pressed, screenPointerCoords, true);

                    GlobalSettings.Logger.LogDebug($"在安卓中 加入Dict, {id}:{sender.GetType().Name}");

                    _idToEffectDictionary.Add(id, this);

                    _capture = _libTouchEffect!.Capture;
                    break;

                case MotionEventActions.Move:
                    // Multiple Move events are bundled, so handle them in a loop
                    for (pointerIndex = 0; pointerIndex < motionEvent.PointerCount; pointerIndex++)
                    {
                        id = motionEvent.GetPointerId(pointerIndex);

                        if (_capture)
                        {
                            senderView.GetLocationOnScreen(_twoIntArray);

                            screenPointerCoords = new Point(_twoIntArray[0] + motionEvent.GetX(pointerIndex),
                                                            _twoIntArray[1] + motionEvent.GetY(pointerIndex));

                            FireEvent(this, id, TouchActionType.Moved, screenPointerCoords, true);
                        }
                        else
                        {
                            CheckForBoundaryHop(id, screenPointerCoords);

                            if (_idToEffectDictionary[id] != null)
                            {
                                FireEvent(_idToEffectDictionary[id], id, TouchActionType.Moved, screenPointerCoords, true);
                            }
                        }
                    }
                    break;

                case MotionEventActions.Up:
                case MotionEventActions.Pointer1Up:
                    if (_capture)
                    {
                        FireEvent(this, id, TouchActionType.Released, screenPointerCoords, false);
                    }
                    else
                    {
                        CheckForBoundaryHop(id, screenPointerCoords);

                        if (_idToEffectDictionary[id] != null)
                        {
                            FireEvent(_idToEffectDictionary[id], id, TouchActionType.Released, screenPointerCoords, false);
                        }
                    }
                    GlobalSettings.Logger.LogDebug($"在安卓中 移除Dict, {id}:{sender.GetType().Name}");
                    _idToEffectDictionary.Remove(id);
                    break;

                case MotionEventActions.Cancel:
                    if (_capture)
                    {
                        FireEvent(this, id, TouchActionType.Cancelled, screenPointerCoords, false);
                    }
                    else
                    {
                        if (_idToEffectDictionary[id] != null)
                        {
                            FireEvent(_idToEffectDictionary[id], id, TouchActionType.Cancelled, screenPointerCoords, false);
                        }
                    }
                    GlobalSettings.Logger.LogDebug($"在安卓中 加入Dict, {id}:{sender.GetType().Name}");
                    _idToEffectDictionary.Remove(id);
                    break;
            }
        }

        void CheckForBoundaryHop(int id, Point pointerLocation)
        {
            TouchEffect? touchEffectHit = null;

            foreach (Android.Views.View view in _viewDictionary.Keys)
            {
                // Get the view rectangle
                try
                {
                    view.GetLocationOnScreen(_twoIntArray);
                }
                catch // System.ObjectDisposedException: Cannot access a disposed object.
                {
                    continue;
                }
                Rectangle viewRect = new Rectangle(_twoIntArray[0], _twoIntArray[1], view.Width, view.Height);

                if (viewRect.Contains(pointerLocation))
                {
                    touchEffectHit = _viewDictionary[view];
                }
            }

            if (touchEffectHit != _idToEffectDictionary[id])
            {
                if (_idToEffectDictionary[id] != null)
                {
                    FireEvent(_idToEffectDictionary[id], id, TouchActionType.Exited, pointerLocation, true);
                }
                if (touchEffectHit != null)
                {
                    FireEvent(touchEffectHit, id, TouchActionType.Entered, pointerLocation, true);
                }
                _idToEffectDictionary[id] = touchEffectHit!;
            }
        }

        void FireEvent(TouchEffect touchEffect, int id, TouchActionType actionType, Point pointerLocation, bool isInContact)
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
        }
    }
}
#nullable restore