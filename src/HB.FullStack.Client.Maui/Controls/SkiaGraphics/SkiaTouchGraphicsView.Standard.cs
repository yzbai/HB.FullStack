using Microsoft.Maui;
using Microsoft.Maui.Graphics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Client.Maui.Controls.SkiaGraphics
{
    public partial class SkiaTouchGraphicsView
    {
        public IDrawable? Drawable { get; internal set; }

        internal void Connect(IGraphicsView virtualView)
        {
            throw new NotImplementedException();
        }

        internal void Disconnect()
        {
            throw new NotImplementedException();
        }

        internal void Invalidate()
        {
            throw new NotImplementedException();
        }
    }
}
