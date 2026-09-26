using Foundation;
using Skottie.Mobile.iOS;
using UIKit;

namespace Skottie.Mobile.Playground.iOS;

public class PlaygroundViewController : UIViewController
{
    private const string AssetName = "animations/orbit.json";
    private SkottieAnimationView _animation = null!;
    private UILabel _status = null!;
    private bool _playRequested = true;

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();
        View!.BackgroundColor = UIColor.SystemBackground;

        _animation = new SkottieAnimationView();
        _animation.LoadFailed += OnLoadFailed;
        _animation.HeightAnchor.ConstraintEqualTo(240).Active = true;
        _status = new UILabel { Lines = 0, Font = UIFont.SystemFontOfSize(16)! };

        var stack = new UIStackView
        {
            Axis = UILayoutConstraintAxis.Vertical,
            Spacing = 16,
            TranslatesAutoresizingMaskIntoConstraints = false,
        };
        stack.AddArrangedSubview(new UILabel { Text = "Skottie / iOS", Font = UIFont.BoldSystemFontOfSize(28)! });
        stack.AddArrangedSubview(new UILabel { Text = "Native .NET 10 · SkiaSharp rendering", Lines = 0 });
        stack.AddArrangedSubview(_animation);
        stack.AddArrangedSubview(_status);
        AddButtons(stack, ("Play", Play), ("Pause", Pause));
        AddButtons(stack, ("Reload bundle", LoadBundle), ("Load file", LoadFile));
        AddButtons(stack, ("Missing asset", LoadMissingAsset), ("Clear", Clear));
        stack.AddArrangedSubview(new UILabel
        {
            Text = "Try pause/resume, reload, file loading, and error recovery. Background the app to check lifecycle handling.",
            Lines = 0,
            Font = UIFont.SystemFontOfSize(14)!,
        });

        var scroll = new UIScrollView { TranslatesAutoresizingMaskIntoConstraints = false };
        View.AddSubview(scroll);
        scroll.AddSubview(stack);
        NSLayoutConstraint.ActivateConstraints([
            scroll.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
            scroll.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
            scroll.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor),
            scroll.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor),
            stack.TopAnchor.ConstraintEqualTo(scroll.ContentLayoutGuide.TopAnchor, 24),
            stack.BottomAnchor.ConstraintEqualTo(scroll.ContentLayoutGuide.BottomAnchor, -24),
            stack.LeadingAnchor.ConstraintEqualTo(scroll.ContentLayoutGuide.LeadingAnchor, 24),
            stack.TrailingAnchor.ConstraintEqualTo(scroll.ContentLayoutGuide.TrailingAnchor, -24),
            stack.WidthAnchor.ConstraintEqualTo(scroll.FrameLayoutGuide.WidthAnchor, -48),
        ]);
        LoadBundle();
    }

    private static void AddButtons(UIStackView stack, params (string Text, Action Action)[] actions)
    {
        var row = new UIStackView { Axis = UILayoutConstraintAxis.Horizontal, Distribution = UIStackViewDistribution.FillEqually, Spacing = 12 };
        foreach (var (text, action) in actions)
        {
            var button = new UIButton(UIButtonType.System);
            button.SetTitle(text, UIControlState.Normal);
            button.HeightAnchor.ConstraintGreaterThanOrEqualTo(44).Active = true;
            button.TouchUpInside += (_, _) => action();
            row.AddArrangedSubview(button);
        }
        stack.AddArrangedSubview(row);
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

    private void LoadBundle()
    {
        _playRequested = true;
        _status.Text = "Bundled animation · looping";
        _animation.InitFromBundle(AssetName);
        if (_playRequested)
            _animation.Start();
    }

    private void LoadFile()
    {
        try
        {
            var bundledPath = NSBundle.MainBundle.PathForResource("orbit", "json", "animations")
                ?? throw new FileNotFoundException("The bundled sample animation is missing.");
            var path = Path.Combine(Path.GetTempPath(), "orbit.json");
            File.Copy(bundledPath, path, overwrite: true);
            _playRequested = true;
            _status.Text = "Local file · looping";
            _animation.Init(path);
            if (_playRequested)
                _animation.Start();
        }
        catch (Exception exception)
        {
            OnLoadFailed(this, exception);
        }
    }

    private void LoadMissingAsset() => _animation.InitFromBundle("missing.json");

    private void Clear()
    {
        _playRequested = false;
        _animation.Clear();
        _status.Text = "Cleared · Reload bundle loads a fresh animation.";
    }

    private void OnLoadFailed(object? sender, Exception exception)
    {
        _playRequested = false;
        _animation.Stop();
        _status.Text = $"Load failed: {exception.Message}";
    }

    public void SuspendPlayback() => _animation?.Stop();

    public void ResumePlayback()
    {
        if (_playRequested)
            _animation?.Start();
    }

    public override void ViewWillDisappear(bool animated)
    {
        SuspendPlayback();
        base.ViewWillDisappear(animated);
    }

    public override void ViewDidAppear(bool animated)
    {
        base.ViewDidAppear(animated);
        ResumePlayback();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && _animation != null)
        {
            _animation.LoadFailed -= OnLoadFailed;
            _animation.RemoveFromSuperview();
            _animation.Dispose();
        }
        base.Dispose(disposing);
    }
}
