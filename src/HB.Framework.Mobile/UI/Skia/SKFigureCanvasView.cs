using HB.Framework.Client.Base;
using HB.Framework.Client.Effects;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xamarin.Forms;

namespace HB.Framework.Client.UI.Skia
{
    public class SKFigureCanvasView : SKCanvasView, IBaseContentView
    {
        private readonly ILogger _logger = DependencyService.Resolve<ILogger<SKFigureCanvasView>>();

        private readonly List<SKFigure> _figures = new List<SKFigure>();

        private readonly Dictionary<long, SKFigure> _touchDictionary = new Dictionary<long, SKFigure>();

        public bool AutoBringToFront { get; set; } = true;

        public bool EnableFailedToHitEvent { get; set; } = true;

        private bool _timerMode;

        private bool _isAnimating;

        private readonly Stopwatch _stopwatch = new Stopwatch();

        private int _intervalMilliseconds = 16;

        public long ElapsedMilliseconds { get => _stopwatch.ElapsedMilliseconds; }

        public SKFigureCanvasView() : base()
        {
            TouchEffect touchEffect = new TouchEffect
            {
                //TODO: 测试这个
                Capture = true
            };

            touchEffect.TouchAction += TouchEffect_TouchAction;

            Effects.Add(touchEffect);

            PaintSurface += FigureCanvasView_PaintSurface;
        }

        public void OnAppearing()
        {
            if (_timerMode)
            {
                _isAnimating = true;
                _stopwatch.Start();

                Device.StartTimer(
                    TimeSpan.FromMilliseconds(_intervalMilliseconds),
                    () =>
                    {
                        Device.BeginInvokeOnMainThread(() => { InvalidateSurface(); });
                        return _isAnimating;
                    });
            }
        }

        public void OnDisappearing()
        {
            if (_timerMode)
            {
                _isAnimating = false;
                _stopwatch.Stop();
            }
        }

        public void SetReDrawTimer(int milliseconds)
        {
            _timerMode = true;

            _intervalMilliseconds = milliseconds;

            Device.StartTimer(TimeSpan.FromMilliseconds(milliseconds), () =>
            {
                InvalidateSurface();
                return true;
            });
        }

        /// <summary>
        /// 按顺序添加，最后添加的显示在最上面
        /// </summary>
        /// <param name="figure"></param>
        public void AddFigure(SKFigure figure)
        {
            figure.Parent = this;
            _figures.Add(figure);
        }

        public bool RemoveFigure(SKFigure figure)
        {
            if (figure is IDisposable disposable)
            {
                disposable.Dispose();
            }

            return _figures.Remove(figure);
        }

        public void ClearFigure()
        {
            foreach (SKFigure figure in _figures)
            {
                if (figure is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            _figures.Clear();
        }


        //TODO: WeakManager 改造
        /// <summary>
        /// 在Painting 事件中不可以再直接或者间接调用InValidateSurface，会引起循环调用
        /// </summary>
        public event EventHandler<SKPaintSurfaceEventArgs>? Painting;

        /// <summary>
        /// Painted 事件中不可以再直接或者间接调用InValidateSurface，会引起循环调用
        /// </summary>
        public event EventHandler<SKPaintSurfaceEventArgs>? Painted;

        private void FigureCanvasView_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            SKCanvas canvas = e.Surface.Canvas;

            canvas.Clear();

            OnPainting(sender, e);

            OnPaintFigures(e, canvas);

            OnPainted(sender, e);
        }

        public void OnPainting(object sender, SKPaintSurfaceEventArgs e)
        {
            SKCanvas canvas = e.Surface.Canvas;

            using (new SKAutoCanvasRestore(canvas))
            {
                Painting?.Invoke(sender, e);
            }
        }
        private void OnPaintFigures(SKPaintSurfaceEventArgs e, SKCanvas canvas)
        {
            foreach (SKFigure figure in _figures)
            {
                using (new SKAutoCanvasRestore(canvas))
                {
                    figure.Paint(e);
                }
            }
        }

        public void OnPainted(object sender, SKPaintSurfaceEventArgs e)
        {
            SKCanvas canvas = e.Surface.Canvas;

            using (new SKAutoCanvasRestore(canvas))
            {
                Painted?.Invoke(sender, e);
            }
        }

        private void TouchEffect_TouchAction(object sender, TouchActionEventArgs args)
        {
            //_logger.LogDebug($"{args.Type}. ID: {args.Id},  Location:{args.Location}");

            SKPoint skPoint = SKUtil.ToSKPoint(args.Location);

            long eventId = args.Id;

            SKFigure? relatedFigure = null;

            if (_touchDictionary.ContainsKey(eventId))
            {
                relatedFigure = _touchDictionary[eventId];
            }

            switch (args.Type)
            {
                case TouchActionType.Pressed:

                    if (relatedFigure != null)
                    {
                        _logger.LogWarning($"Wired in TouchAction, eventId {eventId} already exists, but TouchActionType is Pressed.");

                        _touchDictionary.Remove(eventId);

                        return;
                    }

                    bool founded = false;

                    for (int i = _figures.Count - 1; i >= 0; --i)
                    {
                        SKFigure figure = _figures[i];

                        if (!founded && figure.HitTest(skPoint, args.Id))
                        {
                            founded = true;

                            _touchDictionary.Add(eventId, figure);

                            figure.ProcessTouchAction(args);

                            if (AutoBringToFront)
                            {
                                if (_figures.Remove(figure))
                                {
                                    _figures.Add(figure);
                                }
                            }

                            if (!EnableFailedToHitEvent)
                            {
                                break;
                            }
                        }
                        else
                        {
                            if (EnableFailedToHitEvent)
                            {
                                TouchActionEventArgs unTouchArgs = new TouchActionEventArgs(args.Id, TouchActionType.HitFailed, args.Location, args.IsInContact);
                                figure.ProcessTouchAction(unTouchArgs);
                            }
                        }
                    }

                    if (!_timerMode)
                    {
                        InvalidateSurface();
                    }

                    break;
                case TouchActionType.Moved:

                    if (relatedFigure != null)
                    {
                        relatedFigure.ProcessTouchAction(args);

                        if (!_timerMode)
                        {
                            InvalidateSurface();
                        }
                    }
                    break;
                case TouchActionType.Released:
                case TouchActionType.Exited:
                case TouchActionType.Cancelled:
                    if (relatedFigure != null)
                    {
                        relatedFigure.ProcessTouchAction(args);

                        _touchDictionary.Remove(eventId);

                        if (!_timerMode)
                        {
                            InvalidateSurface();
                        }
                    }

                    break;
            }
        }


    }
}
