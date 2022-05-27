using System;
using System.Collections.Generic;
using System.Text;

namespace HB.FullStack.Client.Maui.Skia
{
    public enum TouchManipulationMode
    {
        None,
        PanOnly,
        IsotropicScale,     // includes panning
        AnisotropicScale,   // includes panning
        ScaleRotate,        // implies isotropic scaling
        ScaleDualRotate     // adds one-finger rotation
    }
}
