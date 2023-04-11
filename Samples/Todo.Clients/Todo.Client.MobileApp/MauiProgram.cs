using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;

using Microsoft.Extensions.Logging;

namespace Todo.Client.MobileApp
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp()
		{
			var builder = MauiApp.CreateBuilder();
			builder
				.UseMauiApp<App>()
				.UseMauiCommunityToolkit()
				.UseMauiCommunityToolkitMarkup()
				.UseMauiCommunityToolkitMediaElement()
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
					fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				});

#if DEBUG
		builder.Logging.AddDebug();
#endif

			return builder.Build();
		}
	}
}