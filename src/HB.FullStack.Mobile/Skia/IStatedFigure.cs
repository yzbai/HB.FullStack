using System;
using System.Collections.Generic;
using System.Text;

namespace HB.FullStack.Mobile.Skia
{
    public interface IStatedFigure
    {
        void SetState(FigureState state);
    }

    public enum FigureState
    {
        None,
        Tapped,
        LongTapped,
        Dragged,
        UnSelected,
    }
}
