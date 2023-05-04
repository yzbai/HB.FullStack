using HB.FullStack.Common;
using HB.FullStack.Common.Figures;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;

using SkiaSharp;
using SkiaSharp.Views.Maui;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HB.FullStack.Client.MauiLib.Figures
{
    public abstract class SKFigure : BindableObject, IDisposable
    {
        public const int LONG_TAP_MIN_DURATION_IN_MILLISECONDS = 400;

        public SKFigureCanvasView? CanvasView { get; set; }

        public ISKFigureGroupController? GroupController { get; set; }

        public string? GroupName { get; set; }

        public Guid Id { get; } = Guid.NewGuid();

        public bool EnableTimeTick { get; set; }

        public bool CanResponseTimeTick { get; set; } = true;

        public bool StopResponseTimeTickWhenTouch { get; set; } = true;

        public bool ResumeResponseTimeTickAfterTouch { get; set; } = true;

        public bool EnableTouch { get; set; } = true;

        public bool EnableDrag { get; set; } = true;

        public bool EnableTwoFingers { get; set; }

        public bool EnableLongTap { get; set; } = true;

        /// <summary>
        ///坐标系原点
        /// </summary>
        public RatioPoint CoordinateOrigin { get; init; } = new RatioPoint(0.5f, 0.5f);

        public SKSize CanvasSize { get; private set; }

        /// <summary>
        /// CanvasSize初始为0，所以第一次Paint的时候CanvasSizeChanged为true
        /// </summary>
        //public bool CanvasSizeChanged { get; set; }

        SKPath _hitTestPathBB = new SKPath();
        public SKPath HitTestPath { get => _hitTestPathBB; set { _hitTestPathBB?.Dispose(); _hitTestPathBB = value; } }

        /// <summary>
        /// 当Canvas的Size发生变化，或者样子发生变法，仅仅是Matrix的变化，不需要更新
        /// </summary>
        protected bool HitTestPathNeedUpdate { get; set; } = true;

        /// <summary>
        /// 主要用来记录Touch带来的变化，OnInitDataChanged中，不要改变
        /// </summary>
        [SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "<Pending>")]
        protected SKMatrix Matrix = SKMatrix.CreateIdentity();

        protected SKFigure()
        {
            BindVisualStateChangeToEvents();
        }

        public void InvalidateSurface() => CanvasView?.InvalidateSurface();

        public void RestoreMatrix()
        {
            Matrix = SKMatrix.CreateIdentity();

            InvalidateSurface();
        }

        public virtual void OnPaint(SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            if (CanvasSize != info.Size)
            {
                SKSize oldSize = CanvasSize;
                CanvasSize = info.Size;
                OnCanvasSizeChanged(oldSize, CanvasSize);

                HitTestPathNeedUpdate = true;
            }

            if (EnableTimeTick && CanResponseTimeTick)
            {
                CaculateMatrixByTime(CanvasView!.ElapsedMilliseconds);
            }

            //set canvas
            canvas.Translate(CoordinateOrigin.ToSKPoint(info.Size));
            canvas.Concat(ref Matrix);

            //draw
            OnDraw(info, canvas);

            //Update
            if (HitTestPathNeedUpdate)
            {
                HitTestPath = CaculateHitTestPath(info);
                HitTestPathNeedUpdate = false;
            }

            CaculateOutput();
        }

        protected virtual void OnCanvasSizeChanged(SKSize oldCanvasSize, SKSize newCanvasSize)
        {
            OnDrawInfoIntialized();
        }

        /// <summary>
        /// step 1:只有第一次，或者CanvasSize发生变化或者绘画要求发生变化才会调用。这里可以初始化一些不太变动的数据。
        /// </summary>
        protected abstract void OnDrawInfoIntialized();

        /// <summary>
        /// step 2: 时间对Matrix的影响
        /// </summary>
        protected virtual void CaculateMatrixByTime(long elapsedMilliseconds) { }

        /// <summary>
        /// step 3: 绘制
        /// </summary>
        protected abstract void OnDraw(SKImageInfo info, SKCanvas canvas);

        /// <summary>
        /// step 4: 计算点击区域
        /// </summary>
        protected abstract SKPath CaculateHitTestPath(SKImageInfo info);

        /// <summary>
        /// step 5: 计算输出
        /// </summary>
        protected abstract void CaculateOutput();

        #region HitTest

        /// <summary>
        /// 由Parent调用，重写以实现更复杂的点击效果，比如记录点击位置
        /// </summary>
        /// <param name="canvasPoint">原始坐标系下的点</param>
        /// <param name="fingerId">第几个指头</param>
        public virtual bool HitTest(SKPoint canvasPoint, long fingerId)
        {
            if (!EnableTouch)
            {
                return false;
            }

            #region 将CanvasView下的点，转换为当前Figure的点

            SKPoint hitPoint = ToCurrentCoordinatePoint(canvasPoint);

            if (!Matrix.TryInvert(out SKMatrix inversedMatrix))
            {
                return false;
            }

            SKPoint mappedPoint = inversedMatrix.MapPoint(hitPoint);

            #endregion

            return IsHitted(mappedPoint);

            bool IsHitted(SKPoint figurePoint)
            {
                if (HitTestPath.IsNullOrEmpty())
                {
                    return false;
                }

                return HitTestPath.Contains(figurePoint.X, figurePoint.Y);
            }
        }

        #endregion

        #region Touch to Events

        //TODO: 抽象出一个TouchHandler来专门处理.数据和行为分离

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

            SKPoint canvasLocation = args.Location;// SKUtil.ToSKPoint(args.DpLocation);

            SKPoint curLocation = ToCurrentCoordinatePoint(canvasLocation);

            //BaseApplication.LogDebug($"开始处理Touch Figure: {this.GetType().EndpointName}, Events: {SerializeUtil.ToJson(args)}");

            switch (args.ActionType)
            {
                case SKTouchAction.Pressed:
                    {
                        if (StopResponseTimeTickWhenTouch)
                        {
                            CanResponseTimeTick = false;
                        }

                        SKFigureTouchEventArgs figureTouchInfo = new SKFigureTouchEventArgs
                        {
                            StartPoint = curLocation,
                            PreviousPoint = curLocation,
                            CurrentPoint = curLocation,
                            FingerId = args.Id,
                            IsOver = false,
                            LongPressHappend = false
                        };

                        _fingerTouchInfos.Add(args.Id, figureTouchInfo);
                        //BaseApplication.LogDebug($"加入FingerTouchInfo Figure: {this.GetType().EndpointName}, Events: {SerializeUtil.ToJson(args)}");

                        if (EnableLongTap)
                        {
                            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

                            _longTouchInfos[args.Id] = new LongTouchTaskInfo
                            {
                                CancellationTokenSource = cancellationTokenSource,
                                Task = LongPressedTaskAsync(figureTouchInfo, cancellationTokenSource.Token)
                            };
                        }

                        OnPressed(figureTouchInfo);

                        break;
                    }
                case SKTouchAction.Moved:
                    {
                        if (!EnableDrag || !EnableTouch)
                        {
                            return;
                        }

                        if (!_fingerTouchInfos.TryGetValue(args.Id, out SKFigureTouchEventArgs? figureTouchInfo))
                        {
                            return;
                        }

                        if (figureTouchInfo.IsOver)
                        {
                            return;
                        }

                        figureTouchInfo.CurrentPoint = curLocation;

                        if (figureTouchInfo.StartPoint == curLocation)
                        {
                            //相当于Press
                            //DO nothing
                            //华为真机会不停的Move在原地

                            return;
                        }

                        if (IsTooSmallMoved(figureTouchInfo.PreviousPoint, figureTouchInfo.CurrentPoint))
                        {
                            return;
                        }

                        CancelLongTap(args);

                        figureTouchInfo.FirstMove = figureTouchInfo.FirstMove == null;

                        if (_fingerTouchInfos.Count == 1)
                        {
                            OnOneFingerDragged(figureTouchInfo);
                        }
                        else if (EnableTwoFingers && _fingerTouchInfos.Count == 2)
                        {
                            figureTouchInfo.PivotPoint = _fingerTouchInfos.Where(p => p.Key != args.Id).First().Value.CurrentPoint;

                            OnTwoFingerDragged(figureTouchInfo);
                        }

                        figureTouchInfo.PreviousPoint = figureTouchInfo.CurrentPoint;

                        break;
                    }
                case SKTouchAction.Exited:
                case SKTouchAction.Released:
                    {
                        //BaseApplication.LogDebug($"进入Exitted,Released. Figure: {this.GetType().EndpointName}, Events: {SerializeUtil.ToJson(args)}");

                        if (ResumeResponseTimeTickAfterTouch)
                        {
                            CanResponseTimeTick = true;
                        }

                        if (!_fingerTouchInfos.TryGetValue(args.Id, out SKFigureTouchEventArgs? touchInfo))
                        {
                            return;
                        }

                        if (touchInfo.LongPressHappend || touchInfo.IsOver)
                        {
                            //LongTap已经发生
                            //BaseApplication.LogDebug($"移除FingerTouchInfo Figure: {this.GetType().EndpointName}, Events: {SerializeUtil.ToJson(args)}");
                            _fingerTouchInfos.Remove(args.Id);
                            return;
                        }

                        CancelLongTap(args);

                        touchInfo.IsOver = true;
                        touchInfo.CurrentPoint = curLocation;

                        if (IsTooSmallMoved(touchInfo.StartPoint, touchInfo.CurrentPoint))
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

                        //BaseApplication.LogDebug($"移除FingerTouchInfo Figure: {this.GetType().EndpointName}, Events: {SerializeUtil.ToJson(args)}");
                        _fingerTouchInfos.Remove(args.Id);

                        break;
                    }
                case SKTouchAction.Cancelled:
                    {
                        //BaseApplication.LogDebug($"进入Cancelled. Figure: {this.GetType().EndpointName}, Events: {SerializeUtil.ToJson(args)}");
                        if (ResumeResponseTimeTickAfterTouch)
                        {
                            CanResponseTimeTick = true;
                        }

                        if (!_fingerTouchInfos.TryGetValue(args.Id, out SKFigureTouchEventArgs? figureTouchInfo))
                        {
                            return;
                        }

                        CancelLongTap(args);

                        if (!figureTouchInfo.IsOver)
                        {
                            figureTouchInfo.IsOver = true;

                            OnCancelled(figureTouchInfo);
                        }

                        //BaseApplication.LogDebug($"移除FingerTouchInfo Figure: {this.GetType().EndpointName}, Events: {SerializeUtil.ToJson(args)}");
                        _fingerTouchInfos.Remove(args.Id);

                        break;
                    }
                default:
                    break;
            }
        }

        private bool IsTooSmallMoved(SKPoint previousPoint, SKPoint currentPoint)
        {
            throw new NotImplementedException();
        }

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
                await Task.Delay(LONG_TAP_MIN_DURATION_IN_MILLISECONDS);

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                info.LongPressHappend = true;

                OnLongTapped(info);

                info.IsOver = true;

                if (CanvasView != null)
                {
                    await CanvasView.Dispatcher.DispatchAsync(() => CanvasView?.InvalidateSurface());
                }

            }, cancellationToken);
        }

        #endregion

        #region Events

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

        public event EventHandler HitFailed
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
        }

        public void OnTwoFingerDragged(SKFigureTouchEventArgs touchInfo)
        {
            _weakEventManager.HandleEvent(this, touchInfo, nameof(TwoFingerDragged));
        }

        public void OnTapped(SKFigureTouchEventArgs touchInfo)
        {
            _weakEventManager.HandleEvent(this, touchInfo, nameof(Tapped));
        }

        public void OnLongTapped(SKFigureTouchEventArgs touchInfo)
        {
            _weakEventManager.HandleEvent(this, touchInfo, nameof(LongTapped));
        }

        public void OnCancelled(SKFigureTouchEventArgs touchInfo)
        {
            _weakEventManager.HandleEvent(this, touchInfo, nameof(Cancelled));
        }

        public void OnHitFailed()
        {
            _weakEventManager.HandleEvent(this, EventArgs.Empty, nameof(HitFailed));
        }

        #endregion

        #region VisualState

        public FigureVisualState LastVisualState { get; protected set; } = FigureVisualState.None;

        public FigureVisualState VisualState { get; internal set; } = FigureVisualState.None;

        private void BindVisualStateChangeToEvents()
        {
            OneFingerDragged += (sender, e) =>
            {
                //最后一个Drag
                if (e.IsOver)
                {
                    VisualState = LastVisualState;
                    LastVisualState = FigureVisualState.Dragging;

                    OnVisualStateChanged(LastVisualState, VisualState);

                    return;
                }

                //Drag中
                if (VisualState == FigureVisualState.Dragging)
                {
                    return;
                }

                //第一个Drag
                LastVisualState = VisualState;
                VisualState = FigureVisualState.Dragging;

                OnVisualStateChanged(LastVisualState, VisualState);
            };

            TwoFingerDragged += (sender, e) =>
            {
                //最后一个Drag
                if (e.IsOver)
                {
                    VisualState = LastVisualState;
                    LastVisualState = FigureVisualState.TwoFinglerDragging;
                    OnVisualStateChanged(LastVisualState, VisualState);
                    return;
                }

                //Drag中
                if (VisualState == FigureVisualState.TwoFinglerDragging)
                {
                    return;
                }

                //第一个Drag
                LastVisualState = VisualState;
                VisualState = FigureVisualState.TwoFinglerDragging;
                OnVisualStateChanged(LastVisualState, VisualState);
            };

            Tapped += (sender, e) =>
            {
                LastVisualState = VisualState;
                VisualState = VisualState == FigureVisualState.Tapped ? FigureVisualState.None : FigureVisualState.Tapped;
                OnVisualStateChanged(LastVisualState, VisualState);
            };

            LongTapped += (sender, e) =>
            {
                LastVisualState = VisualState;
                VisualState = FigureVisualState.LongTapped;
                OnVisualStateChanged(LastVisualState, VisualState);
            };

            HitFailed += (sender, e) =>
            {
                if (GroupController != null)
                {
                    if (GroupController.EnableMultiple)
                    {
                        return;
                    }
                }

                VisualState = FigureVisualState.None;

                OnVisualStateChanged(LastVisualState, VisualState);
            };
        }

        private void OnVisualStateChanged(FigureVisualState lastVisualState, FigureVisualState currentVisualState)
        {
            if (GroupController == null)
            {
                return;
            }

            GroupController.NotifyVisualStateChanged(this);
        }

        #endregion

        #region Util

        /// <summary>
        /// 转换为当前坐标系下的点
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        protected SKPoint ToCurrentCoordinatePoint(SKPoint point)
        {
            return new SKPoint(point.X - CanvasSize.Width * CoordinateOrigin.XRatio, point.Y - CanvasSize.Height * CoordinateOrigin.YRatio);
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
                    _hitTestPathBB.Dispose();
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

        internal class LongTouchTaskInfo
        {
            public Task Task { get; set; } = null!;

            public CancellationTokenSource CancellationTokenSource { get; set; } = null!;
        }
    }
}