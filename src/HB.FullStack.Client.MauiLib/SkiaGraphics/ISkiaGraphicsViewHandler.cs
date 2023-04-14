

using Microsoft.Maui;

namespace HB.FullStack.Client.MauiLib.SkiaGraphics
{
    public partial interface ISkiaGraphicsViewHandler : IViewHandler
    {
        new IGraphicsView VirtualView { get; }
        new SkiaTouchGraphicsView PlatformView { get; }
    }
}
