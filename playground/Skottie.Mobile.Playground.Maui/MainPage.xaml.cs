using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace Skottie.Mobile.Playground.Maui;

public partial class MainPage : ContentPage
{
    private const string AssetName = "animations/orbit.json";
    private string? _animationSource;
    private bool _playbackRequested = true;
    private string _status = "Ready";
    private bool _initialized;
    private bool _suspended;
    private bool _resumePlayback;
    private bool _released;

    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    public string? AnimationSource
    {
        get => _animationSource;
        private set
        {
            _animationSource = value;
            OnPropertyChanged();
        }
    }

    public bool PlaybackRequested
    {
        get => _playbackRequested;
        private set
        {
            _playbackRequested = value;
            OnPropertyChanged();
        }
    }

    public string Status
    {
        get => _status;
        private set
        {
            _status = value;
            OnPropertyChanged();
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (!_initialized)
        {
            _initialized = true;
            LoadSource(AssetName, "Bundled animation · looping");
        }
        ResumePlayback();
    }

    protected override void OnDisappearing()
    {
        SuspendPlayback();
        base.OnDisappearing();
    }

    private void LoadSource(string source, string status)
    {
        // Clearing first also lets Reload asset restart an unchanged source.
        AnimationSource = null;
        Status = status;
        _resumePlayback = true;
        PlaybackRequested = !_suspended;
        AnimationSource = source;
    }

    private void OnPlay(object? sender, EventArgs e)
    {
        PlaybackRequested = true;
        Status = "Playback requested (reload if the view is empty).";
    }

    private void OnPause(object? sender, EventArgs e)
    {
        PlaybackRequested = false;
        Status = "Paused · Play resumes from this position.";
    }

    private void OnReloadAsset(object? sender, EventArgs e) =>
        LoadSource(AssetName, "Bundled animation · looping");

    private async void OnLoadFile(object? sender, EventArgs e)
    {
        ActionButtons.IsEnabled = false;
        try
        {
            var path = Path.Combine(FileSystem.CacheDirectory, "orbit.json");
            using (var input = await FileSystem.OpenAppPackageFileAsync(AssetName))
            using (var output = File.Create(path))
            {
                await input.CopyToAsync(output);
            }
            if (!_released)
                LoadSource(path, "Local file · looping");
        }
        catch (Exception exception)
        {
            if (!_released)
                OnLoadFailed(this, exception);
        }
        finally
        {
            ActionButtons.IsEnabled = true;
        }
    }

    private void OnMissingAsset(object? sender, EventArgs e) =>
        LoadSource("missing.json", "Loading a deliberately missing asset…");

    private void OnClear(object? sender, EventArgs e)
    {
        PlaybackRequested = false;
        AnimationSource = null;
        Status = "Cleared · Reload asset loads a fresh animation.";
    }

    private void OnLoadFailed(object? sender, Exception exception)
    {
        _resumePlayback = false;
        PlaybackRequested = false;
        Status = $"Load failed: {exception.Message}";
    }

    internal void SuspendPlayback()
    {
        if (_suspended)
            return;
        _suspended = true;
        _resumePlayback = PlaybackRequested;
        PlaybackRequested = false;
    }

    internal void ResumePlayback()
    {
        if (!_suspended)
            return;
        _suspended = false;
        PlaybackRequested = _resumePlayback;
    }

    internal void ReleaseAnimation()
    {
        _released = true;
        PlaybackRequested = false;
        AnimationSource = null;
        Animation.Handler?.DisconnectHandler();
    }
}
