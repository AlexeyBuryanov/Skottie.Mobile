using global::Android.App;
using global::Android.OS;
using global::Android.Views;
using global::Android.Widget;
using Skottie.Mobile.Android;

namespace Skottie.Mobile.Playground.Android;

[Activity(Label = "Skottie Android", MainLauncher = true, Theme = "@android:style/Theme.Material.Light.NoActionBar")]
public class MainActivity : Activity
{
    private const string AssetName = "animations/orbit.json";
    private SkottieAnimationView _animation = null!;
    private TextView _status = null!;
    private bool _playRequested = true;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };
        layout.SetPadding(Dp(24), Dp(24), Dp(24), Dp(24));
        layout.SetFitsSystemWindows(true);
        layout.AddView(new TextView(this) { Text = "Skottie / Android", TextSize = 28 });
        layout.AddView(new TextView(this) { Text = "Native .NET 10 · SkiaSharp rendering", TextSize = 16 });

        _animation = new SkottieAnimationView(this);
        _animation.LoadFailed += OnLoadFailed;
        layout.AddView(_animation, new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, Dp(240)));
        _status = new TextView(this) { TextSize = 16 };
        _status.SetPadding(0, Dp(12), 0, Dp(12));
        layout.AddView(_status);

        AddButtons(layout, ("Play", Play), ("Pause", Pause));
        AddButtons(layout, ("Reload asset", LoadAsset), ("Load file", LoadFile));
        AddButtons(layout, ("Missing asset", LoadMissingAsset), ("Clear", Clear));
        layout.AddView(new TextView(this)
        {
            Text = "Try pause/resume, reload, file loading, and error recovery. Background the app to check lifecycle handling.",
            TextSize = 14,
        });

        var scroll = new ScrollView(this);
        scroll.AddView(layout);
        SetContentView(scroll);
        LoadAsset();
    }

    private void AddButtons(LinearLayout layout, params (string Text, Action Action)[] actions)
    {
        var row = new LinearLayout(this) { Orientation = Orientation.Horizontal };
        foreach (var (text, action) in actions)
        {
            var button = new Button(this) { Text = text };
            button.Click += (_, _) => action();
            row.AddView(button, new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1));
        }
        layout.AddView(row);
    }

    private void Play()
    {
        _playRequested = true;
        _animation.Start();
        _status.Text = "Playback requested (reload if the view is empty).";
    }

    private void Pause()
    {
        _playRequested = false;
        _animation.Stop();
        _status.Text = "Paused · Play resumes from this position.";
    }

    private void LoadAsset()
    {
        _playRequested = true;
        _status.Text = "Bundled animation · looping";
        _animation.InitFromAsset(AssetName);
    }

    private void LoadFile()
    {
        try
        {
            var path = Path.Combine(CacheDir!.AbsolutePath, "orbit.json");
            using (var input = Assets!.Open(AssetName))
            using (var output = File.Create(path))
            {
                input.CopyTo(output);
            }
            _playRequested = true;
            _status.Text = "Local file · looping";
            _animation.Init(path);
        }
        catch (Exception exception)
        {
            OnLoadFailed(this, exception);
        }
    }

    private void LoadMissingAsset()
    {
        _animation.InitFromAsset("missing.json");
    }

    private void Clear()
    {
        _playRequested = false;
        _animation.Clear();
        _status.Text = "Cleared · Reload asset loads a fresh animation.";
    }

    private void OnLoadFailed(object? sender, Exception exception)
    {
        _playRequested = false;
        _animation.Stop();
        _status.Text = $"Load failed: {exception.Message}";
    }

    protected override void OnPause()
    {
        _animation.Stop();
        base.OnPause();
    }

    protected override void OnResume()
    {
        base.OnResume();
        if (_playRequested)
            _animation.Start();
    }

    protected override void OnDestroy()
    {
        _animation.LoadFailed -= OnLoadFailed;
        _animation.Clear();
        base.OnDestroy();
        _animation.Dispose();
    }

    private int Dp(int value) => (int)(value * Resources!.DisplayMetrics!.Density);
}
