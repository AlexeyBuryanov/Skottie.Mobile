# Skottie.Mobile

Lottie JSON animation controls for **.NET 10 Android, .NET 10 iOS, and .NET MAUI 10**,
rendered with [SkiaSharp Skottie](https://github.com/mono/SkiaSharp).

**Skottie.Mobile replaces
[Lottie.DotNet8.Android](https://github.com/AlexeyBuryanov/Lottie.DotNet8.Android)
and [Lottie.DotNet8.Ios](https://github.com/AlexeyBuryanov/Lottie.DotNet8.Ios),
which are no longer supported.** This is a replacement control with its own API,
not a binary-compatible update to those bindings. See the migration notes below.

## Projects

| Project | Target frameworks | Control |
| --- | --- | --- |
| `src/Skottie.Mobile.Android` | `net10.0-android` | `Skottie.Mobile.Android.SkottieAnimationView` |
| `src/Skottie.Mobile.iOS` | `net10.0-ios` | `Skottie.Mobile.iOS.SkottieAnimationView` |
| `src/Skottie.Mobile.Maui` | `net10.0-android;net10.0-ios` | `Skottie.Mobile.Maui.SkottieAnimationView` |

The native controls inherit from `SKCanvasView`. Android uses `Choreographer` and
iOS uses `CADisplayLink` for display-synchronized playback. The MAUI handler creates
these same native controls, so native and MAUI applications share rendering code.
Windows and Mac Catalyst are not currently targeted.

## Requirements and building

- .NET 10 SDK and the Android/iOS workloads; the MAUI workload for MAUI consumers.
- Android API 21 or later; iOS 15 or later.
- A compatible Android SDK/JDK to build Android applications.
- A Mac with an Xcode version compatible with your installed .NET iOS workload to
  build and run iOS applications (or a paired Mac when using Visual Studio on Windows).

Dependencies are centrally managed in `Directory.Packages.props`: SkiaSharp 4.152.1 and
Microsoft.Maui.Controls 10.0.110.

From the repository root, with the corresponding workloads installed:

```sh
dotnet restore Skottie.Mobile.slnx
dotnet build Skottie.Mobile.slnx
```

To build only the native Android library:

```sh
dotnet build src/Skottie.Mobile.Android/Skottie.Mobile.Android.csproj
```

To build only the Android target of the MAUI library on a machine without the iOS workload:

```sh
dotnet build src/Skottie.Mobile.Maui/Skottie.Mobile.Maui.csproj -p:TargetFrameworks=net10.0-android
```

## .NET MAUI

### 1. Reference the library

Add a project reference to `src/Skottie.Mobile.Maui/Skottie.Mobile.Maui.csproj` from
your .NET 10 MAUI app. Adjust the relative path to your checkout:

```xml
<ItemGroup>
  <ProjectReference Include="../Skottie.Mobile/src/Skottie.Mobile.Maui/Skottie.Mobile.Maui.csproj" />
</ItemGroup>
```

The native project reference is selected automatically for the app's target platform.
Your app should target `net10.0-android` and/or `net10.0-ios`. If your app also targets
other platforms, condition this reference and the control's usage to Android/iOS.

### 2. Register the handler

In `MauiProgram.cs`:

```csharp
using Skottie.Mobile.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>()
               .UseSkottie();
        return builder.Build();
    }
}
```

`UseSkottie()` is the only registration needed for this control.

### 3. Add an animation

Place `loading.json` in your app's `Resources/Raw` directory with build action
**MauiAsset**. The standard MAUI template includes the following item; add it only
if it is missing:

```xml
<MauiAsset Include="Resources\Raw\**" LogicalName="%(RecursiveDir)%(Filename)%(Extension)" />
```

Use the asset's logical name as `Source`, without the `Resources/Raw/` prefix.
For example, `Resources/Raw/animations/loading.json` becomes
`animations/loading.json`. Resource names are case-sensitive.

### 4. Use the control in XAML

```xml
<ContentPage
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:skottie="clr-namespace:Skottie.Mobile.Maui;assembly=Skottie.Mobile.Maui"
    x:Class="MyApp.MainPage">
    <skottie:SkottieAnimationView
        x:Name="LoadingAnimation"
        Source="loading.json"
        IsPlaying="True"
        WidthRequest="160"
        HeightRequest="160" />
</ContentPage>
```

Both `Source` and `IsPlaying` are bindable properties. For example,
`IsPlaying="{Binding IsBusy}"` controls playback from a view model.

```csharp
LoadingAnimation.Stop();  // Pause at the current position.
LoadingAnimation.Start(); // Resume from that position.
LoadingAnimation.Source = null; // Stop, release the animation, and clear the view.
```

To load an existing file instead of a packaged asset, set `Source` to its absolute path:

```csharp
LoadingAnimation.Source = Path.Combine(FileSystem.AppDataDirectory, "downloaded.json");
```

The control does not download URLs. Download the JSON yourself before setting the
file path. Loading is synchronous; change properties and call methods on the UI thread.

Subscribe before setting `Source` to observe initial loading errors:

```csharp
var animation = new SkottieAnimationView { WidthRequest = 160, HeightRequest = 160 };
animation.LoadFailed += (_, exception) => System.Diagnostics.Debug.WriteLine(exception);
animation.Source = "loading.json";
```

| MAUI member | Behavior |
| --- | --- |
| `Source` | Packaged asset name or absolute JSON file path. Changing it loads from the beginning. Null/empty clears the view. |
| `IsPlaying` | Requested playback state; defaults to `true`. An animation must be loaded and the native view attached to play. |
| `Start()` / `Stop()` | Set `IsPlaying` to `true` / `false`. Stop pauses rather than rewinding. |
| `LoadFailed` | Reports file/resource loading and parsing errors on the UI thread. |

Playback loops indefinitely. Detached native views suspend frame callbacks and resume
on attachment when playback is still requested. The handler releases the loaded
animation when disconnected. For navigation that keeps a page attached but hidden,
pause in `OnDisappearing` and resume in `OnAppearing` as appropriate for your app.

## Native .NET Android

Reference `src/Skottie.Mobile.Android/Skottie.Mobile.Android.csproj` from a
`net10.0-android` app. Put `loading.json` in `Assets` with build action **AndroidAsset**.

```csharp
using Skottie.Mobile.Android;

// In an Activity; keep a field reference for lifecycle management.
var animation = new SkottieAnimationView(this);
animation.LoadFailed += (_, exception) => System.Diagnostics.Debug.WriteLine(exception);
SetContentView(animation);
animation.InitFromAsset("loading.json"); // Loads and requests looping playback.

// Alternatively: animation.Init(absoluteJsonFilePath);
animation.Stop();  // Pause.
animation.Start(); // Resume.
```

For Android XML layouts, the registered view name is:

```xml
<skottie.mobile.SkottieAnimationView
    xmlns:android="http://schemas.android.com/apk/res/android"
    android:id="@+id/animation"
    android:layout_width="160dp"
    android:layout_height="160dp" />
```

Retrieve it using `FindViewById<SkottieAnimationView>(Resource.Id.animation)`, then
call `InitFromAsset` or `Init`. Pause in `OnPause` and resume in `OnResume` when
appropriate. Call `Clear()` to release the animation without disposing the view.
Dispose the view when its owner is permanently destroyed.

## Native .NET iOS

Reference `src/Skottie.Mobile.iOS/Skottie.Mobile.iOS.csproj` from a `net10.0-ios` app.
Add `loading.json` to the application bundle with build action **BundleResource**.

```csharp
using CoreGraphics;
using Skottie.Mobile.iOS;

// In a UIViewController; keep a field reference for lifecycle management.
var animation = new SkottieAnimationView(new CGRect(0, 0, 160, 160));
animation.LoadFailed += (_, exception) => System.Diagnostics.Debug.WriteLine(exception);
View!.AddSubview(animation);
animation.InitFromBundle("loading.json");
animation.Start(); // iOS native loading does not automatically start playback.

// Alternatively:
// animation.Init(absoluteJsonFilePath);
// animation.InitFromNSData(jsonData);
// animation.Start();
```

Bundle names may include subdirectories, for example `animations/loading.json`.
`Stop()` pauses and `Start()` resumes. Pause in `ViewWillDisappear` and resume in
`ViewWillAppear` when appropriate. `Clear()` releases the animation and clears the
view. Remove and dispose the view when its owner is permanently destroyed.

On both native controls, `IsPlaying` reports active playback. `AnimPath` retains
the most recently requested path; assigning it alone does not load an animation.
Use the loading methods instead. Call all native APIs on the UI thread.

## Migrating from the older Lottie bindings

1. Remove the `Lottie.DotNet8.Android` or `Lottie.DotNet8.Ios` reference and add the
   appropriate Skottie.Mobile project reference.
2. Replace the old native view and namespace with `SkottieAnimationView` (including
   the registered class name in Android XML or iOS storyboard integrations).
3. Load your JSON using the native methods above, or `Source` in MAUI, and replace
   playback calls with `Start()` and `Stop()`.
4. Check your animations on each target device. Skottie is a different renderer;
   feature support and rendering can differ from the original Lottie libraries.

The current API supports looping JSON animations and pause/resume. It does not
expose repeat counts, speed, seeking, completion callbacks, `.lottie` archives,
or external image/font resource providers. Prefer self-contained vector animations;
animations requiring external assets need additional resource-provider integration.

## License

[MIT](LICENSE).
