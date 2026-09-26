using Microsoft.Maui.Controls;

namespace Skottie.Mobile.Maui;

/// <summary>A Lottie view rendered by the platform's native Skottie control.</summary>
public class SkottieAnimationView : View
{
    public static readonly BindableProperty SourceProperty = BindableProperty.Create(
        nameof(Source), typeof(string), typeof(SkottieAnimationView));

    public static readonly BindableProperty IsPlayingProperty = BindableProperty.Create(
        nameof(IsPlaying), typeof(bool), typeof(SkottieAnimationView), true);

    /// <summary>A packaged resource name or an absolute path to a Lottie JSON file.</summary>
    public string? Source
    {
        get => (string?)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// <summary>Whether playback is requested. Playback loops and pauses while detached.</summary>
    public bool IsPlaying
    {
        get => (bool)GetValue(IsPlayingProperty);
        set => SetValue(IsPlayingProperty, value);
    }

    /// <summary>Raised on the UI thread if the native control cannot load the source.</summary>
    public event EventHandler<Exception>? LoadFailed;

    public void Start() => IsPlaying = true;

    /// <summary>Pauses at the current position. Start resumes playback.</summary>
    public void Stop() => IsPlaying = false;

    internal void OnLoadFailed(Exception exception) => LoadFailed?.Invoke(this, exception);
}
