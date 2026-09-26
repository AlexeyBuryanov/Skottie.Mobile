using Foundation;
using UIKit;

namespace Skottie.Mobile.Playground.iOS;

[Register("SceneDelegate")]
public class SceneDelegate : UIWindowSceneDelegate
{
    public override UIWindow? Window { get; set; }

    public override void WillConnect(UIScene scene, UISceneSession session, UISceneConnectionOptions connectionOptions)
    {
        if (scene is not UIWindowScene windowScene)
            return;

        Window = new UIWindow(windowScene) { RootViewController = new PlaygroundViewController() };
        Window.MakeKeyAndVisible();
    }

    public override void WillResignActive(UIScene scene) =>
        (Window?.RootViewController as PlaygroundViewController)?.SuspendPlayback();

    public override void DidBecomeActive(UIScene scene) =>
        (Window?.RootViewController as PlaygroundViewController)?.ResumePlayback();

    public override void DidDisconnect(UIScene scene)
    {
        if (Window is not { } window)
            return;

        var controller = window.RootViewController;
        (controller as PlaygroundViewController)?.SuspendPlayback();
        window.RootViewController = null;
        controller?.Dispose();
        window.Dispose();
        Window = null;
    }
}
