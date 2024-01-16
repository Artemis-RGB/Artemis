using System.IO;
using System.Reactive.Disposables;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using Avalonia.Media.Imaging;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using SkiaSharp;

namespace Artemis.UI.Screens.Debugger.Render;

public partial class RenderDebugViewModel : ActivatableViewModelBase
{
    private readonly IRenderService _renderService;
    private string? _frameTargetPath;
    [Notify] private double _currentFps;
    [Notify] private Bitmap? _currentFrame;
    [Notify] private string? _renderer;
    [Notify] private int _renderHeight;
    [Notify] private int _renderWidth;

    public RenderDebugViewModel(IRenderService renderService)
    {
        _renderService = renderService;

        DisplayName = "Rendering";

        this.WhenActivated(disposables =>
        {
            HandleActivation();
            Disposable.Create(HandleDeactivation).DisposeWith(disposables);
        });
    }

    private void HandleActivation()
    {
        Renderer = Constants.ManagedGraphicsContext != null ? Constants.ManagedGraphicsContext.GetType().Name : "Software";
        _renderService.FrameRendered += RenderServiceOnFrameRendered;
    }

    private void HandleDeactivation()
    {
        _renderService.FrameRendered -= RenderServiceOnFrameRendered;
    }

    private void RenderServiceOnFrameRendered(object? sender, FrameRenderedEventArgs e)
    {
        CurrentFps = _renderService.FrameRate;
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