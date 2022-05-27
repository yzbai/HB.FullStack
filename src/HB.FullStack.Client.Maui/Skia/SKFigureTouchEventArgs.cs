using SkiaSharp;

using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace HB.FullStack.Client.Maui.Skia
{
    public class SKFigureTouchEventArgs : EventArgs
    {
        /// <summary>
        /// 第几个指头
        /// </summary>
        public long FingerId { get; set; }

        /// <summary>
        /// 第一个Move事件
        /// </summary>
        public bool? FirstMove { get; set; }

        /// <summary>
        /// 结束事件/Released
        /// </summary>
        public bool IsOver { get; set; }

        /// <summary>
        /// 未经Figure的Matrix转换的新坐标系下的点
        /// </summary>
        public SKPoint StartPoint { get; set; }

        /// <summary>
        /// 未经Figure的Matrix转换的新坐标系下的点
        /// </summary>
        public SKPoint PreviousPoint { get; set; }

        /// <summary>
        /// 未经Figure的Matrix转换的新坐标系下的点
        /// </summary>
        public SKPoint CurrentPoint { get; set; }

        /// <summary>
        /// 两个指头操作时，不动的那个指头为支点，即另外一个指头
        /// </summary>
        public SKPoint PivotPoint { get; set; }

        public bool LongPressHappend { get; set; }

        public Task? LongPressTask { get; set; }
    }
}
