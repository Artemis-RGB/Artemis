using System.IO;
using System.Reactive.Disposables;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using Avalonia.Media.Imaging;
using ReactiveUI;
using SkiaSharp;

namespace Artemis.UI.Screens.Debugger.Render;

public class RenderDebugViewModel : ActivatableViewModelBase
{
    private readonly ICoreService _coreService;
    private double _currentFps;

    private Bitmap? _currentFrame;
    private string? _frameTargetPath;
    private string? _renderer;
    private int _renderHeight;
    private int _renderWidth;

    public RenderDebugViewModel(ICoreService coreService)
    {
        _coreService = coreService;

        DisplayName = "Rendering";

        this.WhenActivated(disposables =>
        {
            HandleActivation();
            Disposable.Create(HandleDeactivation).DisposeWith(disposables);
        });
    }

    public Bitmap? CurrentFrame
    {
        get => _currentFrame;
        set => RaiseAndSetIfChanged(ref _currentFrame, value);
    }

    public double CurrentFps
    {
        get => _currentFps;
        set => RaiseAndSetIfChanged(ref _currentFps, value);
    }

    public int RenderWidth
    {
        get => _renderWidth;
        set => RaiseAndSetIfChanged(ref _renderWidth, value);
    }

    public int RenderHeight
    {
        get => _renderHeight;
        set => RaiseAndSetIfChanged(ref _renderHeight, value);
    }

    public string? Renderer
    {
        get => _renderer;
        set => RaiseAndSetIfChanged(ref _renderer, value);
    }

    private void HandleActivation()
    {
        Renderer = Constants.ManagedGraphicsContext != null ? Constants.ManagedGraphicsContext.GetType().Name : "Software";
        _coreService.FrameRendered += CoreServiceOnFrameRendered;
    }

    private void HandleDeactivation()
    {
        _coreService.FrameRendered -= CoreServiceOnFrameRendered;
    }

    private void CoreServiceOnFrameRendered(object? sender, FrameRenderedEventArgs e)
    {
        CurrentFps = _coreService.FrameRate;
        using SKImage skImage = e.Texture.Surface.Snapshot();
        SKImageInfo bitmapInfo = e.Texture.ImageInfo;

        if (_frameTargetPath != null)
        {
            using (SKData data = skImage.Encode(SKEncodedImageFormat.Png, 100))
            {
                using (FileStream stream = File.OpenWrite(_frameTargetPath))
                {
                    data.SaveTo(stream);
                }
            }

            _frameTargetPath = null;
        }

        RenderHeight = bitmapInfo.Height;
        RenderWidth = bitmapInfo.Width;

        // TODO: This performs well enough but look into something else
        using (SKData data = skImage.Encode(SKEncodedImageFormat.Png, 100))
        {
            CurrentFrame = new Bitmap(data.AsStream());
        }
    }
}