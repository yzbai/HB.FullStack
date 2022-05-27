using HB.FullStack.Common.Figures;

using Microsoft.Maui.Controls;

using SkiaSharp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Client.Maui.Skia
{
    public abstract class SKFigure<TDrawInfo, TData> : SKFigure
        where TDrawInfo : FigureDrawInfo
        where TData : FigureData
    {
        public static BindableProperty DrawInfoProperty = BindableProperty.Create(
                    nameof(DrawInfo),
                    typeof(TDrawInfo),
                    typeof(SKFigure<TDrawInfo, TData>),
                    null,
                    BindingMode.OneWay,
                    propertyChanged: (b, oldValue, newValue) => ((SKFigure<TDrawInfo, TData>)b).OnBaseDrawDataChanged());

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

        public TDrawInfo? DrawInfo { get => (TDrawInfo?)GetValue(DrawInfoProperty); set => SetValue(DrawInfoProperty, value); }

        public TData? InitData { get => (TData?)GetValue(InitDataProperty); set => SetValue(InitDataProperty, value); }

        public TData? ResultData { get => (TData?)GetValue(ResultDataProperty); set => SetValue(ResultDataProperty, value); }

        private void OnBaseDrawDataChanged()
        {
            HitTestPathNeedUpdate = true;

            OnDrawInfoOrCanvasSizeChanged();

            RestoreMatrix();
        }

        private void OnBaseInitDataChanged()
        {
            HitTestPathNeedUpdate = true;

            OnInitDataChanged();

            RestoreMatrix();
        }

        protected abstract void OnInitDataChanged();

        protected override void OnCanvasSizeChanged(SKSize oldCanvasSize, SKSize canvasSize)
        {
            OnDrawInfoOrCanvasSizeChanged();
        }

        /// <summary>
        /// 计算因为DrawInfo或者CanvasSize发生变化，引起的绘画数据变化
        /// </summary>
        protected abstract void OnDrawInfoOrCanvasSizeChanged();

        protected sealed override void CaculateOutput()
        {
            CaculateOutput(out TData? newResult);

            if (newResult != null)
            {
                newResult.VisualState = VisualState;
            }

            if (newResult != ResultData)
            {
                ResultData = newResult;
            }
        }

        protected abstract void CaculateOutput(out TData? newResultData);
    }

    public abstract class SKDrawFigure<TDrawInfo> : SKFigure<TDrawInfo, EmptyData> where TDrawInfo : FigureDrawInfo
    {
        protected override void CaculateOutput(out EmptyData? newResultData)
        {
            newResultData = null;
        }

        protected override void OnInitDataChanged()
        {
            //Do nothing
        }
    }

    public abstract class SKDataFigure<TData> : SKFigure<EmptyDrawInfo, TData> where TData : FigureData
    {
    }

    public class EmptyDrawInfo : FigureDrawInfo
    {
        protected override bool EqualsCore(FigureDrawInfo other)
        {
            return other is EmptyDrawInfo;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(nameof(EmptyDrawInfo));
        }
    }

    public class EmptyData : FigureData
    {
        protected override bool EqualsCore(FigureData other)
        {
            return other is EmptyData;
        }

        protected override int GetHashCodeCore()
        {
            return HashCode.Combine(nameof(EmptyData));
        }
    }
}
