using Artemis.Core.Tests.Infrastructure;
using Artemis.Core.Tests.Infrastructure.Plugins.LayerBrushes;
using Artemis.Core.Tests.Infrastructure.Rendering;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core.Tests.Profiles;

/// <summary>
///     Every blend mode in the situations that matter for Artemis. The expected colors come from a reference render: the
///     same scene drawn directly with Skia onto a single pixel, stacked the way Artemis does it. The canvas is cleared to
///     black, the root folder and every other folder composite their children in a layer of their own, and every layer
///     composites itself with its blend mode and opacity.
///     <para>
///         Skia's blend math is taken as a given, what's tested is that Artemis composites each element in the right place
///         with the right mode and alpha.
///     </para>
/// </summary>
public class BlendModeTests : RenderTestBase
{
    private static readonly SKColor Source = new(200, 100, 50);
    private static readonly SKColor Destination = new(40, 160, 220);

    public static TheoryData<SKBlendMode> BlendModes => new(Enum.GetValues<SKBlendMode>());

    [Theory]
    [MemberData(nameof(BlendModes))]
    public void BlendMode_OverASiblingInTheSameFolder(SKBlendMode blendMode)
    {
        Profile profile = Harness.CreateProfile();
        AddSolid(profile.GetRootFolder(), Source, blendMode, 100);
        AddSolid(profile.GetRootFolder(), Destination, SKBlendMode.SrcOver, 100);

        using RenderFrame frame = Harness.Render(profile, TimeSpan.FromSeconds(0.1));
        SaveOutput(frame, $"{TestName}_{blendMode}");

        SKColor expected = ReferenceRender(canvas =>
        {
            using (RootFolder(canvas))
            {
                DrawLayer(canvas, Destination, SKBlendMode.SrcOver, 255);
                DrawLayer(canvas, Source, blendMode, 255);
            }
        });
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(expected);
    }

    [Theory]
    [MemberData(nameof(BlendModes))]
    public void BlendMode_WithHalfLayerOpacity(SKBlendMode blendMode)
    {
        Profile profile = Harness.CreateProfile();
        AddSolid(profile.GetRootFolder(), Source, blendMode, 50);
        AddSolid(profile.GetRootFolder(), Destination, SKBlendMode.SrcOver, 100);

        using RenderFrame frame = Harness.Render(profile, TimeSpan.FromSeconds(0.1));
        SaveOutput(frame, $"{TestName}_{blendMode}");

        SKColor expected = ReferenceRender(canvas =>
        {
            using (RootFolder(canvas))
            {
                DrawLayer(canvas, Destination, SKBlendMode.SrcOver, 255);
                // Artemis turns the opacity percentage into an alpha with (byte) (opacity * 2.55f)
                DrawLayer(canvas, Source, blendMode, 127);
            }
        });
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(expected);
    }

    [Theory]
    [MemberData(nameof(BlendModes))]
    public void BlendMode_InAFolderAboveTheDestination(SKBlendMode blendMode)
    {
        Profile profile = Harness.CreateProfile();
        Folder folder = Harness.AddFolder(profile.GetRootFolder());
        AddSolid(folder, Source, blendMode, 100);
        AddSolid(profile.GetRootFolder(), Destination, SKBlendMode.SrcOver, 100);

        using RenderFrame frame = Harness.Render(profile, TimeSpan.FromSeconds(0.1));
        SaveOutput(frame, $"{TestName}_{blendMode}");

        // The folder isolates the blending layer, it blends with the empty folder and the folder is drawn over the destination
        SKColor expected = ReferenceRender(canvas =>
        {
            using (RootFolder(canvas))
            {
                DrawLayer(canvas, Destination, SKBlendMode.SrcOver, 255);
                using (Folder(canvas))
                    DrawLayer(canvas, Source, blendMode, 255);
            }
        });
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(expected);
    }

    /// <summary>
    ///     Hand calculated for an opaque source S = (200, 100, 50) and an opaque destination D = (40, 160, 220), independent
    ///     of the reference render. In a folder the blend happens against the empty folder, which is then drawn over D.
    /// </summary>
    [Theory]
    [InlineData(SKBlendMode.SrcATop, "#C86432", "#28A0DC")] // S atop D is S, S atop nothing is nothing
    [InlineData(SKBlendMode.SrcIn, "#C86432", "#28A0DC")] // S within D is S, S within nothing is nothing
    [InlineData(SKBlendMode.DstOut, "#000000", "#28A0DC")] // Cuts D away, in a folder there is nothing to cut
    [InlineData(SKBlendMode.Clear, "#000000", "#28A0DC")] // Clears D, in a folder it clears nothing
    [InlineData(SKBlendMode.Xor, "#000000", "#C86432")] // Two opaque colors cancel out, against nothing S remains
    [InlineData(SKBlendMode.Screen, "#D1C5E3", "#C86432")] // S + D - S * D, against nothing S remains
    public void CommonBlendModes_BehaveDifferentlyInAFolder(SKBlendMode blendMode, string expectedInRoot, string expectedInFolder)
    {
        Profile profile = Harness.CreateProfile();
        Folder root = profile.GetRootFolder();
        // A: blending layer in a folder above D
        Folder folder = Harness.AddFolder(root);
        (Layer inFolder, SolidColorTestBrush inFolderBrush) = Harness.AddLayer<SolidColorTestBrush>(folder, [Led(LedId.Keyboard_A)], "In folder");
        inFolderBrush.Properties.Color.SetCurrentValue(Source);
        inFolder.General.BlendMode.SetCurrentValue(blendMode);
        // S: blending layer directly above D
        (Layer inRoot, SolidColorTestBrush inRootBrush) = Harness.AddLayer<SolidColorTestBrush>(root, [Led(LedId.Keyboard_S)], "In root");
        inRootBrush.Properties.Color.SetCurrentValue(Source);
        inRoot.General.BlendMode.SetCurrentValue(blendMode);
        (_, SolidColorTestBrush destination) = Harness.AddLayer<SolidColorTestBrush>(root, Leds(LedId.Keyboard_A, LedId.Keyboard_S), "Destination");
        destination.Properties.Color.SetCurrentValue(Destination);

        using RenderFrame frame = Harness.Render(profile, TimeSpan.FromSeconds(0.1));
        SaveOutput(frame, $"{TestName}_{blendMode}");

        frame.GetColor(Keyboard, LedId.Keyboard_S).ShouldBeColor(SKColor.Parse(expectedInRoot));
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColor.Parse(expectedInFolder));
    }

    private void AddSolid(Folder parent, SKColor color, SKBlendMode blendMode, float opacity)
    {
        (Layer layer, SolidColorTestBrush brush) = Harness.AddLayer<SolidColorTestBrush>(parent, [Led(LedId.Keyboard_A)], $"{color} {blendMode}");
        brush.Properties.Color.SetCurrentValue(color);
        layer.General.BlendMode.SetCurrentValue(blendMode);
        layer.Transform.Opacity.SetCurrentValue(opacity);
    }

    #region Reference render

    private static SKColor ReferenceRender(Action<SKCanvas> draw)
    {
        using SKBitmap bitmap = new(1, 1);
        using SKCanvas canvas = new(bitmap);
        // SurfaceManager clears the canvas to black before every frame
        canvas.Clear(SKColors.Black);
        draw(canvas);
        canvas.Flush();
        return bitmap.GetPixel(0, 0);
    }

    /// <summary>
    ///     Folder.Render composites its children in a layer, using a default paint
    /// </summary>
    private static SKAutoCanvasRestore Folder(SKCanvas canvas)
    {
        // Created before saving the layer so disposing it restores the layer
        SKAutoCanvasRestore restore = new(canvas, false);
        using SKPaint paint = new();
        canvas.SaveLayer(paint);
        return restore;
    }

    private static SKAutoCanvasRestore RootFolder(SKCanvas canvas)
    {
        return Folder(canvas);
    }

    /// <summary>
    ///     Layer.DelegateRendering composites the layer with its blend mode and opacity, the brush draws inside that layer
    /// </summary>
    private static void DrawLayer(SKCanvas canvas, SKColor color, SKBlendMode blendMode, byte alpha)
    {
        using SKPaint layerPaint = new() {BlendMode = blendMode, Color = new SKColor(0, 0, 0, alpha)};
        canvas.SaveLayer(layerPaint);
        using SKPaint brushPaint = new() {Color = color};
        canvas.DrawRect(SKRect.Create(0, 0, 1, 1), brushPaint);
        canvas.Restore();
    }

    #endregion
}
