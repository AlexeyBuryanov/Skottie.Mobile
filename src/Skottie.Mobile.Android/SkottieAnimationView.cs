using global::Android.Content;
using global::Android.Runtime;
using global::Android.Util;
using global::Android.Views;
using SkiaSharp;
using SkiaSharp.Views.Android;

namespace Skottie.Mobile.Android;

[Register("skottie.mobile.SkottieAnimationView")]
public class SkottieAnimationView : SKCanvasView, Choreographer.IFrameCallback
{
    private const double NanosecondsPerSecond = 1_000_000_000.0;

    private SkiaSharp.Skottie.Animation? _animation;
    private Choreographer? _choreographer;
    private double _currentFrame;
    private long _lastFrameTimeNanos;
    private bool _isPlaying;
    private bool _playbackRequested;

    public SkottieAnimationView(Context context)
        : base(context)
    {
    }

    public SkottieAnimationView(Context context, IAttributeSet attrs)
        : base(context, attrs)
    {
    }

    public SkottieAnimationView(Context context, IAttributeSet attrs, int defStyleAttr)
        : base(context, attrs, defStyleAttr)
    {
    }

    protected SkottieAnimationView(IntPtr javaReference, JniHandleOwnership transfer)
        : base(javaReference, transfer)
    {
    }

    public string? AnimPath { get; set; }

    public bool IsPlaying => _isPlaying;

    public event EventHandler<Exception>? LoadFailed;

    public void Init(string path)
    {
        Init(path, false);
    }

    public void InitFromAsset(string assetPath)
    {
        Init(assetPath, true);
    }

    public void Start()
    {
        _playbackRequested = true;
        if (!IsAttachedToWindow)
        {
            return;
        }

        if (_animation == null || _isPlaying)
        {
            return;
        }

        _lastFrameTimeNanos = 0;
        _isPlaying = true;

        var choreographer = _choreographer ?? Choreographer.Instance;
        if (choreographer == null)
        {
            _isPlaying = false;
            return;
        }

        _choreographer = choreographer;
        choreographer.PostFrameCallback(this);
    }

    public void Stop()
    {
        _playbackRequested = false;
        PausePlayback();
    }

    private void PausePlayback()
    {
        if (!_isPlaying)
        {
            return;
        }

        _isPlaying = false;
        _lastFrameTimeNanos = 0;
        _choreographer?.RemoveFrameCallback(this);
    }

    public void Clear()
    {
        Stop();
        _animation?.Dispose();
        _animation = null;
        _currentFrame = 0;
        AnimPath = null;
        Invalidate();
    }

    protected override void OnAttachedToWindow()
    {
        base.OnAttachedToWindow();
        if (_playbackRequested)
        {
            Start();
        }
    }

    protected override void OnDetachedFromWindow()
    {
        PausePlayback();
        base.OnDetachedFromWindow();
    }

    public void DoFrame(long frameTimeNanos)
    {
        var animation = _animation;
        if (!_isPlaying || animation == null)
        {
            return;
        }

        var elapsed = _lastFrameTimeNanos > 0 ? (frameTimeNanos - _lastFrameTimeNanos) / NanosecondsPerSecond : 0;
        _lastFrameTimeNanos = frameTimeNanos;

        if (elapsed > 0)
        {
            _currentFrame += elapsed;

            var duration = animation.Duration.TotalSeconds;
            if (duration > 0 && _currentFrame >= duration)
            {
                _currentFrame %= duration;
            }
        }

        PostInvalidateOnAnimation();
        _choreographer?.PostFrameCallback(this);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            PaintSurface -= OnPaintSurface;
            Stop();
            _choreographer = null;
            _animation?.Dispose();
            _animation = null;
        }

        base.Dispose(disposing);
    }

    private void Init(string path, bool isAsset)
    {
        if (string.IsNullOrEmpty(path))
        {
            System.Diagnostics.Debug.WriteLine("SkottieAnimation: Animation path is null or empty");

            return;
        }

        try
        {
            Clear();
            AnimPath = path;

            // Load data from either file or asset
            using var data = CreateData(path, isAsset);

            if (data == null)
            {
                throw new InvalidDataException("Failed to read animation data.");
            }

            if (!SkiaSharp.Skottie.Animation.TryCreate(data, out var createdAnimation))
            {
                throw new InvalidDataException("The data is not a supported Lottie animation.");
            }

            _animation = createdAnimation;
            _currentFrame = 0;

            PaintSurface -= OnPaintSurface;
            PaintSurface += OnPaintSurface;
            Invalidate();
            Start();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SkottieAnimation: Error initializing animation: {ex.Message}");
            LoadFailed?.Invoke(this, ex);
        }
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        if (_animation != null)
        {
            _animation.SeekFrameTime(_currentFrame);
            _animation.Render(canvas, new SKRect(0, 0, e.Info.Width, e.Info.Height));
        }
    }

    private SKData? CreateData(string path, bool isAsset)
    {
        if (isAsset)
        {
            return CreateDataFromAsset(path);
        }

        using var stream = File.OpenRead(path);

        return SKData.Create(stream);
    }

    private SKData? CreateDataFromAsset(string path)
    {
        var assets = Context?.Assets;
        if (assets == null)
        {
            return null;
        }

        using var assetStream = assets.Open(path);
        using var memoryStream = new MemoryStream();
        assetStream.CopyTo(memoryStream);

        return SKData.CreateCopy(memoryStream.ToArray());
    }
}
