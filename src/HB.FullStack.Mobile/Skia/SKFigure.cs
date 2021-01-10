using HB.FullStack.Mobile.Effects;
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
    internal class TaskWrapper
    {
        public Task Task { get; set; } = null!;

        public CancellationTokenSource CancellationTokenSource { get; set; } = null!;
    }

    public abstract class SKFigure : IDisposable
    {
        public const float LongTapTolerantDistanceInDp = 0.1f;
        public const int LongTapMinDurationInMilliseconds = 400;

        private readonly Dictionary<long, SKTouchInfoEventArgs> _touchInfos = new Dictionary<long, SKTouchInfoEventArgs>();
        private readonly Dictionary<long, TaskWrapper> _touchLongTask = new Dictionary<long, TaskWrapper>();

        protected SKFigure() : this(1f, 1f, SKAlignment.Center, SKAlignment.Center) { }

        protected SKFigure(float widthRatio, float heightRatio) : this(widthRatio, heightRatio, SKAlignment.Center, SKAlignment.Center) { }

        protected SKFigure(float widthRatio, float heightRatio, SKAlignment horizontalAlignment, SKAlignment verticalAlignment)
        {
            WidthRatio = widthRatio;
            HeightRatio = heightRatio;

            VerticalAlignment = verticalAlignment;
            HorizontalAlignment = horizontalAlignment;
        }

        public float WidthRatio { get; }
        public float HeightRatio { get; }

        public SKAlignment VerticalAlignment { get; }
        public SKAlignment HorizontalAlignment { get; }

        /// <summary>
        /// Maybe SKFigureGroup or SKFigureCanvasView
        /// </summary>
        public object? Parent { get; set; }

        public SKFigureCanvasView? CanvasView
        {
            get
            {
                object? obj = Parent;

                while (obj != null)
                {
                    if (obj is SKFigureCanvasView canvasView)
                    {
                        return canvasView;
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

                return null;
            }
        }

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

        [SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "<Pending>")]
        public SKMatrix AppliedMatrix = SKMatrix.CreateIdentity();

        public abstract void Paint(SKPaintSurfaceEventArgs e);

        #region 事件

        public virtual bool HitTest(SKPoint skPoint, long touchId)
        {
            if (HitTestBounds == SKRect.Empty && Bounds == SKRect.Empty)
            {
                return false;
            }

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
                await Task.Delay(LongTapMinDurationInMilliseconds).ConfigureAwait(false);

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

                            _touchLongTask[args.Id] = new TaskWrapper
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
                                if (_touchLongTask.TryGetValue(args.Id, out TaskWrapper? taskWrapper))
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
                            if (_touchLongTask.TryGetValue(args.Id, out TaskWrapper? taskWrapper))
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
                            if (_touchLongTask.TryGetValue(args.Id, out TaskWrapper? taskWrapper))
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

        #region Matrix

        public SKSize GetFigureSize(SKSize canvasSize)
        {
            return new SKSize(canvasSize.Width * WidthRatio, canvasSize.Height * HeightRatio);
        }

        public float GetFigureWidth(SKSize canvasSize)
        {
            return canvasSize.Width * WidthRatio;
        }

        public float GetFigureHeight(SKSize canvasSize)
        {
            return canvasSize.Height * HeightRatio;
        }

        public SKMatrix GetTransToFigureCenterMatrix(SKSize canvasSize, SKSize sourceSize)
        {
            SKPoint figureCenter = GetFigureCenter(canvasSize);

            SKPoint sourceCenter = new SKPoint(sourceSize.Width / 2f, sourceSize.Height / 2f);

            return SKMatrix.CreateTranslation(figureCenter.X - sourceCenter.X, figureCenter.Y - sourceCenter.Y);
        }

        public SKPoint GetFigureCenter(SKSize canvasSize)
        {
            float x, y;

            x = HorizontalAlignment switch
            {
                SKAlignment.Center => canvasSize.Width / 2f,
                SKAlignment.Start => canvasSize.Width * WidthRatio / 2f,
                SKAlignment.End => canvasSize.Width - canvasSize.Width * WidthRatio / 2f,
                _ => 0
            };

            y = VerticalAlignment switch
            {
                SKAlignment.Center => canvasSize.Height / 2f,
                SKAlignment.Start => canvasSize.Height * HeightRatio / 2f,
                SKAlignment.End => canvasSize.Height - canvasSize.Height * HeightRatio / 2f,
                _ => 0
            };

            return new SKPoint(x, y);
        }

        /// <summary>
        /// 获取sourceSize大小的bitmap填充Figure的变换矩阵
        /// </summary>
        /// <param name="canvasSize"></param>
        /// <param name="sourceSize"></param>
        /// <param name="stretch"></param>
        /// <returns></returns>
        public SKMatrix GetFilledMatrix(SKSize canvasSize, SKSize sourceSize, SKStretch stretch = SKStretch.AspectFill)
        {
            SKMatrix transToCenterMatrix = GetTransToFigureCenterMatrix(canvasSize, sourceSize);

            float figureWidth = canvasSize.Width * WidthRatio;
            float figureHeight = canvasSize.Height * HeightRatio;
            float widthScale = figureWidth / sourceSize.Width;
            float heightScale = figureHeight / sourceSize.Height;
            float maxScale = Math.Max(heightScale, widthScale);
            float minScale = Math.Min(heightScale, widthScale);

            SKPoint sourceCenter = new SKPoint(sourceSize.Width / 2f, sourceSize.Height / 2f);

            SKMatrix scaleMatrix = stretch switch
            {
                SKStretch.AspectFill => SKMatrix.CreateScale(maxScale, maxScale, sourceCenter.X, sourceCenter.Y),
                SKStretch.AspectFit => SKMatrix.CreateScale(minScale, minScale, sourceCenter.X, sourceCenter.Y),
                SKStretch.Fill => SKMatrix.CreateScale(widthScale, heightScale, sourceCenter.X, sourceCenter.Y),
                _ => SKMatrix.Identity
            };

            return SKMatrix.Concat(transToCenterMatrix, scaleMatrix);

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
