using System;
using System.Diagnostics;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Artemis.Core;
using Artemis.Core.Services;
using Ookii.Dialogs.Wpf;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using Stylet;

namespace Artemis.UI.Screens.Settings.Debug.Tabs
{
    public class RenderDebugViewModel : Screen
    {
        private readonly ICoreService _coreService;
        private readonly Timer _fpsTimer;
        private double _currentFps;
        private WriteableBitmap _currentFrame;
        private int _renderWidth;
        private int _renderHeight;
        private string _frameTargetPath;
        private string _renderer;
        private int _frames;

        public RenderDebugViewModel(ICoreService coreService)
        {
            DisplayName = "RENDERING";
            _coreService = coreService;
            _fpsTimer = new Timer(1000);
            _fpsTimer.Start();
        }

        public WriteableBitmap CurrentFrame
        {
            get => _currentFrame;
            set => SetAndNotify(ref _currentFrame, value);
        }

        public double CurrentFps
        {
            get => _currentFps;
            set => SetAndNotify(ref _currentFps, value);
        }

        public int RenderWidth
        {
            get => _renderWidth;
            set => SetAndNotify(ref _renderWidth, value);
        }

        public int RenderHeight
        {
            get => _renderHeight;
            set => SetAndNotify(ref _renderHeight, value);
        }

        public string Renderer
        {
            get => _renderer;
            set => SetAndNotify(ref _renderer, value);
        }

        public void SaveFrame()
        {
            VistaSaveFileDialog dialog = new VistaSaveFileDialog {Filter = "Portable network graphic (*.png)|*.png", Title = "Save render frame"};
            dialog.FileName = $"Artemis frame {DateTime.Now:yyyy-dd-M--HH-mm-ss}.png";
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                if (dialog.FileName.EndsWith(".png"))
                    _frameTargetPath = dialog.FileName;
                else
                    _frameTargetPath = dialog.FileName + ".png";
            }
        }

        protected override void OnActivate()
        {
            _coreService.FrameRendered += CoreServiceOnFrameRendered;
            _fpsTimer.Elapsed += FpsTimerOnElapsed;
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            _coreService.FrameRendered -= CoreServiceOnFrameRendered;
            _fpsTimer.Elapsed -= FpsTimerOnElapsed;
            base.OnDeactivate();
        }

        protected override void OnClose()
        {
            _fpsTimer.Dispose();
            base.OnClose();
        }


        private void CoreServiceOnFrameRendered(object sender, FrameRenderedEventArgs e)
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

            Execute.OnUIThreadSync(() =>
            {
                // ReSharper disable twice CompareOfFloatsByEqualityOperator
                if (CurrentFrame == null || CurrentFrame.Width != bitmapInfo.Width || CurrentFrame.Height != bitmapInfo.Height)
                {
                    CurrentFrame = e.Texture.Surface.Snapshot().ToWriteableBitmap();
                    return;
                }

                CurrentFrame.Lock();
                using (SKPixmap pixmap = new(bitmapInfo, CurrentFrame.BackBuffer, CurrentFrame.BackBufferStride))
                {
                    // ReSharper disable once AccessToDisposedClosure - Looks fine
                    skImage.ReadPixels(pixmap, 0, 0);
                }

                CurrentFrame.AddDirtyRect(new Int32Rect(0, 0, CurrentFrame.PixelWidth, CurrentFrame.PixelHeight));
                CurrentFrame.Unlock();
            });
        }

        private void FpsTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            CurrentFps = _frames;
            // Renderer = Constants.ManagedGraphicsContext != null ? Constants.ManagedGraphicsContext.GetType().Name : "Software";
            Renderer = $"HighAccuracyTimers: {Stopwatch.IsHighResolution}";
            _frames = 0;
        }
    }
}