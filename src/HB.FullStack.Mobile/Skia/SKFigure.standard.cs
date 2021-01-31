using HB.FullStack.Mobile.Effects.Touch;

using Microsoft.Extensions.Logging;

using SkiaSharp;
using SkiaSharp.Views.Forms;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace HB.FullStack.Mobile.Skia
{
    public abstract class SKFigure : ObservableObject, IDisposable
    {
        private SKFigureCanvasView? _canvasView;

        public object? Parent { get; set; }

        public SKFigureCanvasView? CanvasView
        {
            set
            {
                _canvasView = value;
            }
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

        public bool EnableTimeTick { get; set; }

        public bool CanResponseTimeTick { get; set; } = true;

        public bool StopResponseTimeTickWhenTouch { get; set; } = true;

        public bool ResumeResponseTimeTickAfterTouch { get; set; } = true;

        public bool EnableTouch { get; set; } = true;

        public bool EnableDrag { get; set; } = true;

        public bool EnableTwoFingers { get; set; }

        public bool EnableLongTap { get; set; } = true;

        public SKRatioPoint PivotRatioPoint { get; set; } = new SKRatioPoint(0.5f, 0.5f);

        public SKMatrix Matrix = SKMatrix.CreateIdentity();

        public SKPath? HitTestPath { get; set; }

        protected SKSize CanvasSize { get; private set; }

        public FigureState State { get; private set; } = FigureState.None;

        public void SetState(FigureState figureState)
        {
            State = figureState;
        }

        public virtual void OnPaint(SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;
            CanvasSize = info.Size;

            if (EnableTimeTick && CanResponseTimeTick)
            {
                CaculateMatrixByTime(CanvasView!.ElapsedMilliseconds);
            }

            //Translate to Pivot
            canvas.Translate(PivotRatioPoint.ToPoint(info.Size));

            //变型
            canvas.Concat(ref Matrix);

            OnDraw(info, canvas);

            OnUpdateHitTestPath(info);

            OnCaculateOutput();
        }

        protected abstract void OnDraw(SKImageInfo info, SKCanvas canvas);

        protected virtual void OnUpdateHitTestPath(SKImageInfo info) { }

        protected virtual void OnCaculateOutput() { }

        protected virtual void CaculateMatrixByTime(long elapsedMilliseconds) { }

        #region OnTouch

        public virtual bool OnHitTest(SKPoint skPoint, long touchId)
        {
            if (!EnableTouch || HitTestPath.IsNullOrEmpty())
            {
                return false;
            }

            SKPoint pivotedPoint = GetPivotedPoint(skPoint);

            if (Matrix.TryInvert(out SKMatrix inversedMatrix))
            {
                SKPoint mappedToOriginPoint = inversedMatrix.MapPoint(pivotedPoint);

                return HitTestPath.Contains(mappedToOriginPoint.X, mappedToOriginPoint.Y);
            }

            return false;
        }

        private readonly Dictionary<long, SKTouchInfoEventArgs> _touchInfos = new Dictionary<long, SKTouchInfoEventArgs>();
        private readonly Dictionary<long, LongTouchTaskInfo> _longTouchInfos = new Dictionary<long, LongTouchTaskInfo>();

        /// <summary>
        /// touchInfo在Pressed中放入，在Existed,Release,Cancel中释放
        /// </summary>
        public void ProcessTouchAction(TouchActionEventArgs args)
        {
            if (!EnableTouch)
            {
                return;
            }

            SKPoint curLocation = SKUtil.ToSKPoint(args.Location);

            //_logger.LogDebug($"{args.Type}, Id:{args.Id}, Location : {args.Location}");

            switch (args.Type)
            {
                case TouchActionType.HitFailed:
                    {
                        OnHitFailed();
                        break;
                    }
                case TouchActionType.Pressed:
                    {
                        if (StopResponseTimeTickWhenTouch)
                        {
                            CanResponseTimeTick = false;
                        }

                        SKTouchInfoEventArgs touchInfo = new SKTouchInfoEventArgs
                        {
                            StartPoint = curLocation,
                            PreviousPoint = curLocation,
                            CurrentPoint = curLocation,
                            TouchEventId = args.Id,
                            IsOver = false,
                            LongPressHappend = false
                        };

                        _touchInfos.Add(args.Id, touchInfo);

                        if (EnableLongTap)
                        {
                            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

                            _longTouchInfos[args.Id] = new LongTouchTaskInfo
                            {
                                CancellationTokenSource = cancellationTokenSource,
                                Task = LongPressedTaskAsync(touchInfo, cancellationTokenSource.Token)
                            };
                        }

                        OnPressed(touchInfo);

                        break;
                    }
                case TouchActionType.Moved:
                    {
                        if (!EnableDrag || !EnableTouch)
                        {
                            return;
                        }

                        if (!_touchInfos.TryGetValue(args.Id, out SKTouchInfoEventArgs? touchInfo))
                        {
                            return;
                        }

                        if (touchInfo.IsOver)
                        {
                            return;
                        }

                        touchInfo.CurrentPoint = curLocation;

                        if (touchInfo.StartPoint == curLocation)
                        {
                            //相当于Press
                            //DO nothing
                            //华为真机会不停的Move在原地

                            return;
                        }

                        if (_touchInfos.Count == 1)
                        {
                            CancelLongTap(args);

                            OnDragged(touchInfo);
                        }
                        else if (EnableTwoFingers && _touchInfos.Count == 2)
                        {
                            CancelLongTap(args);

                            touchInfo.PivotPoint = _touchInfos.Where(p => p.Key != args.Id).First().Value.CurrentPoint;

                            OnTwoFingerDragged(touchInfo);
                        }

                        touchInfo.PreviousPoint = touchInfo.CurrentPoint;

                        break;
                    }
                case TouchActionType.Exited:
                case TouchActionType.Released:
                    {
                        if (ResumeResponseTimeTickAfterTouch)
                        {
                            CanResponseTimeTick = true;
                        }

                        if (!_touchInfos.TryGetValue(args.Id, out SKTouchInfoEventArgs? touchInfo))
                        {
                            return;
                        }

                        CancelLongTap(args);

                        if (touchInfo.IsOver)
                        {
                            _touchInfos.Remove(args.Id);
                            return;
                        }

                        touchInfo.CurrentPoint = curLocation;
                        touchInfo.IsOver = true;

                        if (touchInfo.StartPoint == touchInfo.CurrentPoint && !touchInfo.LongPressHappend)
                        {
                            OnTapped(touchInfo);
                        }
                        else if (EnableDrag && _touchInfos.Count == 1)
                        {
                            OnDragged(touchInfo);
                        }
                        else if (EnableTwoFingers && _touchInfos.Count == 2)
                        {
                            touchInfo.PivotPoint = _touchInfos.Where(p => p.Key != args.Id).First().Value.CurrentPoint;
                            OnTwoFingerDragged(touchInfo);
                        }

                        _touchInfos.Remove(args.Id);

                        break;
                    }
                case TouchActionType.Cancelled:
                    {
                        if (ResumeResponseTimeTickAfterTouch)
                        {
                            CanResponseTimeTick = true;
                        }

                        if (!_touchInfos.TryGetValue(args.Id, out SKTouchInfoEventArgs? touchInfo))
                        {
                            return;
                        }

                        CancelLongTap(args);

                        if (!touchInfo.IsOver)
                        {
                            touchInfo.IsOver = true;

                            OnCancelled(touchInfo);
                        }

                        _touchInfos.Remove(args.Id);

                        break;
                    }
            }
        }

        private void CancelLongTap(TouchActionEventArgs args)
        {
            if (EnableLongTap && _longTouchInfos.TryGetValue(args.Id, out LongTouchTaskInfo? taskWrapper))
            {
                taskWrapper.CancellationTokenSource.Cancel();
                taskWrapper.CancellationTokenSource.Dispose();

                _longTouchInfos.Remove(args.Id);
            }
        }

        private Task LongPressedTaskAsync(SKTouchInfoEventArgs info, CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                await Task.Delay(Consts.LongTapMinDurationInMilliseconds).ConfigureAwait(false);

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                //GlobalSettings.Logger.LogDebug("LLLLLLLLLLLLLLLong pressed!");

                info.LongPressHappend = true;

                OnLongTapped(info);

                Device.BeginInvokeOnMainThread(() => CanvasView?.InvalidateSurface());

            }, cancellationToken);
        }

        public SKPoint GetPivotedPoint(SKPoint point)
        {
            return new SKPoint(point.X - CanvasSize.Width * PivotRatioPoint.XRatio, point.Y - CanvasSize.Height * PivotRatioPoint.YRatio);
        }

        class LongTouchTaskInfo
        {
            public Task Task { get; set; } = null!;

            public CancellationTokenSource CancellationTokenSource { get; set; } = null!;
        }

        private readonly WeakEventManager _weakEventManager = new WeakEventManager();

        public event EventHandler<SKTouchInfoEventArgs> Pressed
        {
            add => _weakEventManager.AddEventHandler(value, nameof(Pressed));
            remove => _weakEventManager.RemoveEventHandler(value, nameof(Pressed));
        }

        public event EventHandler<SKTouchInfoEventArgs> LongTapped
        {
            add => _weakEventManager.AddEventHandler(value, nameof(LongTapped));
            remove => _weakEventManager.RemoveEventHandler(value, nameof(LongTapped));
        }

        public event EventHandler<SKTouchInfoEventArgs> Tapped
        {
            add => _weakEventManager.AddEventHandler(value, nameof(Tapped));
            remove => _weakEventManager.RemoveEventHandler(value, nameof(Tapped));
        }

        public event EventHandler<SKTouchInfoEventArgs> Dragged
        {
            add => _weakEventManager.AddEventHandler(value, nameof(Dragged));
            remove => _weakEventManager.RemoveEventHandler(value, nameof(Dragged));
        }

        public event EventHandler<SKTouchInfoEventArgs> TwoFingerDragged
        {
            add => _weakEventManager.AddEventHandler(value, nameof(TwoFingerDragged));
            remove => _weakEventManager.RemoveEventHandler(value, nameof(TwoFingerDragged));
        }

        public event EventHandler<SKTouchInfoEventArgs> Cancelled
        {
            add => _weakEventManager.AddEventHandler(value, nameof(Cancelled));
            remove => _weakEventManager.RemoveEventHandler(value, nameof(Cancelled));
        }

        public event EventHandler? HitFailed
        {
            add => _weakEventManager.AddEventHandler(value, nameof(HitFailed));
            remove => _weakEventManager.RemoveEventHandler(value, nameof(HitFailed));
        }

        public void OnPressed(SKTouchInfoEventArgs touchInfo)
        {
            _weakEventManager.HandleEvent(this, touchInfo, nameof(Pressed));
        }

        public void OnDragged(SKTouchInfoEventArgs touchInfo)
        {
            _weakEventManager.HandleEvent(this, touchInfo, nameof(Dragged));

            if (State == FigureState.Selected || State == FigureState.LongSelected)
            {
                return;
            }

            SetState(FigureState.Selected);
        }

        public void OnTwoFingerDragged(SKTouchInfoEventArgs touchInfo)
        {
            _weakEventManager.HandleEvent(this, touchInfo, nameof(TwoFingerDragged));

            if (State == FigureState.Selected || State == FigureState.LongSelected)
            {
                return;
            }

            SetState(FigureState.Selected);
        }

        public void OnTapped(SKTouchInfoEventArgs touchInfo)
        {
            _weakEventManager.HandleEvent(this, touchInfo, nameof(Tapped));

            SetState(FigureState.Selected);
        }

        public void OnLongTapped(SKTouchInfoEventArgs touchInfo)
        {
            _weakEventManager.HandleEvent(this, touchInfo, nameof(LongTapped));

            SetState(FigureState.LongSelected);
        }

        public void OnCancelled(SKTouchInfoEventArgs touchInfo)
        {
            _weakEventManager.HandleEvent(this, touchInfo, nameof(Cancelled));

            //SetState(FigureState.None);
        }

        public void OnHitFailed()
        {
            _weakEventManager.HandleEvent(this, EventArgs.Empty, nameof(HitFailed));

            if (Parent is SKFigureGroup group)
            {
                if (group.EnableMultipleSelected)
                {
                    return;
                }
            }

            SetState(FigureState.None);
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
