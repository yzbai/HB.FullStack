using System;
using System.Collections.Generic;
using System.Linq;
using Android.Views;
using HB.Framework.Client.Effects;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportEffect(typeof(HB.Framework.Client.Droid.Effects.TouchEffect), nameof(HB.Framework.Client.Effects.TouchEffect))]

namespace HB.Framework.Client.Droid.Effects
{
    public class TouchEffect : PlatformEffect
    {
        Android.Views.View _view;
        Element _formsElement;
        HB.Framework.Client.Effects.TouchEffect _libTouchEffect;
        bool _capture;
        Func<double, double> _fromPixels;
        readonly int[] _twoIntArray = new int[2];

        static readonly Dictionary<Android.Views.View, TouchEffect> _viewDictionary = new Dictionary<Android.Views.View, TouchEffect>();

        static readonly Dictionary<int, TouchEffect> _idToEffectDictionary = new Dictionary<int, TouchEffect>();

        protected override void OnAttached()
        {
            // Get the Android View corresponding to the Element that the effect is attached to
            _view = Control ?? Container;

            // Get access to the TouchEffect class in the .NET Standard library
            HB.Framework.Client.Effects.TouchEffect touchEffect =
                (HB.Framework.Client.Effects.TouchEffect)Element.Effects.
                    FirstOrDefault(e => e is HB.Framework.Client.Effects.TouchEffect);

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
            if (_viewDictionary.ContainsKey(_view))
            {
                _viewDictionary.Remove(_view);
                _view.Touch -= OnTouch;
            }
        }

        void OnTouch(object sender, Android.Views.View.TouchEventArgs args)
        {
            // Two object common to all the events
            Android.Views.View senderView = sender as Android.Views.View;
            MotionEvent motionEvent = args.Event;

            // Get the pointer index
            int pointerIndex = motionEvent.ActionIndex;

            // Get the id that identifies a finger over the course of its progress
            int id = motionEvent.GetPointerId(pointerIndex);


            senderView.GetLocationOnScreen(_twoIntArray);
            Point screenPointerCoords = new Point(_twoIntArray[0] + motionEvent.GetX(pointerIndex),
                                                  _twoIntArray[1] + motionEvent.GetY(pointerIndex));


            //Log.Debug("HB.MyColorfulTime", $"Android : {args.Event.ActionMasked}, Id : {id}, at : {screenPointerCoords}");


            // Use ActionMasked here rather than Action to reduce the number of possibilities
            switch (args.Event.ActionMasked)
            {
                case MotionEventActions.Down:
                case MotionEventActions.PointerDown:
                    FireEvent(this, id, TouchActionType.Pressed, screenPointerCoords, true);

                    _idToEffectDictionary.Add(id, this);

                    _capture = _libTouchEffect.Capture;
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
                    _idToEffectDictionary.Remove(id);
                    break;
            }
        }

        void CheckForBoundaryHop(int id, Point pointerLocation)
        {
            TouchEffect touchEffectHit = null;

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
                _idToEffectDictionary[id] = touchEffectHit;
            }
        }

        void FireEvent(TouchEffect touchEffect, int id, TouchActionType actionType, Point pointerLocation, bool isInContact)
        {
            // Get the method to call for firing events
            Action<Element, TouchActionEventArgs> onTouchAction = touchEffect._libTouchEffect.OnTouchAction;

            // Get the location of the pointer within the view
            touchEffect._view.GetLocationOnScreen(_twoIntArray);
            double x = pointerLocation.X - _twoIntArray[0];
            double y = pointerLocation.Y - _twoIntArray[1];
            Point point = new Point(_fromPixels(x), _fromPixels(y));

            // Call the method
            onTouchAction(touchEffect._formsElement,
                new TouchActionEventArgs(id, actionType, point, isInContact));
        }
    }
}
