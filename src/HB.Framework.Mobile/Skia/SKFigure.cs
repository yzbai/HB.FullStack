using HB.Framework.Client.Effects;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace HB.Framework.Client.Skia
{
    internal class TaskWrapper
    {
        public Task Task { get; set; } = null!;

        public CancellationTokenSource CancellationTokenSource { get; set; } = null!;
    }
    public abstract class SKFigure
    {
        public const float LongTapTolerantDistanceInDp = 0.1f;
        public const int LongTapMinDurationInMilliseconds = 400;

        /// <summary>
        /// Maybe SKFigureGroup or SKFigureCanvasView
        /// </summary>
        public object? Parent { get; set; }

        public bool EnableDrag { get; set; } = true;

        public bool EnableTouch { get; set; } = true;

        /// <summary>
        /// Figure的方形轮廓
        /// </summary>
        public virtual SKRect Bounds { get; set; }

        /// <summary>
        /// 用于测试是否点击到的方形轮廓，一般比Bounds大一点
        /// </summary>
        public virtual SKRect HitTestBounds { get; set; }

#pragma warning disable CA1051 // Do not declare visible instance fields
        public SKMatrix AppliedMatrix = SKMatrix.CreateIdentity();
#pragma warning restore CA1051 // Do not declare visible instance fields

        private readonly ILogger _logger = DependencyService.Resolve<ILogger<SKFigure>>();

        private readonly Dictionary<long, SKTouchInfo> _touchInfos = new Dictionary<long, SKTouchInfo>();

        private readonly Dictionary<long, TaskWrapper> _touchLongTask = new Dictionary<long, TaskWrapper>();

        public abstract void Paint(SKPaintSurfaceEventArgs e);

        #region 事件

        public virtual bool HitTest(SKPoint skPoint, long touchId)
        {
            if (EnableTouch == false)
            {
                return false;
            }

            if (AppliedMatrix.TryInvert(out SKMatrix inversedMatrix))
            {
                SKPoint mappedToOriginPoint = inversedMatrix.MapPoint(skPoint);

                if (HitTestBounds != default)
                {
                    return HitTestBounds.Contains(mappedToOriginPoint);
                }
                else
                {
                    return Bounds.Contains(mappedToOriginPoint);
                }
            }

            return false;
        }

        public event EventHandler<SKTouchInfo>? Pressed;

        public event EventHandler<SKTouchInfo>? LongTapped;

        public event EventHandler<SKTouchInfo>? Tapped;

        public event EventHandler<SKTouchInfo>? Dragged;

        public event EventHandler<SKTouchInfo>? Cancelled;

        public event EventHandler? HitFailed;

        private Task LongPressedTask(SKTouchInfo info, CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                await Task.Delay(LongTapMinDurationInMilliseconds).ConfigureAwait(false);

                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogDebug($"LongPress Cancelled.");
                    return;
                }

                _logger.LogDebug($"LongPress Fired.");

                info.LongPressHappend = true;

                OnLongTapped(info);
            }, cancellationToken);
        }

        public virtual void ProcessTouchAction(TouchActionEventArgs args)
        {
            SKPoint curLocation = SKUtil.ToSKPoint(args.Location);

            //_logger.LogDebug($"{args.Type}, Id:{args.Id}, Location : {args.Location}");

            switch (args.Type)
            {
                case TouchActionType.HitFailed:
                    {
                        if (!EnableTouch)
                        {
                            return;
                        }

                        OnHitFailed();
                    }
                    break;
                case TouchActionType.Pressed:
                    {
                        if (EnableTouch)
                        {
                            SKTouchInfo touchInfo = new SKTouchInfo
                            {
                                StartPoint = curLocation,
                                PreviousPoint = curLocation,
                                CurrentPoint = curLocation,
                                TouchEventId = args.Id,
                                IsOver = false,
                                LongPressHappend = false
                            };

                            _touchInfos[args.Id] = touchInfo;


                            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

                            _touchLongTask[args.Id] = new TaskWrapper
                            {
                                CancellationTokenSource = cancellationTokenSource,
                                Task = LongPressedTask(touchInfo, cancellationTokenSource.Token)
                            };

                            OnPressed(touchInfo);
                        }
                    }
                    break;
                case TouchActionType.Moved:
                    {
                        if (!EnableDrag || !EnableTouch)
                        {
                            return;
                        }

                        if (_touchInfos.TryGetValue(args.Id, out SKTouchInfo touchInfo))
                        {
                            touchInfo.CurrentPoint = curLocation;

                            if (touchInfo.StartPoint == curLocation)
                            {
                                //相当于Press
                            }
                            else
                            {
                                if (_touchLongTask.TryGetValue(args.Id, out TaskWrapper taskWrapper))
                                {
                                    taskWrapper.CancellationTokenSource.Cancel();
                                    _touchLongTask.Remove(args.Id);
                                }

                                OnDragged(touchInfo);

                                touchInfo.PreviousPoint = touchInfo.CurrentPoint;
                            }
                        }
                    }
                    break;
                case TouchActionType.Exited:
                case TouchActionType.Released:
                    {
                        if (!EnableTouch)
                        {
                            return;
                        }

                        if (_touchInfos.TryGetValue(args.Id, out SKTouchInfo touchInfo))
                        {
                            if (_touchLongTask.TryGetValue(args.Id, out TaskWrapper taskWrapper))
                            {
                                taskWrapper.CancellationTokenSource.Cancel();
                                _touchLongTask.Remove(args.Id);
                            }

                            touchInfo.CurrentPoint = curLocation;
                            touchInfo.IsOver = true;

                            if (touchInfo.StartPoint == touchInfo.CurrentPoint)
                            {
                                if (!touchInfo.LongPressHappend)
                                {
                                    OnTapped(touchInfo);
                                }
                            }
                            else
                            {
                                if (EnableDrag)
                                {
                                    OnDragged(touchInfo);
                                }
                            }

                            _touchInfos.Remove(args.Id);
                        }
                    }
                    break;
                case TouchActionType.Cancelled:
                    {
                        if (_touchInfos.TryGetValue(args.Id, out SKTouchInfo touchInfo))
                        {
                            if (_touchLongTask.TryGetValue(args.Id, out TaskWrapper taskWrapper))
                            {
                                taskWrapper.CancellationTokenSource.Cancel();
                                _touchLongTask.Remove(args.Id);
                            }

                            touchInfo.IsOver = true;

                            OnCancelled(touchInfo);

                            _touchInfos.Remove(args.Id);

                            _logger.LogDebug($"Touch Action Cancelled. Id{args.Id}");
                        }
                    }
                    break;
            }
        }

        public void OnPressed(SKTouchInfo touchInfo)
        {
            Pressed?.Invoke(this, touchInfo);
        }

        public void OnDragged(SKTouchInfo touchInfo)
        {
            Dragged?.Invoke(this, touchInfo);
        }

        public void OnTapped(SKTouchInfo touchInfo)
        {
            Tapped?.Invoke(this, touchInfo);
        }

        public void OnLongTapped(SKTouchInfo touchInfo)
        {
            LongTapped?.Invoke(this, touchInfo);
        }

        public void OnCancelled(SKTouchInfo touchInfo)
        {
            Cancelled?.Invoke(this, touchInfo);
        }

        public void OnHitFailed()
        {
            HitFailed?.Invoke(this, new EventArgs());
        }

        public void InvalidateSurface()
        {
            object? obj = Parent;

            while (obj != null)
            {
                if (obj is SKFigureCanvasView canvasView)
                {
                    canvasView.InvalidateSurface();
                    return;
                }
                else if (obj is SKFigure figure)
                {
                    obj = figure.Parent;
                }
                else
                {
                    break;
                }
            }
        }

        #endregion 

        public static SKPoint TranslatePointToCenter(SKPoint skPoint, float canvasWidth, float canvasHeight)
        {
            return new SKPoint(skPoint.X - canvasWidth / 2, skPoint.Y - canvasHeight / 2);
        }
    }
}
