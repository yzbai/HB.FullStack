using HB.FullStack.Common.Figures;

using Microsoft.Maui.Controls;

using SkiaSharp;
using SkiaSharp.Views.Maui;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace HB.FullStack.Client.Maui.Skia
{
    internal interface ISKFigureGroupController
    {
        bool EnableMultipleSelected { get; set; }

        bool EnableMultipleLongSelected { get; set; }

        bool EnableUnSelectedByHitFailed { get; set; }

        void AddToGroup(string groupName, SKFigure figure);
        
        void RemoveFromGroup(string groupName, SKFigure figure);

        void SetVisualState(string groupName, VisualState visualState);
    }

    public abstract class SKFigureGroupController : ISKFigureGroupController
    {
        //TODO: 添加最多选择几个
        public bool EnableMultipleSelected { get; set; }

        public bool EnableMultipleLongSelected { get; set; }

        public bool EnableUnSelectedByHitFailed { get; set; } = true;

        protected IList<SKFigure> Figures { get; } = new List<SKFigure>();

        protected Dictionary<long, SKDataFigure<TData>> HittingFigures { get; } = new Dictionary<long, SKDataFigure<TData>>();

        public IList<SKDataFigure<TData>> SelectedFigures { get; } = new List<SKDataFigure<TData>>();

        public FigureVisualState SelectedFiguresState { get; private set; }



     

        public void UnSelect(SKFigure> figure)
        {
            SelectedFigures.Remove(figure);

            figure.SetVisualState(FigureVisualState.None);
        }

        public void UnSelectAll()
        {
            foreach (SKFigure f in SelectedFigures)
            {
                f.SetVisualState(FigureVisualState.None);
            }

            SelectedFigures.Clear();
        }

        private void CheckSelected(SKDataFigure<TData> figure)
        {
            if (figure.VisualState != FigureVisualState.Selected && figure.VisualState != FigureVisualState.LongSelected)
            {
                return;
            }

            if (SelectedFiguresState != figure.VisualState
                || (figure.VisualState == FigureVisualState.Selected && !EnableMultipleSelected)
                || (figure.VisualState == FigureVisualState.LongSelected && !EnableMultipleLongSelected))
            {
                UnSelectAllExcept(figure);
            }
            else
            {
                SelectedFigures.Add(figure);
            }

            void UnSelectAllExcept(SKDataFigure<TData> figure)
            {
                foreach (SKFigure sf in SelectedFigures)
                {
                    if (sf == figure)
                    {
                        continue;
                    }

                    sf.SetVisualState(FigureVisualState.None);
                }

                SelectedFigures.Clear();
                SelectedFiguresState = figure.VisualState;
                SelectedFigures.Add(figure);
            }
        }

        #region 事件派发



        private void OnDragged(object? sender, SKFigureTouchEventArgs info)
        {
            if (!HittingFigures.TryGetValue(info.FingerId, out SKDataFigure<TData>? figure))
            {
                return;
            }

            figure.OnOneFingerDragged(info);

            CheckSelected(figure);

            if (info.IsOver)
            {
                HittingFigures.Remove(info.FingerId);
            }
        }

        private void OnLongTapped(object? sender, SKFigureTouchEventArgs info)
        {
            if (!HittingFigures.TryGetValue(info.FingerId, out SKDataFigure<TData>? figure))
            {
                return;
            }

            figure.OnLongTapped(info);

            CheckSelected(figure);

            if (info.IsOver)
            {
                HittingFigures.Remove(info.FingerId);
            }
        }

        private void OnTapped(object? sender, SKFigureTouchEventArgs info)
        {
            if (!HittingFigures.TryGetValue(info.FingerId, out SKDataFigure<TData>? figure))
            {
                return;
            }

            figure.OnTapped(info);

            CheckSelected(figure);

            if (info.IsOver)
            {
                HittingFigures.Remove(info.FingerId);
            }
        }

        private void OnCancelled(object? sender, SKFigureTouchEventArgs info)
        {
            if (!HittingFigures.TryGetValue(info.FingerId, out SKDataFigure<TData>? figure))
            {
                return;
            }

            figure.OnCancelled(info);

            //CheckSelected(figure);

            if (info.IsOver)
            {
                HittingFigures.Remove(info.FingerId);
            }
        }

        private void OnHitFailed(object? sender, EventArgs e)
        {
            HittingFigures.Clear();

            if (EnableUnSelectedByHitFailed)
            {
                UnSelectAll();
            }

            foreach (SKDataFigure<TData> figure in Figures)
            {
                figure.OnHitFailed();
            }
        }

        #endregion
    }
}
