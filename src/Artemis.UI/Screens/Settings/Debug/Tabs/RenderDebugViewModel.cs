using System;
using System.IO;
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
        private double _currentFps;
        private ImageSource _currentFrame;
        private int _renderWidth;
        private int _renderHeight;
        private string _frameTargetPath;
        private string _renderer;
        private int _frames;
        private DateTime _frameCountStart;

        public RenderDebugViewModel(ICoreService coreService)
        {
            DisplayName = "RENDERING";
            _coreService = coreService;
        }

        public ImageSource CurrentFrame
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
            _coreService.FrameRendering += CoreServiceOnFrameRendering;
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            _coreService.FrameRendered -= CoreServiceOnFrameRendered;
            _coreService.FrameRendering -= CoreServiceOnFrameRendering;
            base.OnDeactivate();
        }

        private void CoreServiceOnFrameRendered(object sender, FrameRenderedEventArgs e)
        {
            Execute.OnUIThreadSync(() =>
            {
                SKImageInfo bitmapInfo = e.Texture.ImageInfo;
                RenderHeight = bitmapInfo.Height;
                RenderWidth = bitmapInfo.Width;

                // ReSharper disable twice CompareOfFloatsByEqualityOperator
                if (CurrentFrame is not WriteableBitmap writable || writable.Width != bitmapInfo.Width || writable.Height != bitmapInfo.Height)
                {
                    CurrentFrame = e.Texture.Surface.Snapshot().ToWriteableBitmap();
                    return;
                }

                using SKImage skImage = e.Texture.Surface.Snapshot();

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

                SKImageInfo info = new(skImage.Width, skImage.Height);
                writable.Lock();
                using (SKPixmap pixmap = new(info, writable.BackBuffer, writable.BackBufferStride))
                {
                    skImage.ReadPixels(pixmap, 0, 0);
                }

                writable.AddDirtyRect(new Int32Rect(0, 0, writable.PixelWidth, writable.PixelHeight));
                writable.Unlock();
            });
        }

        private void CoreServiceOnFrameRendering(object sender, FrameRenderingEventArgs e)
        {
            if (DateTime.Now - _frameCountStart >= TimeSpan.FromSeconds(1))
            {
                CurrentFps = _frames;
                Renderer = Constants.ManagedGraphicsContext != null ? Constants.ManagedGraphicsContext.GetType().Name : "Software";

                _frames = 0;
                _frameCountStart = DateTime.Now;
            }
            _frames++;
        }
    }
}