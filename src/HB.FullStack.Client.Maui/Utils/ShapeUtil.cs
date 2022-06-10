using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Shapes
{
    public static class ShapeUtil
    {
        public static Point GetCenter(this Rectangle rect)
        {
            return new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
        }
    }
}
