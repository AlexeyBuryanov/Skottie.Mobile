using CoreAnimation;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using SkiaSharp;
using SkiaSharp.Views.iOS;
using UIKit;

namespace Skottie.Mobile.iOS;

[Register("SkottieMobileAnimationView")]
public class SkottieAnimationView : SKCanvasView
{
    private SkiaSharp.Skottie.Animation? _animation;
    private CADisplayLink? _displayLink;
    private double _currentFrame;
    private bool _isPlaying;
    private bool _playbackRequested;
    private double _lastTimestamp;

    public SkottieAnimationView()
    {
        Initialize();
    }

    public SkottieAnimationView(CGRect frame)
        : base(frame)
    {
        Initialize();
    }

    protected SkottieAnimationView(NativeHandle handle)
        : base(handle)
    {
        Initialize();
    }

    public string? AnimPath { get; set; }

    public bool IsPlaying => _isPlaying;

    public event EventHandler<Exception>? LoadFailed;

    /// <summary>
    /// Initialize the animation from a file path
    /// </summary>
    /// <param name="path">Path to the Lottie JSON file</param>
    public void Init(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            Console.WriteLine("SkottieAnimation: Animation path is null or empty");
            return;
        }

        Clear();
        AnimPath = path;

        try
        {
            using var stream = File.OpenRead(path);
            using var data = SKData.Create(stream);

            InitializeFromData(data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SkottieAnimation: Error initializing animation: {ex.Message}");
            LoadFailed?.Invoke(this, ex);
        }
    }

    /// <summary>
    /// Initialize the animation from a bundle resource
    /// </summary>
    /// <param name="resourceName">Name of the resource in the bundle</param>
    public void InitFromBundle(string resourceName)
    {
        if (string.IsNullOrEmpty(resourceName))
        {
            Console.WriteLine("SkottieAnimation: Resource name is null or empty");
            return;
        }

        Clear();
        AnimPath = resourceName;

        try
        {
            var bundlePath = NSBundle.MainBundle.PathForResource(
                Path.GetFileNameWithoutExtension(resourceName),
                Path.GetExtension(resourceName)?.TrimStart('.'),
                Path.GetDirectoryName(resourceName));

            if (bundlePath == null)
            {
                throw new FileNotFoundException("Animation bundle resource not found.", resourceName);
            }

            using var stream = File.OpenRead(bundlePath);
            using var data = SKData.Create(stream);

            InitializeFromData(data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SkottieAnimation: Error initializing animation from bundle: {ex.Message}");
            LoadFailed?.Invoke(this, ex);
        }
    }

    /// <summary>
    /// Initialize the animation from NSData (for binary data from APIs or other sources)
    /// </summary>
    /// <param name="nsData">NSData containing the Lottie JSON</param>
    public void InitFromNSData(NSData? nsData)
    {
        if (nsData == null)
        {
            Console.WriteLine("SkottieAnimation: NSData is null");
            return;
        }

        try
        {
            Clear();
            var bytes = new byte[checked((int)nsData.Length)];
            System.Runtime.InteropServices.Marshal.Copy(nsData.Bytes, bytes, 0, bytes.Length);

            using var data = SKData.CreateCopy(bytes);
            InitializeFromData(data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SkottieAnimation: Error initializing animation from NSData: {ex.Message}");
            LoadFailed?.Invoke(this, ex);
        }
    }

    public void Start()
    {
        _playbackRequested = true;
        if (_animation == null || Window == null)
        {
            return;
        }

        if (_isPlaying)
        {
            return;
        }

        _isPlaying = true;
        _lastTimestamp = 0;
        _displayLink = CADisplayLink.Create(OnDisplayLink);
        _displayLink.Paused = false;
        _displayLink.AddToRunLoop(NSRunLoop.Main, NSRunLoopMode.Common);
    }

    public void Stop()
    {
        _playbackRequested = false;
        PausePlayback();
    }

    private void PausePlayback()
    {
        _isPlaying = false;
        _lastTimestamp = 0;
        if (_displayLink == null)
        {
            return;
        }

        _displayLink.Paused = true;
        _displayLink.Invalidate();
        _displayLink.Dispose();
        _displayLink = null;
    }

    public void Clear()
    {
        Cleanup();
        _currentFrame = 0;
        AnimPath = null;
        SetNeedsDisplay();
    }

    public override void MovedToWindow()
    {
        base.MovedToWindow();
        if (Window == null)
        {
            PausePlayback();
        }
        else if (_playbackRequested)
        {
            Start();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            PaintSurface -= OnPaintSurface;
            Cleanup();
        }

        base.Dispose(disposing);
    }

    private void Initialize()
    {
        BackgroundColor = UIColor.Clear;
        Opaque = false;
    }

    private void InitializeFromData(SKData? data)
    {
        if (data == null)
        {
            throw new InvalidDataException("Failed to read animation data.");
        }

        Cleanup();

        if (!SkiaSharp.Skottie.Animation.TryCreate(data, out var createdAnimation))
        {
            throw new InvalidDataException("The data is not a supported Lottie animation.");
        }

        _animation = createdAnimation;
        _currentFrame = 0;

        PaintSurface -= OnPaintSurface;
        PaintSurface += OnPaintSurface;
        SetNeedsDisplay();
    }

    private void OnDisplayLink()
    {
        var animation = _animation;
        if (animation == null || _displayLink == null)
        {
            return;
        }

        // Measure actual callback time, including dropped frames, and reset on resume.
        var timestamp = _displayLink.Timestamp;
        var elapsed = _lastTimestamp > 0 ? Math.Max(0, timestamp - _lastTimestamp) : 0;
        _lastTimestamp = timestamp;

        _currentFrame += elapsed;

        var duration = animation.Duration.TotalSeconds;
        if (duration > 0 && _currentFrame >= duration)
        {
            _currentFrame %= duration;
        }

        SetNeedsDisplay();
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        var animation = _animation;
        if (animation == null)
        {
            return;
        }

        animation.SeekFrameTime(_currentFrame);
        animation.Render(canvas, new SKRect(0, 0, e.Info.Width, e.Info.Height));
    }

    private void Cleanup()
    {
        Stop();

        _animation?.Dispose();
        _animation = null;
    }
}
