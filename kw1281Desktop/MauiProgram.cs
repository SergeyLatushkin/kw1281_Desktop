using BitFab.KW1281Test;
using CommunityToolkit.Maui;
using kw1281Desktop.Services.implementation;
using kw1281Desktop.Services.Implementation;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.UI.Windowing;
using Syncfusion.Maui.Toolkit.Hosting;
using System.Runtime.Versioning;

namespace kw1281Desktop
{
    public static class MauiProgram
    {
        [SupportedOSPlatform("windows10.0.19041.0")]
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureSyncfusionToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
                    fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
                })
                .ConfigureLifecycleEvents(events =>
                {
                    events.AddWindows(w =>
                    {
                        w.OnWindowCreated(window =>
                        {
                            AppWindow nativeWindow = window.AppWindow;

                            if (nativeWindow.Presenter is OverlappedPresenter presenter)
                            {
                                presenter.PreferredMinimumWidth = 1400;
                                presenter.PreferredMinimumHeight = 800;
                            }
                        });
                    });
                });

#if DEBUG
            builder.Logging.AddDebug();
            builder.Services.AddLogging(configure => configure.AddDebug());
#endif

            builder.Services.AddSingleton<IErrorHandler, ModalErrorHandler>();
            builder.Services.AddSingleton<ILoaderService, LoaderService>();

            builder.Services.AddSingleton<Diagnostic>();

            builder.Services.AddSingleton<AdaptationPageViewModel>();
            builder.Services.AddSingleton<AppSettingsPageViewModel>();
            builder.Services.AddSingleton<AutoscanPageViewModel>();
            builder.Services.AddSingleton<DumpPageViewModel>();
            builder.Services.AddSingleton<GroupReadPageViewModel>();
            builder.Services.AddSingleton<ReadFaultCodesPageViewModel>();
            builder.Services.AddSingleton<UtilsPageViewModel>();


            return builder.Build();
        }
    }
}
