
using SkiaSharp;

using Xamarin.Forms;

namespace HB.FullStack.XamarinForms.Skia.Generic
{
    public abstract class SKFigure<TDrawData> : SKFigure where TDrawData : SKFigureDrawData
    {
        public static BindableProperty InitDrawDataProperty = BindableProperty.Create(
                    nameof(InitDrawData),
                    typeof(TDrawData),
                    typeof(SKFigure<TDrawData>),
                    null,
                    BindingMode.OneWay,
                    propertyChanged: (b, oldValue, newValue) => ((SKFigure<TDrawData>)b).OnInitDrawDataChanged((TDrawData?)oldValue, (TDrawData?)newValue));

        public static BindableProperty ResultDrawDataProperty = BindableProperty.Create(
                    nameof(ResultDrawData),
                    typeof(TDrawData),
                    typeof(SKFigure<TDrawData>),
                    null,
                    BindingMode.OneWayToSource);

        public TDrawData? InitDrawData { get => (TDrawData?)GetValue(InitDrawDataProperty); set => SetValue(InitDrawDataProperty, value); }

        public TDrawData? ResultDrawData { get => (TDrawData?)GetValue(ResultDrawDataProperty); set => SetValue(ResultDrawDataProperty, value); }

        protected bool HitPathNeedUpdate { get; set; }

        private void OnInitDrawDataChanged(TDrawData? oldValue, TDrawData? newValue)
        {
            HitPathNeedUpdate = true;
            InvalidateMatrixAndSurface();
        }

        protected override void OnDraw(SKImageInfo info, SKCanvas canvas)
        {
            if (InitDrawData == null)
            {
                return;
            }

            OnDraw(info, canvas, InitDrawData);
        }

        protected override void OnUpdateHitTestPath(SKImageInfo info)
        {
            if (InitDrawData == null)
            {
                return;
            }

            if (CanvasSizeChanged || HitPathNeedUpdate)
            {
                HitPathNeedUpdate = false;

                HitTestPath.Reset();

                OnUpdateHitTestPath(info, InitDrawData);
            }
        }

        protected override void OnCaculateOutput()
        {
            if (InitDrawData == null)
            {
                return;
            }

            OnCaculateOutput(out TDrawData? newResult, InitDrawData);

            if (newResult != ResultDrawData)
            {
                ResultDrawData = newResult;
            }
        }

        protected abstract void OnDraw(SKImageInfo info, SKCanvas canvas, TDrawData initDrawData);

        protected abstract void OnUpdateHitTestPath(SKImageInfo info, TDrawData initDrawData);

        /// <summary>
        /// 返回是否需要重新赋值结果
        /// </summary>
        /// <param name="newResultDrawData"></param>
        /// <param name="initDrawData"></param>
        /// <returns></returns>
        protected abstract void OnCaculateOutput(out TDrawData? newResultDrawData, TDrawData initDrawData);
    }
}
