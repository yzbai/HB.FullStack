using HB.FullStack.XamarinForms.Base;
using HB.FullStack.XamarinForms.Effects.Touch;

using SkiaSharp;
using SkiaSharp.Views.Forms;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace HB.FullStack.XamarinForms.Skia
{
    public abstract class SKFigure : ObservableObject, IDisposable
    {
        private SKFigureCanvasView? _canvasView;
        private SKPath? _hitTestPath;

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

        /// <summary>
        ///在原始坐标系下，新坐标系的原点。
        /// </summary>
        public SKRatioPoint NewCoordinateOriginalRatioPoint { get; set; } = new SKRatioPoint(0.5f, 0.5f);

        public SKSize CanvasSize { get; private set; }
        
        public bool CanvasSizeChanged { get; set; }

        public FigureState State { get; private set; } = FigureState.None;

        public SKPath? HitTestPath { get => _hitTestPath; set { _hitTestPath?.Dispose(); _hitTestPath = value; } }
        
        public SKMatrix Matrix = SKMatrix.CreateIdentity();

        public void SetState(FigureState figureState)
        {
            State = figureState;
        }

        public virtual void OnPaint(SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            if (CanvasSize != info.Size)
            {
                CanvasSize = info.Size;
                CanvasSizeChanged = true;
            }
            else
            {
                CanvasSizeChanged = false;
            }

            if (EnableTimeTick && CanResponseTimeTick)
            {
                CaculateMatrixByTime(CanvasView!.ElapsedMilliseconds);
            }

            //Translate to Pivot
            canvas.Translate(NewCoordinateOriginalRatioPoint.ToSKPoint(info.Size));

            //Matrix
            canvas.Concat(ref Matrix);

            OnDraw(info, canvas);

            OnUpdateHitTestPath(info);

            OnCaculateOutput();
        }

        /// <summary>
        /// 在新坐标系下画画
        /// </summary>
        /// <param name="info"></param>
        /// <param name="canvas"></param>
        protected abstract void OnDraw(SKImageInfo info, SKCanvas canvas);

        protected virtual void OnUpdateHitTestPath(SKImageInfo info) { }

        protected virtual void OnCaculateOutput() { }

        protected virtual void CaculateMatrixByTime(long elapsedMilliseconds) { }

        #region OnTouch

        /// <summary>
        /// 由SKFigureCanvasView调用
        /// </summary>
        /// <param name="skPoint">原始坐标系下的点</param>
        /// <param name="touchId">第几个指头</param>
        /// <returns></returns>
        public virtual bool OnHitTest(SKPoint skPoint, long touchId)
        {
            if (!EnableTouch || HitTestPath.IsNullOrEmpty())
            {
                return false;
            }

            SKPoint hitPoint = GetNewCoordinatedPoint(skPoint);

            if (Matrix.TryInvert(out SKMatrix inversedMatrix))
            {
                SKPoint mappedToOriginPoint = inversedMatrix.MapPoint(hitPoint);

                return HitTestPath.Contains(mappedToOriginPoint.X, mappedToOriginPoint.Y);
            }

            return false;
        }

        private readonly Dictionary<long, SKFigureTouchInfo> _fingerTouchInfos = new Dictionary<long, SKFigureTouchInfo>();
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

            SKPoint location = SKUtil.ToSKPoint(args.DpLocation);

            SKPoint curLocation = GetNewCoordinatedPoint(location);

            BaseApplication.LogDebug($"开始处理Touch Figure: {this.GetType().Name}, Events: {SerializeUtil.ToJson(args)}");

            switch (args.ActionType)
            {
                case TouchActionType.Pressed:
                    {
                        if (StopResponseTimeTickWhenTouch)
                        {
                            CanResponseTimeTick = false;
                        }

                        SKFigureTouchInfo touchInfo = new SKFigureTouchInfo
                        {
                            StartPoint = curLocation,
                            PreviousPoint = curLocation,
                            CurrentPoint = curLocation,
                            TouchEventId = args.Id,
                            IsOver = false,
                            LongPressHappend = false
                        };

                        _fingerTouchInfos.Add(args.Id, touchInfo);
                        BaseApplication.LogDebug($"加入FingerTouchInfo Figure: {this.GetType().Name}, Events: {SerializeUtil.ToJson(args)}");

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

                        if (!_fingerTouchInfos.TryGetValue(args.Id, out SKFigureTouchInfo? touchInfo))
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

                        if (_fingerTouchInfos.Count == 1)
                        {
                            CancelLongTap(args);

                            OnOneFingerDragged(touchInfo);
                        }
                        else if (EnableTwoFingers && _fingerTouchInfos.Count == 2)
                        {
                            CancelLongTap(args);

                            touchInfo.PivotPoint = _fingerTouchInfos.Where(p => p.Key != args.Id).First().Value.CurrentPoint;

                            OnTwoFingerDragged(touchInfo);
                        }

                        touchInfo.PreviousPoint = touchInfo.CurrentPoint;

                        break;
                    }
                case TouchActionType.Exited:
                case TouchActionType.Released:
                    {
                        BaseApplication.LogDebug($"进入Exitted,Released. Figure: {this.GetType().Name}, Events: {SerializeUtil.ToJson(args)}");

                        if (ResumeResponseTimeTickAfterTouch)
                        {
                            CanResponseTimeTick = true;
                        }

                        if (!_fingerTouchInfos.TryGetValue(args.Id, out SKFigureTouchInfo? touchInfo))
                        {
                            return;
                        }

                        CancelLongTap(args);

                        if (touchInfo.IsOver)
                        {
                            BaseApplication.LogDebug($"移除FingerTouchInfo Figure: {this.GetType().Name}, Events: {SerializeUtil.ToJson(args)}");
                            _fingerTouchInfos.Remove(args.Id);
                            return;
                        }

                        touchInfo.CurrentPoint = curLocation;
                        touchInfo.IsOver = true;

                        if (touchInfo.StartPoint == touchInfo.CurrentPoint && !touchInfo.LongPressHappend)
                        {
                            OnTapped(touchInfo);
                        }
                        else if (EnableDrag && _fingerTouchInfos.Count == 1)
                        {
                            OnOneFingerDragged(touchInfo);
                        }
                        else if (EnableTwoFingers && _fingerTouchInfos.Count == 2)
                        {
                            touchInfo.PivotPoint = _fingerTouchInfos.Where(p => p.Key != args.Id).First().Value.CurrentPoint;
                            OnTwoFingerDragged(touchInfo);
                        }

                        BaseApplication.LogDebug($"移除FingerTouchInfo Figure: {this.GetType().Name}, Events: {SerializeUtil.ToJson(args)}");
                        _fingerTouchInfos.Remove(args.Id);

                        break;
                    }
                case TouchActionType.Cancelled:
                    {
                        BaseApplication.LogDebug($"进入Cancelled. Figure: {this.GetType().Name}, Events: {SerializeUtil.ToJson(args)}");
                        if (ResumeResponseTimeTickAfterTouch)
                        {
                            CanResponseTimeTick = true;
                        }

                        if (!_fingerTouchInfos.TryGetValue(args.Id, out SKFigureTouchInfo? touchInfo))
                        {
                            return;
                        }

                        CancelLongTap(args);

                        if (!touchInfo.IsOver)
                        {
                            touchInfo.IsOver = true;

                            OnCancelled(touchInfo);
                        }

                        BaseApplication.LogDebug($"移除FingerTouchInfo Figure: {this.GetType().Name}, Events: {SerializeUtil.ToJson(args)}");
                        _fingerTouchInfos.Remove(args.Id);

                        break;
                    }
                default:
                    break;
            }
        }

        public void ProcessUnTouchAction(long fingerId, SKPoint location)
        {
            OnHitFailed();
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

        private Task LongPressedTaskAsync(SKFigureTouchInfo info, CancellationToken cancellationToken)
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

        protected SKPoint GetNewCoordinatedPoint(SKPoint point)
        {
            return new SKPoint(point.X - CanvasSize.Width * NewCoordinateOriginalRatioPoint.XRatio, point.Y - CanvasSize.Height * NewCoordinateOriginalRatioPoint.YRatio);
        }

        class LongTouchTaskInfo
        {
            public Task Task { get; set; } = null!;

            public CancellationTokenSource CancellationTokenSource { get; set; } = null!;
        }

        private readonly WeakEventManager _weakEventManager = new WeakEventManager();

        public event EventHandler<SKFigureTouchInfo> Pressed
        {
            add => _weakEventManager.AddEventHandler(value, nameof(Pressed));
            remove => _weakEventManager.RemoveEventHandler(value, nameof(Pressed));
        }

        public event EventHandler<SKFigureTouchInfo> LongTapped
        {
            add => _weakEventManager.AddEventHandler(value, nameof(LongTapped));
            remove => _weakEventManager.RemoveEventHandler(value, nameof(LongTapped));
        }

        public event EventHandler<SKFigureTouchInfo> Tapped
        {
            add => _weakEventManager.AddEventHandler(value, nameof(Tapped));
            remove => _weakEventManager.RemoveEventHandler(value, nameof(Tapped));
        }

        public event EventHandler<SKFigureTouchInfo> OneFingerDragged
        {
            add => _weakEventManager.AddEventHandler(value, nameof(OneFingerDragged));
            remove => _weakEventManager.RemoveEventHandler(value, nameof(OneFingerDragged));
        }

        public event EventHandler<SKFigureTouchInfo> TwoFingerDragged
        {
            add => _weakEventManager.AddEventHandler(value, nameof(TwoFingerDragged));
            remove => _weakEventManager.RemoveEventHandler(value, nameof(TwoFingerDragged));
        }

        public event EventHandler<SKFigureTouchInfo> Cancelled
        {
            add => _weakEventManager.AddEventHandler(value, nameof(Cancelled));
            remove => _weakEventManager.RemoveEventHandler(value, nameof(Cancelled));
        }

        public event EventHandler? HitFailed
        {
            add => _weakEventManager.AddEventHandler(value, nameof(HitFailed));
            remove => _weakEventManager.RemoveEventHandler(value, nameof(HitFailed));
        }

        public void OnPressed(SKFigureTouchInfo touchInfo)
        {
            _weakEventManager.HandleEvent(this, touchInfo, nameof(Pressed));
        }

        public void OnOneFingerDragged(SKFigureTouchInfo touchInfo)
        {
            _weakEventManager.HandleEvent(this, touchInfo, nameof(OneFingerDragged));

            if (State == FigureState.Selected || State == FigureState.LongSelected)
            {
                return;
            }

            SetState(FigureState.Selected);
        }

        public void OnTwoFingerDragged(SKFigureTouchInfo touchInfo)
        {
            _weakEventManager.HandleEvent(this, touchInfo, nameof(TwoFingerDragged));

            if (State == FigureState.Selected || State == FigureState.LongSelected)
            {
                return;
            }

            SetState(FigureState.Selected);
        }

        public void OnTapped(SKFigureTouchInfo touchInfo)
        {
            _weakEventManager.HandleEvent(this, touchInfo, nameof(Tapped));

            SetState(FigureState.Selected);
        }

        public void OnLongTapped(SKFigureTouchInfo touchInfo)
        {
            _weakEventManager.HandleEvent(this, touchInfo, nameof(LongTapped));

            SetState(FigureState.LongSelected);
        }

        public void OnCancelled(SKFigureTouchInfo touchInfo)
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
                    HitTestPath?.Dispose();
                    _fingerTouchInfos.Clear();
                    _longTouchInfos.Clear();
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
