using Microsoft.Maui.Hosting;
using Skottie.Mobile.Maui;

namespace Skottie.Mobile.Playground.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp() => MauiApp.CreateBuilder()
        .UseMauiApp<App>()
        .UseSkottie()
        .Build();
}
