using HB.FullStack.Common.Figures;

using Microsoft.Maui.Controls;

using SkiaSharp;
using SkiaSharp.Views.Maui;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace HB.FullStack.Client.Maui.Figures
{
    public interface ISKFigureGroupController
    {
        bool EnableMultiple { get; set; }

        IEnumerable<SKFigure> GetFiguresByGroup(string groupName);

        void AddToGroup(string groupName, SKFigure figure);
        
        void RemoveFromGroup(string groupName, SKFigure figure);

        void SetGroupVisualState(string groupName, FigureVisualState visualState);
        
        void NotifyVisualStateChanged(SKFigure figure);
    }
}
