using System.IO;
using System.Reactive.Disposables;
using System.Timers;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using ReactiveUI;
using SkiaSharp;

namespace Artemis.UI.Screens.Debugger.Tabs.Render
{
    public class RenderDebugViewModel : ActivatableViewModelBase, IRoutableViewModel
    {
        private readonly ICoreService _coreService;
        private readonly Timer _fpsTimer;
        private double _currentFps;

        private SKImage? _currentFrame;
        private int _frames;
        private string? _frameTargetPath;
        private string _renderer;
        private int _renderHeight;
        private int _renderWidth;

        public RenderDebugViewModel(DebugViewModel hostScreen, ICoreService coreService)
        {
            HostScreen = hostScreen;

            _coreService = coreService;
            _fpsTimer = new Timer(1000);
            _fpsTimer.Start();

            this.WhenActivated(disposables =>
            {
                HandleActivation();
                Disposable.Create(HandleDeactivation).DisposeWith(disposables);
            });
        }

        public SKImage? CurrentFrame
        {
            get => _currentFrame;
            set => this.RaiseAndSetIfChanged(ref _currentFrame, value);
        }

        public double CurrentFps
        {
            get => _currentFps;
            set => this.RaiseAndSetIfChanged(ref _currentFps, value);
        }

        public int RenderWidth
        {
            get => _renderWidth;
            set => this.RaiseAndSetIfChanged(ref _renderWidth, value);
        }

        public int RenderHeight
        {
            get => _renderHeight;
            set => this.RaiseAndSetIfChanged(ref _renderHeight, value);
        }

        public string Renderer
        {
            get => _renderer;
            set => this.RaiseAndSetIfChanged(ref _renderer, value);
        }

        private void HandleActivation()
        {
            _coreService.FrameRendered += CoreServiceOnFrameRendered;
            _fpsTimer.Elapsed += FpsTimerOnElapsed;
        }

        private void HandleDeactivation()
        {
            _coreService.FrameRendered -= CoreServiceOnFrameRendered;
            _fpsTimer.Elapsed -= FpsTimerOnElapsed;
            _fpsTimer.Dispose();
        }

        private void CoreServiceOnFrameRendered(object? sender, FrameRenderedEventArgs e)
        {
            _frames++;

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

            CurrentFrame = e.Texture.Surface.Snapshot();
        }

        private void FpsTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            CurrentFps = _frames;
            Renderer = Constants.ManagedGraphicsContext != null ? Constants.ManagedGraphicsContext.GetType().Name : "Software";
            _frames = 0;
        }

        public string UrlPathSegment => "render";
        public IScreen HostScreen { get; }
    }
}