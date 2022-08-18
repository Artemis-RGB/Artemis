using Artemis.Core;
using Artemis.VisualScripting.Nodes.Image.Screens;
using ScreenCapture.NET;
using SkiaSharp;

namespace Artemis.VisualScripting.Nodes.Image;

[Node("Capture Screen", "Captures a region of the screen", "Image", OutputType = typeof(SKBitmap))]
public class CaptureScreenNode : Node<object, CaptureScreenNodeCustomViewModel>
{
    #region Properties & Fields

    private static readonly Thread _thread;
    private static readonly IScreenCaptureService _screenCaptureService = new DX11ScreenCaptureService();
    private static readonly IScreenCapture _screenCapture;

    static CaptureScreenNode()
    {
        IEnumerable<GraphicsCard> graphicsCards = _screenCaptureService.GetGraphicsCards();
        IEnumerable<Display> displays = _screenCaptureService.GetDisplays(graphicsCards.First());
        _screenCapture = _screenCaptureService.GetScreenCapture(displays.First());

        _thread = new Thread(() =>
                             {
                                 while (true)
                                     _screenCapture.CaptureScreen();
                             });
        _thread.Start();
    }

    private CaptureZone _captureZone;

    public OutputPin<SKBitmap> Output { get; set; }

    #endregion

    #region Constructors

    public CaptureScreenNode()
        : base("Capture Screen", "Captures a region of the screen")
    {
        Output = CreateOutputPin<SKBitmap>("Image");

        _captureZone = _screenCapture.RegisterCaptureZone(20, 20, 256, 256, 1);
    }

    #endregion

    #region Methods

    public override unsafe void Evaluate()
    {
        lock (_captureZone.Buffer)
        {
            ReadOnlySpan<byte> capture = _captureZone.Buffer;
            if (capture.IsEmpty) return;

            fixed (byte* ptr = capture)
                Output.Value = SKBitmap.FromImage(SKImage.FromPixels(new SKImageInfo(_captureZone.Width, _captureZone.Height, SKColorType.Bgra8888, SKAlphaType.Opaque), new IntPtr(ptr), _captureZone.Stride));

            //TODO DarthAffe 18.08.2022: Dispose Output or better reuse it
        }
    }

    #endregion
}