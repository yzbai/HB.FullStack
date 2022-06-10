using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Skia;

using SkiaSharp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Client.Maui.Controls.SkiaGraphics
{
    public abstract class SkiaDrawable : IDrawable
    {
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            ScalingCanvas scalingCanvas = (ScalingCanvas)canvas;
            SKCanvas sKCanvas = ((SkiaCanvas)scalingCanvas.ParentCanvas).Canvas;

            float scale = scalingCanvas.GetScale();
            
            SkiaDraw(canvas, sKCanvas, dirtyRect, scale);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="skCanvas"></param>
        /// <param name="dirtyRect"></param>
        /// <param name="skScale">使用的SK数据，乘以这个倍数与Maui一致</param>
        public abstract void SkiaDraw(ICanvas canvas, SKCanvas skCanvas, RectF dirtyRect, float skScale);
    }
}
