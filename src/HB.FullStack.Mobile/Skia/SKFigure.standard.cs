using HB.FullStack.Mobile.Effects;
using HB.FullStack.Mobile.Effects.Touch;

using Microsoft.Extensions.Logging;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace HB.FullStack.Mobile.Skia
{
    public abstract class SKFigure : IDisposable
    {
        class LongTouchTaskInfo
        {
            public Task Task { get; set; } = null!;

            public CancellationTokenSource CancellationTokenSource { get; set; } = null!;
        }

        private readonly Dictionary<long, SKTouchInfoEventArgs> _touchInfos = new Dictionary<long, SKTouchInfoEventArgs>();
        private readonly Dictionary<long, LongTouchTaskInfo> _touchLongTask = new Dictionary<long, LongTouchTaskInfo>();
        private SKFigureCanvasView? _canvasView;

        /// <summary>
        /// Maybe SKFigureGroup or SKFigureCanvasView
        /// </summary>
        public object? Parent { get; set; }

        public SKFigureCanvasView? CanvasView
        {
            get
            {
                if (_canvasView == null)
                {
                    object? obj = Parent;

                    while (obj != null)
                    {
                        if (obj is SKFigureCanvasView canvasView)
                        {
                            _canvasView = canvasView;
                            break;
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

                return _canvasView;
            }
        }

        public bool EnableDrag { get; set; } = true;

        public bool EnableTouch { get; set; } = true;

        /// <summary>
        /// 轮廓
        /// </summary>
        public virtual SKPath? Path { get; set; }

        /// <summary>
        /// 用于测试是否点击到的轮廓，一般比Path大一点
        /// </summary>
        public virtual SKPath? HitTestPath { get; set; }

        /// <summary>
        /// 变换矩阵
        /// </summary>
        public SKMatrix Matrix = SKMatrix.CreateIdentity();

        public abstract void Paint(SKPaintSurfaceEventArgs e);

        public virtual bool HitTest(SKPoint skPoint, long touchId)
        {
            if (Path.IsNullOrEmpty() && HitTestPath.IsNullOrEmpty())
            {
                return false;
            }

            if (EnableTouch == false)
            {
                return false;
            }

            if (Matrix.TryInvert(out SKMatrix inversedMatrix))
            {
                SKPoint mappedToOriginPoint = inversedMatrix.MapPoint(skPoint);

                if (HitTestPath.IsNotNullOrEmpty())
                {
                    return HitTestPath.Contains(mappedToOriginPoint.X, mappedToOriginPoint.Y);
                }
                else
                {
                    return Path!.Contains(mappedToOriginPoint.X, mappedToOriginPoint.Y);
                }
            }

            return false;
        }

        #region 事件

        public event EventHandler<SKTouchInfoEventArgs>? Pressed;

        public event EventHandler<SKTouchInfoEventArgs>? LongTapped;

        public event EventHandler<SKTouchInfoEventArgs>? Tapped;

        public event EventHandler<SKTouchInfoEventArgs>? Dragged;

        public event EventHandler<SKTouchInfoEventArgs>? Cancelled;

        public event EventHandler? HitFailed;

        private Task LongPressedTaskAsync(SKTouchInfoEventArgs info, CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                await Task.Delay(Consts.LongTapMinDurationInMilliseconds).ConfigureAwait(false);

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

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
                            SKTouchInfoEventArgs touchInfo = new SKTouchInfoEventArgs
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

                            _touchLongTask[args.Id] = new LongTouchTaskInfo
                            {
                                CancellationTokenSource = cancellationTokenSource,
                                Task = LongPressedTaskAsync(touchInfo, cancellationTokenSource.Token)
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

                        if (_touchInfos.TryGetValue(args.Id, out SKTouchInfoEventArgs? touchInfo))
                        {
                            touchInfo.CurrentPoint = curLocation;

                            if (touchInfo.StartPoint == curLocation)
                            {
                                //相当于Press
                            }
                            else
                            {
                                if (_touchLongTask.TryGetValue(args.Id, out LongTouchTaskInfo? taskWrapper))
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

                        if (_touchInfos.TryGetValue(args.Id, out SKTouchInfoEventArgs? touchInfo))
                        {
                            if (_touchLongTask.TryGetValue(args.Id, out LongTouchTaskInfo? taskWrapper))
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
                        if (_touchInfos.TryGetValue(args.Id, out SKTouchInfoEventArgs? touchInfo))
                        {
                            if (_touchLongTask.TryGetValue(args.Id, out LongTouchTaskInfo? taskWrapper))
                            {
                                taskWrapper.CancellationTokenSource.Cancel();
                                _touchLongTask.Remove(args.Id);
                            }

                            touchInfo.IsOver = true;

                            OnCancelled(touchInfo);

                            _touchInfos.Remove(args.Id);

                        }
                    }
                    break;
            }
        }

        public void OnPressed(SKTouchInfoEventArgs touchInfo)
        {
            Pressed?.Invoke(this, touchInfo);
        }

        public void OnDragged(SKTouchInfoEventArgs touchInfo)
        {
            Dragged?.Invoke(this, touchInfo);
        }

        public void OnTapped(SKTouchInfoEventArgs touchInfo)
        {
            Tapped?.Invoke(this, touchInfo);
        }

        public void OnLongTapped(SKTouchInfoEventArgs touchInfo)
        {
            LongTapped?.Invoke(this, touchInfo);
        }

        public void OnCancelled(SKTouchInfoEventArgs touchInfo)
        {
            Cancelled?.Invoke(this, touchInfo);
        }

        public void OnHitFailed()
        {
            HitFailed?.Invoke(this, new EventArgs());
        }

        public void InvalidateSurface()
        {
            CanvasView?.InvalidateSurface();
        }

        #endregion

        #region Disposable Pattern

        private bool _disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SKFigure()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }
}
