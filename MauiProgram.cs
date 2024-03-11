using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace StudyingImprovement
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiCommunityToolkit()
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            Microsoft.Maui.Handlers.WebViewHandler.Mapper.AppendToMapping("MyCustomization", (handler, view) =>
            {
#if ANDROID
                handler.PlatformView.Settings.DomStorageEnabled = true;
                handler.PlatformView.Settings.JavaScriptEnabled=true;
#endif
            });
            return builder.Build();
        }
    }
}
