using Microsoft.Maui;
using Microsoft.Maui.Handlers;
#if ANDROID
using NativeAnimationView = Skottie.Mobile.Android.SkottieAnimationView;
#elif IOS
using NativeAnimationView = Skottie.Mobile.iOS.SkottieAnimationView;
#endif

namespace Skottie.Mobile.Maui;

public class SkottieAnimationViewHandler : ViewHandler<SkottieAnimationView, NativeAnimationView>
{
    public static readonly IPropertyMapper<SkottieAnimationView, SkottieAnimationViewHandler> Mapper =
        new PropertyMapper<SkottieAnimationView, SkottieAnimationViewHandler>(ViewMapper)
        {
            [nameof(SkottieAnimationView.Source)] = MapSource,
            [nameof(SkottieAnimationView.IsPlaying)] = MapIsPlaying,
        };

    public SkottieAnimationViewHandler() : base(Mapper)
    {
    }

    protected override NativeAnimationView CreatePlatformView()
    {
#if ANDROID
        return new NativeAnimationView(Context);
#elif IOS
        return new NativeAnimationView();
#endif
    }

    protected override void ConnectHandler(NativeAnimationView platformView)
    {
        base.ConnectHandler(platformView);
        platformView.LoadFailed += OnLoadFailed;
    }

    protected override void DisconnectHandler(NativeAnimationView platformView)
    {
        platformView.LoadFailed -= OnLoadFailed;
        platformView.Clear();
        base.DisconnectHandler(platformView);
    }

    private void OnLoadFailed(object? sender, Exception exception) => VirtualView.OnLoadFailed(exception);

    public static void MapSource(SkottieAnimationViewHandler handler, SkottieAnimationView view)
    {
        var source = view.Source;
        if (string.IsNullOrWhiteSpace(source))
        {
            handler.PlatformView.Clear();
            return;
        }

        if (Path.IsPathRooted(source))
        {
            handler.PlatformView.Init(source);
        }
        else
        {
#if ANDROID
            handler.PlatformView.InitFromAsset(source);
#elif IOS
            handler.PlatformView.InitFromBundle(source);
#endif
        }

        MapIsPlaying(handler, view);
    }

    public static void MapIsPlaying(SkottieAnimationViewHandler handler, SkottieAnimationView view)
    {
        if (view.IsPlaying)
        {
            handler.PlatformView.Start();
        }
        else
        {
            handler.PlatformView.Stop();
        }
    }
}
