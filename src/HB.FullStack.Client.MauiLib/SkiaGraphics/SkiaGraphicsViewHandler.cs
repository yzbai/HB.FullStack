using Microsoft.Maui;
using Microsoft.Maui.Handlers;

using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;

namespace HB.FullStack.Client.MauiLib.SkiaGraphics
{
    public partial class SkiaGraphicsViewHandler : ViewHandler<IGraphicsView, SkiaTouchGraphicsView>, ISkiaGraphicsViewHandler
    {
        public static IPropertyMapper<IGraphicsView, ISkiaGraphicsViewHandler> Mapper = new PropertyMapper<IGraphicsView, ISkiaGraphicsViewHandler>(ViewMapper)
        {
            [nameof(IGraphicsView.Drawable)] = MapDrawable,
            [nameof(IView.FlowDirection)] = MapFlowDirection
        };

        public static CommandMapper<IGraphicsView, ISkiaGraphicsViewHandler> CommandMapper = new(ViewCommandMapper)
        {
            [nameof(IGraphicsView.Invalidate)] = MapInvalidate
        };

        public SkiaGraphicsViewHandler() : base(Mapper, CommandMapper)
        {

        }

        public SkiaGraphicsViewHandler(IPropertyMapper? mapper = null, CommandMapper? commandMapper = null)
            : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
        {

        }

        IGraphicsView ISkiaGraphicsViewHandler.VirtualView => VirtualView;

        SkiaTouchGraphicsView ISkiaGraphicsViewHandler.PlatformView => PlatformView;

        protected override void ConnectHandler(SkiaTouchGraphicsView platformView)
        {
            platformView.Connect(VirtualView);
            base.ConnectHandler(platformView);
        }
        protected override void DisconnectHandler(SkiaTouchGraphicsView platformView)
        {
            platformView.Disconnect();
            base.DisconnectHandler(platformView);
        }

        protected override SkiaTouchGraphicsView CreatePlatformView()
        {
#if ANDROID
            return new SkiaTouchGraphicsView(Context);
#else
            return new SkiaTouchGraphicsView();
#endif
        }

        public static void MapDrawable(ISkiaGraphicsViewHandler handler, IGraphicsView graphicsView)
        {
            if (handler.PlatformView != null)
            {
                handler.PlatformView.Drawable = graphicsView.Drawable;
            }
        }

        public static void MapFlowDirection(ISkiaGraphicsViewHandler handler, IGraphicsView graphicsView)
        {
            handler.PlatformView?.UpdateFlowDirection(graphicsView);
            handler.PlatformView?.Invalidate();
        }

        public static void MapInvalidate(ISkiaGraphicsViewHandler handler, IGraphicsView graphicsView, object? arg)
        {
            handler.PlatformView?.Invalidate();
        }
    }
}
