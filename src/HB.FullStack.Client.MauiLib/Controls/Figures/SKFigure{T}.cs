using HB.FullStack.Common.Figures;

using Microsoft.Maui.Controls;

using SkiaSharp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Client.Maui.Figures
{
    public abstract class SKFigure<TDrawInfo, TData> : SKFigure
        where TDrawInfo : IFigureDrawInfo
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

            OnDrawInfoIntialized();

            RestoreMatrix();
        }

        private void OnBaseInitDataChanged()
        {
            HitTestPathNeedUpdate = true;

            OnDrawInfoIntialized();

            RestoreMatrix();
        }

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

    public abstract class SKDrawFigure<TDrawInfo> : SKFigure<TDrawInfo, EmptyData> where TDrawInfo : IFigureDrawInfo
    {
        protected override void CaculateOutput(out EmptyData? newResultData)
        {
            newResultData = null;
        }
    }

    public abstract class SKDataFigure<TData> : SKFigure<EmptyDrawInfo, TData> where TData : FigureData
    {
    }

    public record EmptyDrawInfo : IFigureDrawInfo
    {
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
