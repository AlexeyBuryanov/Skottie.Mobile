using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Skottie.Mobile.Playground.Maui;

public class App : Application
{
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var page = new MainPage();
        var window = new Window(page);
        window.Stopped += (_, _) => page.SuspendPlayback();
        window.Resumed += (_, _) => page.ResumePlayback();
        window.Destroying += (_, _) => page.ReleaseAnimation();
        return window;
    }
}
