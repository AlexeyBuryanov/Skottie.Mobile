using Microsoft.Maui.Hosting;

namespace Skottie.Mobile.Maui;

public static class MauiAppBuilderExtensions
{
    /// <summary>Registers the Skottie animation handler for Android and iOS.</summary>
    public static MauiAppBuilder UseSkottie(this MauiAppBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.ConfigureMauiHandlers(handlers =>
            handlers.AddHandler<SkottieAnimationView, SkottieAnimationViewHandler>());
    }
}
