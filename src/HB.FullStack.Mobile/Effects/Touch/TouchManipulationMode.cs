using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Mobile.Effects.Touch
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
