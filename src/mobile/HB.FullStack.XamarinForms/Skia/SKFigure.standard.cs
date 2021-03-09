
using HB.FullStack.Common;

using SkiaSharp;
using SkiaSharp.Views.Forms;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace HB.FullStack.XamarinForms.Skia
{
    public class EmptyData : FigureData
    {
        protected override bool EqualsImpl(FigureData other)
        {
            return other is EmptyData;
        }

        protected override HashCode GetHashCodeImpl()
        {
            return new HashCode();
        }
    }

    public abstract class SKFigure<TDrawInfo> : SKFigure<TDrawInfo, EmptyData> where TDrawInfo : FigureData
    {
        protected override void OnCaculateOutput(out EmptyData? newResultDrawData, TDrawInfo initDrawData)
        {
            newResultDrawData = null;
        }
    }

    public abstract class SKFigure<TDrawInfo, TData> : SKFigure 
        where TDrawInfo : FigureData 
        where TData : FigureData
    {
        public static BindableProperty DrawDataProperty = BindableProperty.Create(
                    nameof(DrawData),
                    typeof(TDrawInfo),
                    typeof(SKFigure<TDrawInfo, TData>),
                    null,
                    BindingMode.OneWay,
                    propertyChanged: (b, oldValue, newValue) => ((SKFigure<TDrawInfo, TData>)b).OnBaseDrawDataChanged((TDrawInfo?)oldValue, (TDrawInfo?)newValue));

        public static BindableProperty InitDataProperty = BindableProperty.Create(
                    nameof(InitData),
                    typeof(TData),
                    typeof(SKFigure<TDrawInfo, TData>),
                    null,
                    BindingMode.OneWay,
                    propertyChanged: (b, oldValue, newValue) => ((SKFigure<TDrawInfo, TData>)b).OnBaseInitDataChanged());


        public static BindableProperty ResultDataProperty = BindableProperty.Create(
                    nameof(ResultData),
                    typeof(TData),
                    typeof(SKFigure<TDrawInfo, TData>),
                    null,
                    BindingMode.OneWayToSource);

        public TDrawInfo? DrawData { get => (TDrawInfo?)GetValue(DrawDataProperty); set => SetValue(DrawDataProperty, value); }

        public TData? InitData { get => (TData?)GetValue(InitDataProperty); set => SetValue(InitDataProperty, value); }

        public TData? ResultData { get => (TData?)GetValue(ResultDataProperty); set => SetValue(ResultDataProperty, value); }

        protected bool HitPathNeedUpdate { get; set; }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "<Pending>")]
        private void OnBaseDrawDataChanged(TDrawInfo? oldValue, TDrawInfo? newValue)
        {
            HitPathNeedUpdate = true;

            OnDrawDataChanged();

            InvalidateMatrixAndSurface();
        }

        private void OnBaseInitDataChanged()
        {
            OnInitDataChanged();

            InvalidateOnlySurface();
        }

        protected abstract void OnInitDataChanged();

        protected override void OnDraw(SKImageInfo info, SKCanvas canvas)
        {
            if (DrawData == null)
            {
                return;
            }

            OnDraw(info, canvas, DrawData, State);
        }

        protected override void OnUpdateHitTestPath(SKImageInfo info)
        {
            if (DrawData == null)
            {
                return;
            }

            if (CanvasSizeChanged || HitPathNeedUpdate)
            {
                HitPathNeedUpdate = false;

                HitTestPath.Reset();

                OnUpdateHitTestPath(info, DrawData);
            }
        }

        protected override void OnCaculateOutput()
        {
            if (DrawData == null)
            {
                return;
            }

            OnCaculateOutput(out TData? newResult, DrawData);

            if (newResult != null)
            {
                newResult.State = State;
            }

            if (newResult != ResultData)
            {
                ResultData = newResult;
            }
        }

        protected abstract void OnDrawDataChanged();

        protected abstract void OnDraw(SKImageInfo info, SKCanvas canvas, TDrawInfo initDrawData, FigureState currentState);

        protected abstract void OnUpdateHitTestPath(SKImageInfo info, TDrawInfo initDrawData);

        protected abstract void OnCaculateOutput(out TData? newResultDrawData, TDrawInfo initDrawData);
    }
    public abstract class SKFigure : BindableObject, IDisposable
    {
        private SKFigureCanvasView? _canvasView;
        private SKPath _hitTestPath = new SKPath();

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
        public RatioPoint NewCoordinateOriginalRatioPoint { get; set; } = new RatioPoint(0.5f, 0.5f);

        public SKSize CanvasSize { get; private set; }

        public bool CanvasSizeChanged { get; set; }

        public FigureState State { get; private set; } = FigureState.None;

        public SKPath HitTestPath { get => _hitTestPath; set { _hitTestPath?.Dispose(); _hitTestPath = value; } }

        public SKMatrix Matrix = SKMatrix.CreateIdentity();

        public void SetState(FigureState figureState)
        {
            State = figureState;
        }

        public void InvalidateOnlySurface(bool evenTimeTickEnabled = false)
        {
            if (Parent == null || CanvasView == null)
            {
                return;
            }

            if (CanvasView.EnableTimeTick && !evenTimeTickEnabled)
            {
                return;
            }

            if (Parent is SKFigureGroup)
            {
                //留给集合统一处理
                return;
            }

            CanvasView.InvalidateSurface();
        }

        public void InvalidateMatrixAndSurface(bool evenTimeTickEnabled = false)
        {
            Matrix = SKMatrix.CreateIdentity();

            InvalidateOnlySurface(evenTimeTickEnabled);
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
        protected virtual void OnDraw(SKImageInfo info, SKCanvas canvas) { }

        protected virtual void OnUpdateHitTestPath(SKImageInfo info) { }

        protected virtual void OnCaculateOutput() { }

        protected virtual void CaculateMatrixByTime(long elapsedMilliseconds) { }

        #region OnTouch

        /// <summary>
        /// 由SKFigureCanvasView调用
        /// </summary>
        /// <param name="location">原始坐标系下的点</param>
        /// <param name="fingerId">第几个指头</param>
        /// <returns></returns>
        public virtual bool OnHitTest(SKPoint location, long fingerId)
        {
            if (!EnableTouch || HitTestPath.IsNullOrEmpty())
            {
                return false;
            }

            SKPoint hitPoint = GetNewCoordinatedPoint(location);

            if (Matrix.TryInvert(out SKMatrix inversedMatrix))
            {
                SKPoint mappedToOriginPoint = inversedMatrix.MapPoint(hitPoint);

                return HitTestPath.Contains(mappedToOriginPoint.X, mappedToOriginPoint.Y);
            }

            return false;
        }

        private readonly Dictionary<long, SKFigureTouchEventArgs> _fingerTouchInfos = new Dictionary<long, SKFigureTouchEventArgs>();
        private readonly Dictionary<long, LongTouchTaskInfo> _longTouchInfos = new Dictionary<long, LongTouchTaskInfo>();

        /// <summary>
        /// touchInfo在Pressed中放入，在Existed,Release,Cancel中释放
        /// </summary>
        public void ProcessTouchAction(SKTouchEventArgs args)
        {
            if (!EnableTouch)
            {
                return;
            }

            args.Handled = true;

            SKPoint location = args.Location;// SKUtil.ToSKPoint(args.DpLocation);

            SKPoint curLocation = GetNewCoordinatedPoint(location);

            //BaseApplication.LogDebug($"开始处理Touch Figure: {this.GetType().Name}, Events: {SerializeUtil.ToJson(args)}");

            switch (args.ActionType)
            {
                case SKTouchAction.Pressed:
                    {
                        if (StopResponseTimeTickWhenTouch)
                        {
                            CanResponseTimeTick = false;
                        }

                        SKFigureTouchEventArgs touchInfo = new SKFigureTouchEventArgs
                        {
                            StartPoint = curLocation,
                            PreviousPoint = curLocation,
                            CurrentPoint = curLocation,
                            FingerId = args.Id,
                            IsOver = false,
                            LongPressHappend = false
                        };

                        _fingerTouchInfos.Add(args.Id, touchInfo);
                        //BaseApplication.LogDebug($"加入FingerTouchInfo Figure: {this.GetType().Name}, Events: {SerializeUtil.ToJson(args)}");

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
                case SKTouchAction.Moved:
                    {
                        if (!EnableDrag || !EnableTouch)
                        {
                            return;
                        }

                        if (!_fingerTouchInfos.TryGetValue(args.Id, out SKFigureTouchEventArgs? touchInfo))
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
                case SKTouchAction.Exited:
                case SKTouchAction.Released:
                    {
                        //BaseApplication.LogDebug($"进入Exitted,Released. Figure: {this.GetType().Name}, Events: {SerializeUtil.ToJson(args)}");

                        if (ResumeResponseTimeTickAfterTouch)
                        {
                            CanResponseTimeTick = true;
                        }

                        if (!_fingerTouchInfos.TryGetValue(args.Id, out SKFigureTouchEventArgs? touchInfo))
                        {
                            return;
                        }

                        CancelLongTap(args);

                        if (touchInfo.IsOver)
                        {
                            //BaseApplication.LogDebug($"移除FingerTouchInfo Figure: {this.GetType().Name}, Events: {SerializeUtil.ToJson(args)}");
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

                        //BaseApplication.LogDebug($"移除FingerTouchInfo Figure: {this.GetType().Name}, Events: {SerializeUtil.ToJson(args)}");
                        _fingerTouchInfos.Remove(args.Id);

                        break;
                    }
                case SKTouchAction.Cancelled:
                    {
                        //BaseApplication.LogDebug($"进入Cancelled. Figure: {this.GetType().Name}, Events: {SerializeUtil.ToJson(args)}");
                        if (ResumeResponseTimeTickAfterTouch)
                        {
                            CanResponseTimeTick = true;
                        }

                        if (!_fingerTouchInfos.TryGetValue(args.Id, out SKFigureTouchEventArgs? touchInfo))
                        {
                            return;
                        }

                        CancelLongTap(args);

                        if (!touchInfo.IsOver)
                        {
                            touchInfo.IsOver = true;

                            OnCancelled(touchInfo);
                        }

                        //BaseApplication.LogDebug($"移除FingerTouchInfo Figure: {this.GetType().Name}, Events: {SerializeUtil.ToJson(args)}");
                        _fingerTouchInfos.Remove(args.Id);

                        break;
                    }
                default:
                    break;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "<Pending>")]
        public void ProcessUnTouchAction(long fingerId, SKPoint location)
        {
            OnHitFailed();
        }

        private void CancelLongTap(SKTouchEventArgs args)
        {
            if (EnableLongTap && _longTouchInfos.TryGetValue(args.Id, out LongTouchTaskInfo? taskWrapper))
            {
                taskWrapper.CancellationTokenSource.Cancel();
                taskWrapper.CancellationTokenSource.Dispose();

                _longTouchInfos.Remove(args.Id);
            }
        }

        private Task LongPressedTaskAsync(SKFigureTouchEventArgs info, CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                await Task.Delay(Conventions.LongTapMinDurationInMilliseconds).ConfigureAwait(false);

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

        public event EventHandler<SKFigureTouchEventArgs> Pressed
        {
            add => _weakEventManager.AddEventHandler(value, nameof(Pressed));
            remove => _weakEventManager.RemoveEventHandler(value, nameof(Pressed));
        }

        public event EventHandler<SKFigureTouchEventArgs> LongTapped
        {
            add => _weakEventManager.AddEventHandler(value, nameof(LongTapped));
            remove => _weakEventManager.RemoveEventHandler(value, nameof(LongTapped));
        }

        public event EventHandler<SKFigureTouchEventArgs> Tapped
        {
            add => _weakEventManager.AddEventHandler(value, nameof(Tapped));
            remove => _weakEventManager.RemoveEventHandler(value, nameof(Tapped));
        }

        public event EventHandler<SKFigureTouchEventArgs> OneFingerDragged
        {
            add => _weakEventManager.AddEventHandler(value, nameof(OneFingerDragged));
            remove => _weakEventManager.RemoveEventHandler(value, nameof(OneFingerDragged));
        }

        public event EventHandler<SKFigureTouchEventArgs> TwoFingerDragged
        {
            add => _weakEventManager.AddEventHandler(value, nameof(TwoFingerDragged));
            remove => _weakEventManager.RemoveEventHandler(value, nameof(TwoFingerDragged));
        }

        public event EventHandler<SKFigureTouchEventArgs> Cancelled
        {
            add => _weakEventManager.AddEventHandler(value, nameof(Cancelled));
            remove => _weakEventManager.RemoveEventHandler(value, nameof(Cancelled));
        }

        public event EventHandler? HitFailed
        {
            add => _weakEventManager.AddEventHandler(value, nameof(HitFailed));
            remove => _weakEventManager.RemoveEventHandler(value, nameof(HitFailed));
        }

        public void OnPressed(SKFigureTouchEventArgs touchInfo)
        {
            _weakEventManager.HandleEvent(this, touchInfo, nameof(Pressed));
        }

        public void OnOneFingerDragged(SKFigureTouchEventArgs touchInfo)
        {
            _weakEventManager.HandleEvent(this, touchInfo, nameof(OneFingerDragged));

            if (State == FigureState.Selected || State == FigureState.LongSelected)
            {
                return;
            }

            SetState(FigureState.Selected);
        }

        public void OnTwoFingerDragged(SKFigureTouchEventArgs touchInfo)
        {
            _weakEventManager.HandleEvent(this, touchInfo, nameof(TwoFingerDragged));

            if (State == FigureState.Selected || State == FigureState.LongSelected)
            {
                return;
            }

            SetState(FigureState.Selected);
        }

        public void OnTapped(SKFigureTouchEventArgs touchInfo)
        {
            _weakEventManager.HandleEvent(this, touchInfo, nameof(Tapped));

            if (State == FigureState.Selected)
            {
                SetState(FigureState.None);
            }
            else if (State == FigureState.None)
            {
                SetState(FigureState.Selected);
            }
        }

        public void OnLongTapped(SKFigureTouchEventArgs touchInfo)
        {
            _weakEventManager.HandleEvent(this, touchInfo, nameof(LongTapped));

            SetState(FigureState.LongSelected);
        }

        public void OnCancelled(SKFigureTouchEventArgs touchInfo)
        {
            _weakEventManager.HandleEvent(this, touchInfo, nameof(Cancelled));

            //SetState(FigureState.None);
        }

        public void OnHitFailed()
        {
            _weakEventManager.HandleEvent(this, EventArgs.Empty, nameof(HitFailed));

            if (Parent is SKFigureGroup collection)
            {
                if (collection.EnableMultipleSelected)
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
                    _hitTestPath.Dispose();
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
