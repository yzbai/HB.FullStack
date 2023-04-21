/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using Microsoft.Extensions.Logging;
using SkiaSharp;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading;
using SkiaSharp.Views.Maui.Controls;
using Microsoft.Maui.Controls;
using HB.FullStack.Common;

using SkiaSharp.Views.Maui;
using Microsoft.Maui;
using System.Diagnostics.CodeAnalysis;
using HB.FullStack.Common.Figures;
using HB.FullStack.Client.MauiLib.Controls;
using HB.FullStack.Client.MauiLib.Base;

namespace HB.FullStack.Client.MauiLib.Figures
{
    internal enum InvalidateSurfaceType
    {
        ByTouch,
        ByTimeTick
    }

    [SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "在PageDisappearing中会调用_timer的Dispose")]
    public class SKFigureCanvasView : SKCanvasView, ISKFigureGroupController, IBaseContentView
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

        //储存指头与Figure的对应
        private readonly Dictionary<long, SKFigure> _fingerFigureDict = new Dictionary<long, SKFigure>();

        private readonly Stopwatch _stopwatch = new Stopwatch();
        private Timer? _timer;

        public IList<SKFigure> Figures { get => (IList<SKFigure>)GetValue(FiguresProperty); set => SetValue(FiguresProperty, value); }

        public bool EnableTimeTick { get => (bool)GetValue(EnableTimeTickProperty); set => SetValue(EnableTimeTickProperty, value); }

        public TimeSpan TimeTickIntervals { get => (TimeSpan)GetValue(TimeTickIntervalsProperty); set => SetValue(TimeTickIntervalsProperty, value); }

        public long ElapsedMilliseconds { get => _stopwatch.ElapsedMilliseconds; }

        public bool IsAppearred { get; private set; }

        public bool AutoBringToFront { get; set; } = true;

        public IList<IBaseContentView>? CustomerControls => null;

        public SKFigureCanvasView() : base()
        {
            //使用统一的长度单位，而不是px
            //IgnorePixelScaling = true;

            //Touch
            //EnableTouchEvents = true;
            Touch += OnTouch;

            //Paint
            PaintSurface += OnPaintSurface;
        }

        public void OnPageAppearing()
        {
            Globals.Logger.LogDebug("SKFigureCanvasView即将显示. Type: {type}", this.GetType().Name);

            IsAppearred = true;

            if (EnableTimeTick)
            {
                Globals.Logger.LogDebug("调用ResumeResponseTimeTick， Place {pos}", 2);
                ResumeTimeTick();
            }

            Globals.Logger.LogDebug("SKFigureCanvasView 结束 即将显示. Type: {type}", this.GetType().Name);
        }

        public void OnPageDisappearing()
        {
            IsAppearred = false;

            StopTimeTick();

            _fingerFigureDict.Clear();
        }

        #region OnFigures

        private void OnFiguresChanged(IList<SKFigure>? oldFigures, IList<SKFigure>? newFigures)
        {
            StopTimeTick();
            _groupFigures.Clear();

            if (oldFigures != null)
            {
                DisconnectFigures(oldFigures);
            }

            if (newFigures != null)
            {
                ConnectFigures(newFigures);
            }

            if (oldFigures is ObservableCollection<SKFigure> oldCollection)
            {
                oldCollection.CollectionChanged -= OnFiguresCollectionChanged;
            }

            if (newFigures is ObservableCollection<SKFigure> newCollection)
            {
                newCollection.CollectionChanged += OnFiguresCollectionChanged;
            }

            InvalidateSurface();

            if (EnableTimeTick)
            {
                Globals.Logger.LogDebug("调用ResumeResponseTimeTick， Place {pos}", 3);
                ResumeTimeTick();
            }
        }

        private void OnFiguresCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    ConnectFigures(e.NewItems);
                    break;

                case NotifyCollectionChangedAction.Move:
                    break;

                case NotifyCollectionChangedAction.Remove:
                    DisconnectFigures(e.OldItems);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    DisconnectFigures(e.OldItems);
                    ConnectFigures(e.NewItems);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    DisconnectFigures(e.OldItems);
                    break;
            }

            InvalidateSurface();
        }

        private void ConnectFigures(IEnumerable? list)
        {
            if (list == null)
            {
                return;
            }

            foreach (object item in list)
            {
                if (item is SKFigure figure)
                {
                    figure.GroupController = this;
                    figure.CanvasView = this;

                    //Add to Group
                    if (figure.GroupName.IsNotNullOrEmpty())
                    {
                        AddToGroup(figure.GroupName, figure);
                    }
                }
            }
        }

        private void DisconnectFigures(IEnumerable? list)
        {
            if (list == null)
            {
                return;
            }

            foreach (var item in list)
            {
                if (item is SKFigure figure)
                {
                    //TODO: 我们需要在这里dispose figure吗，figure需要View来管理吗
                    //figure.Dispose();

                    if (figure.GroupName.IsNotNullOrEmpty())
                    {
                        RemoveFromGroup(figure.GroupName, figure);
                    }
                }
            }
        }

        #endregion

        #region OnTimeTick

        private void OnEnableTimeTickChanged(bool oldValue, bool newValue)
        {
            Globals.Logger.LogDebug("SKFigureCanvasView EnableTimeTick 改变 from {old} to {new}", oldValue, newValue);

            if (oldValue == newValue)
            {
                Globals.Logger.LogDebug("SKFigureCanvasView EnableTimeTick 与上次改变相同，直接返回 from {old} to {new}", oldValue, newValue);
                return;
            }

            if (newValue && IsAppearred)
            {
                Globals.Logger.LogDebug("调用ResumeResponseTimeTick， Place {pos}", 1);
                ResumeTimeTick();
            }
            else if (!newValue)
            {
                StopTimeTick();
            }
        }

        private void OnTimeTickIntervalChanged()
        {
            Globals.Logger.LogDebug("调用ResumeResponseTimeTick， Place {pos}", 6);
            ResumeTimeTick();
        }

        private void ResumeTimeTick()
        {
            if (!EnableTimeTick)
            {
                return;
            }

            Globals.Logger.LogDebug("SKFigureCanvasView开启TimeTick模式");

            _stopwatch.Restart();
            //IsResponsingTimeTick = true;

            _timer?.Dispose();

            _timer = new Timer(
                new TimerCallback(state => Dispatcher.Dispatch(() => InvalidateSurface())),
                null,
                TimeSpan.Zero,
                TimeTickIntervals);
        }

        private void StopTimeTick()
        {
            _timer?.Dispose();
            _timer = null;

            //IsResponsingTimeTick = false;
            _stopwatch.Stop();

            Globals.Logger.LogDebug("SKFigureCanvasView关闭TimeTick模式");
        }

        #endregion

        #region OnTouch

        private void OnTouch(object? sender, SKTouchEventArgs args)
        {
            //Globals.Logger.LogDebug($"HHHHHHHHHHHHHH:{SerializeUtil.ToJson(args)}");

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

                        //BaseApplication.LogError("不应该到这里：_fingerFigureDict没有清除前一个相关Touch事件");

                        return;
                    }

                    bool founded = false;

                    for (int i = Figures.Count - 1; i >= 0; --i)
                    {
                        SKFigure figure = Figures[i];

                        if (!founded && figure.EnableTouch && figure.HitTest(location, args.Id))
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

                    InvalidateSurface();

                    break;

                case SKTouchAction.Moved:

                    if (relatedFigure != null)
                    {
                        relatedFigure.ProcessTouchAction(args);

                        InvalidateSurface();
                    }

                    break;

                case SKTouchAction.Released:
                case SKTouchAction.Exited:
                case SKTouchAction.Cancelled:
                    if (relatedFigure != null)
                    {
                        relatedFigure.ProcessTouchAction(args);

                        _fingerFigureDict.Remove(args.Id);

                        InvalidateSurface();
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
                _eventManager.HandleEvent(sender!, e, nameof(Painting));
            }
        }

        private void OnPaintFigures(SKPaintSurfaceEventArgs e, SKCanvas canvas)
        {
            foreach (var figure in Figures)
            {
                using (new SKAutoCanvasRestore(canvas))
                {
                    figure.OnPaint(e);
                }
            }
        }

        private void OnPainted(object? sender, SKPaintSurfaceEventArgs e)
        {
            SKCanvas canvas = e.Surface.Canvas;

            using (new SKAutoCanvasRestore(canvas))
            {
                _eventManager.HandleEvent(sender!, e, nameof(Painted));
            }
        }

        #endregion

        #region GroupController

        private Dictionary<string, List<SKFigure>> _groupFigures = new Dictionary<string, List<SKFigure>>();

        public void AddToGroup(string groupName, SKFigure figure)
        {
            figure.GroupName = groupName;

            if (_groupFigures.ContainsKey(groupName))
            {
                _groupFigures[groupName].Add(figure);
            }
            else
            {
                _groupFigures[groupName] = new List<SKFigure> { figure };
            }
        }

        public void RemoveFromGroup(string groupName, SKFigure figure)
        {
            if (!_groupFigures.ContainsKey(groupName))
            {
                return;
            }

            _groupFigures[groupName].Remove(figure);
        }

        public bool EnableMultiple { get; set; }

        public IEnumerable<SKFigure> GetFiguresByGroup(string groupName)
        {
            if (_groupFigures.TryGetValue(groupName, out List<SKFigure>? figures))
            {
                return figures;
            }

            return new List<SKFigure>();
        }

        public void SetGroupVisualState(string groupName, FigureVisualState visualState)
        {
            IEnumerable figures = GetFiguresByGroup(groupName);

            foreach (SKFigure figure in figures)
            {
                figure.VisualState = visualState;
            }
        }

        public void NotifyVisualStateChanged(SKFigure figure)
        {
        }

        #endregion
    }
}