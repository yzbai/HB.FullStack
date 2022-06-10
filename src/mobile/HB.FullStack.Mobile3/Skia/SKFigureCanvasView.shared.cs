using HB.FullStack.Mobile.Base;
using HB.FullStack.Mobile.Effects;
using Microsoft.Extensions.Logging;
using SkiaSharp;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using Xamarin.Forms;
using System.Linq;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using SkiaSharp.Views.Forms;
using Xamarin.CommunityToolkit.ObjectModel;

namespace HB.FullStack.Mobile.Skia
{
    enum InvalidateSurfaceType
    {
        ByTouch,
        ByTimeTick
    }

    [SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "当Page Disappearing时，会调用所有BaseContentView的Disappering。那里会dispose")]
    public class SKFigureCanvasView : SKCanvasView, IBaseContentView
    {
        public static readonly BindableProperty FiguresProperty = BindableProperty.Create(
            nameof(Figures),
            typeof(IList<SKFigure>),
            typeof(SKFigureCanvasView),
            defaultValue: null,
            defaultValueCreator: b =>
            {
                ObservableRangeCollection<SKFigure> figures = new ObservableRangeCollection<SKFigure>();

                figures.CollectionChanged += ((SKFigureCanvasView)b).OnFiguresCollectionChanged;

                return figures;
            },
            propertyChanged: (b, o, n) => { ((SKFigureCanvasView)b).OnFiguresChanged((IList<SKFigure>?)o, (IList<SKFigure>?)n); });

        public static readonly BindableProperty EnableTimeTickProperty = BindableProperty.Create(
            nameof(EnableTimeTick),
            typeof(bool),
            typeof(SKFigureCanvasView),
            false,
            propertyChanged: (b, o, n) => { ((SKFigureCanvasView)b).OnEnableTimeTickChanged((bool)o, (bool)n); });

        public static readonly BindableProperty TimeTickIntervalsProperty = BindableProperty.Create(
            nameof(TimeTickIntervals),
            typeof(TimeSpan),
            typeof(SKFigureCanvasView),
            TimeSpan.FromMilliseconds(16),
            propertyChanged: (b, o, n) => { ((SKFigureCanvasView)b).OnTimeTickIntervalChanged(); });

        private readonly WeakEventManager _eventManager = new WeakEventManager();
        private readonly Dictionary<long, SKFigure> _fingerFigureDict = new Dictionary<long, SKFigure>();
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private Timer? _timer;

        public IList<SKFigure> Figures { get => (IList<SKFigure>)GetValue(FiguresProperty); set => SetValue(FiguresProperty, value); }

        public bool EnableTimeTick { get => (bool)GetValue(EnableTimeTickProperty); set => SetValue(EnableTimeTickProperty, value); }

        public TimeSpan TimeTickIntervals { get => (TimeSpan)GetValue(TimeTickIntervalsProperty); set => SetValue(TimeTickIntervalsProperty, value); }

        public long ElapsedMilliseconds { get => _stopwatch.ElapsedMilliseconds; }

        public bool IsAppearing { get; private set; }

        public bool AutoBringToFront { get; set; } = true;

        public SKFigureCanvasView() : base()
        {
            //Touch
            EnableTouchEvents = true;
            Touch += OnTouch;

            //Paint
            PaintSurface += OnPaintSurface;
        }

        public void OnAppearing()
        {
            GlobalSettings.Logger.LogDebug("SKFigureCanvasView即将显示. Type: {type}", this.GetType().Name);

            IsAppearing = true;

            if (EnableTimeTick)
            {
                GlobalSettings.Logger.LogDebug("调用ResumeResponseTimeTick， Place {pos}", 2);
                ResumeResponseTimeTick();
            }

            GlobalSettings.Logger.LogDebug("SKFigureCanvasView 结束 即将显示. Type: {type}", this.GetType().Name);
        }

        public void OnDisappearing()
        {
            IsAppearing = false;

            StopResponseTimeTick();

            _fingerFigureDict.Clear();
        }

        public IList<IBaseContentView?>? GetAllCustomerControls() => null;

        #region OnFigures

        private void OnFiguresChanged(IList<SKFigure>? oldValues, IList<SKFigure>? newValues)
        {
            StopResponseTimeTick();

            if (oldValues != null)
            {
                foreach (var f in oldValues)
                {
                    f.Dispose();
                }
            }

            if (newValues != null)
            {
                SetSKFigureParent(newValues, this);
            }

            if (oldValues is ObservableCollection<SKFigure> oldCollection)
            {
                oldCollection.CollectionChanged -= OnFiguresCollectionChanged;
            }

            if (newValues is ObservableCollection<SKFigure> newCollection)
            {
                newCollection.CollectionChanged += OnFiguresCollectionChanged;
            }

            //TODO: 调查可能与OnFiguresCollectionChanged里的重画 重复
            if (EnableTimeTick)
            {
                GlobalSettings.Logger.LogDebug("调用ResumeResponseTimeTick， Place {pos}", 3);
                ResumeResponseTimeTick();
            }
            else
            {
                InvalidateSurface();
            }
        }

        private void OnFiguresCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    SetSKFigureParent(e.NewItems, this);
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    SetSKFigureParent(e.OldItems, null);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    SetSKFigureParent(e.OldItems, null);
                    SetSKFigureParent(e.NewItems, this);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
            }

            if (EnableTimeTick)
            {
                //GlobalSettings.Logger.LogDebug("调用ResumeResponseTimeTick， Place {pos}", 4);
                //ResumeResponseTimeTick();
            }
            else
            {
                InvalidateSurface();
            }
        }

        private static void SetSKFigureParent(IEnumerable? list, SKFigureCanvasView? canvas)
        {
            if (list == null)
            {
                return;
            }

            foreach (object f in list)
            {
                if (f is SKFigure figure)
                {
                    figure.Parent = canvas;
                    figure.CanvasView = canvas;
                }
            }
        }

        #endregion

        #region OnTimeTick

        private void OnEnableTimeTickChanged(bool oldValue, bool newValue)
        {
            GlobalSettings.Logger.LogDebug("SKFigureCanvasView EnableTimeTick 改变 from {old} to {new}", oldValue, newValue);

            if (oldValue == newValue)
            {
                GlobalSettings.Logger.LogDebug("SKFigureCanvasView EnableTimeTick 与上次改变相同，直接返回 from {old} to {new}", oldValue, newValue);
                return;
            }

            if (newValue && IsAppearing)
            {
                GlobalSettings.Logger.LogDebug("调用ResumeResponseTimeTick， Place {pos}", 1);
                ResumeResponseTimeTick();
            }
            else if (!newValue)
            {
                StopResponseTimeTick();
            }
        }

        private void OnTimeTickIntervalChanged()
        {
            GlobalSettings.Logger.LogDebug("调用ResumeResponseTimeTick， Place {pos}", 6);
            ResumeResponseTimeTick();
        }

        private void ResumeResponseTimeTick()
        {
            if (!EnableTimeTick)
            {
                return;
            }

            GlobalSettings.Logger.LogDebug("SKFigureCanvasView开启TimeTick模式");

            _stopwatch.Restart();
            //IsResponsingTimeTick = true;

            _timer?.Dispose();

            _timer = new Timer(
                new TimerCallback(state => Device.BeginInvokeOnMainThread(() => InvalidateSurface())),
                null,
                TimeSpan.Zero,
                TimeTickIntervals);
        }

        private void StopResponseTimeTick()
        {
            _timer?.Dispose();
            _timer = null;

            //IsResponsingTimeTick = false;
            _stopwatch.Stop();

            GlobalSettings.Logger.LogDebug("SKFigureCanvasView关闭TimeTick模式");
        }

        #endregion

        #region OnTouch

        private void OnTouch(object sender, SKTouchEventArgs args)
        {
            //GlobalSettings.Logger.LogDebug($"HHHHHHHHHHHHHH:{SerializeUtil.ToJson(args)}");

            if (Figures.IsNullOrEmpty())
            {
                return;
            }

            SKPoint location = args.Location; //SKUtil.ToSKPoint(args.DpLocation);

            SKFigure? relatedFigure = null;

            //能找到这个指头对应的Figure
            if (_fingerFigureDict.ContainsKey(args.Id))
            {
                relatedFigure = _fingerFigureDict[args.Id];
            }

            switch (args.ActionType)
            {
                case SKTouchAction.Pressed:

                    if (relatedFigure != null)
                    {
                        _fingerFigureDict.Remove(args.Id);

                        BaseApplication.LogError("不应该到这里：_fingerFigureDict没有清除前一个相关Touch事件");

                        return;
                    }

                    bool founded = false;

                    for (int i = Figures.Count - 1; i >= 0; --i)
                    {
                        SKFigure figure = Figures.ElementAt(i);

                        if (!founded && figure.EnableTouch && figure.OnHitTest(location, args.Id))
                        {
                            founded = true;

                            _fingerFigureDict.Add(args.Id, figure);

                            figure.ProcessTouchAction(args);

                            if (AutoBringToFront)
                            {
                                if (Figures.Remove(figure))
                                {
                                    Figures.Add(figure);
                                }
                            }
                        }
                        else
                        {
                            //TouchActionEventArgs unTouchArgs = new TouchActionEventArgs(args.Id, TouchActionType.HitFailed, args.Location, args.IsInContact);
                            figure.ProcessUnTouchAction(args.Id, location);
                        }
                    }

                    //if (!EnableTimeTick)
                    {
                        InvalidateSurface();
                    }

                    break;
                case SKTouchAction.Moved:

                    if (relatedFigure != null)
                    {
                        relatedFigure.ProcessTouchAction(args);

                        //if (!EnableTimeTick)
                        {
                            InvalidateSurface();
                        }
                    }
                    break;
                case SKTouchAction.Released:
                case SKTouchAction.Exited:
                case SKTouchAction.Cancelled:
                    if (relatedFigure != null)
                    {
                        relatedFigure.ProcessTouchAction(args);

                        _fingerFigureDict.Remove(args.Id);

                        //if (!EnableTimeTick)
                        {
                            InvalidateSurface();
                        }
                    }

                    break;
                default:
                    break;
            }
        }

        #endregion

        #region Paint

        public event EventHandler<SKPaintSurfaceEventArgs> Painting
        {
            add => _eventManager.AddEventHandler(value);
            remove => _eventManager.RemoveEventHandler(value);
        }

        public event EventHandler<SKPaintSurfaceEventArgs> Painted
        {
            add => _eventManager.AddEventHandler(value);
            remove => _eventManager.AddEventHandler(value);
        }

        private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            SKCanvas canvas = e.Surface.Canvas;

            canvas.Clear();

            OnPainting(sender, e);

            OnPaintFigures(e, canvas);

            OnPainted(sender, e);
        }

        private void OnPainting(object? sender, SKPaintSurfaceEventArgs e)
        {
            SKCanvas canvas = e.Surface.Canvas;

            using (new SKAutoCanvasRestore(canvas))
            {
                _eventManager.HandleEvent(sender, e, nameof(Painting));
            }
        }

        private void OnPaintFigures(SKPaintSurfaceEventArgs e, SKCanvas canvas)
        {
            foreach (var f in Figures)
            {
                using (new SKAutoCanvasRestore(canvas))
                {
                    f.OnPaint(e);
                }
            }
        }

        private void OnPainted(object? sender, SKPaintSurfaceEventArgs e)
        {
            SKCanvas canvas = e.Surface.Canvas;

            using (new SKAutoCanvasRestore(canvas))
            {
                _eventManager.HandleEvent(sender, e, nameof(Painted));
            }
        }

        #endregion
    }
}
