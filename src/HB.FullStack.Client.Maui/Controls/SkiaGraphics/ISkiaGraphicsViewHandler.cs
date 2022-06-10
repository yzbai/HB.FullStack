using Microsoft.Maui;

namespace HB.FullStack.Client.Maui.Controls.SkiaGraphics
{
    public partial interface ISkiaGraphicsViewHandler : IViewHandler
    {
        new IGraphicsView VirtualView { get; }
        new SkiaTouchGraphicsView PlatformView { get; }
    }
}
