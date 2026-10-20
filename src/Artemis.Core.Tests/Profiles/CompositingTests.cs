using Artemis.Core.Tests.Infrastructure;
using Artemis.Core.Tests.Infrastructure.Plugins.LayerBrushes;
using Artemis.Core.Tests.Infrastructure.Plugins.LayerEffects;
using Artemis.Core.Tests.Infrastructure.Rendering;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core.Tests.Profiles;

/// <summary>
///     Z-order and blend modes across folders. Every folder (the root folder included) composites its children into a
///     layer of its own, so a blend mode only blends with siblings below it in the same folder, never with what is below
///     the folder.
/// </summary>
public class CompositingTests : RenderTestBase
{
    #region Z-order

    [Fact]
    public void ZOrder_IsDepthFirstWithTheFirstChildOnTop()
    {
        Profile profile = Harness.CreateProfile();
        Folder root = profile.GetRootFolder();
        Folder folderA = Harness.AddFolder(root, "A");
        AddSolid(folderA, SKColors.Red, LedId.Keyboard_1, LedId.Keyboard_2, LedId.Keyboard_3);
        Folder folderA2 = Harness.AddFolder(folderA, "A2");
        AddSolid(folderA2, SKColors.Lime, LedId.Keyboard_2, LedId.Keyboard_3, LedId.Keyboard_4);
        AddSolid(root, SKColors.Blue, LedId.Keyboard_3, LedId.Keyboard_4, LedId.Keyboard_5);
        Folder folderC = Harness.AddFolder(root, "C");
        AddSolid(folderC, SKColors.Yellow, LedId.Keyboard_4, LedId.Keyboard_5, LedId.Keyboard_6);

        using RenderFrame frame = Render(profile);

        frame.GetColor(Keyboard, LedId.Keyboard_1).ShouldBeColor(SKColors.Red);
        frame.GetColor(Keyboard, LedId.Keyboard_2).ShouldBeColor(SKColors.Red);
        frame.GetColor(Keyboard, LedId.Keyboard_3).ShouldBeColor(SKColors.Red);
        frame.GetColor(Keyboard, LedId.Keyboard_4).ShouldBeColor(SKColors.Lime);
        frame.GetColor(Keyboard, LedId.Keyboard_5).ShouldBeColor(SKColors.Blue);
        frame.GetColor(Keyboard, LedId.Keyboard_6).ShouldBeColor(SKColors.Yellow);
        frame.GetColor(Keyboard, LedId.Keyboard_7).ShouldBeColor(SKColors.Black);
    }

    [Fact]
    public void ZOrder_MovingAFolderMovesAllOfItsChildren()
    {
        Profile profile = Harness.CreateProfile();
        Folder root = profile.GetRootFolder();
        Folder top = Harness.AddFolder(root, "Top");
        AddSolid(top, SKColors.Red, LedId.Keyboard_1, LedId.Keyboard_2);
        Folder bottom = Harness.AddFolder(root, "Bottom");
        AddSolid(bottom, SKColors.Blue, LedId.Keyboard_1);
        AddSolid(bottom, SKColors.Lime, LedId.Keyboard_2);

        root.RemoveChild(bottom);
        root.AddChild(bottom, 0);
        using RenderFrame frame = Render(profile);

        frame.GetColor(Keyboard, LedId.Keyboard_1).ShouldBeColor(SKColors.Blue);
        frame.GetColor(Keyboard, LedId.Keyboard_2).ShouldBeColor(SKColors.Lime);
    }

    #endregion

    #region Blend modes

    [Fact]
    public void BlendMode_BlendsWithSiblingsBelowItInTheSameFolder()
    {
        Profile profile = Harness.CreateProfile();
        Folder folder = Harness.AddFolder(profile.GetRootFolder());
        AddSolid(folder, SKColors.Blue, SKBlendMode.Plus, LedId.Keyboard_A);
        AddSolid(folder, SKColors.Red, LedId.Keyboard_A);

        using RenderFrame frame = Render(profile);

        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Magenta);
    }

    [Fact]
    public void BlendMode_DoesNotReachElementsBelowTheFolder()
    {
        Profile profile = Harness.CreateProfile();
        Folder root = profile.GetRootFolder();
        // A: the blending layer is in a folder above the red layer
        Folder folder = Harness.AddFolder(root);
        AddSolid(folder, SKColors.Blue, SKBlendMode.Plus, LedId.Keyboard_A);
        // S: the same setup without the folder
        AddSolid(root, SKColors.Blue, SKBlendMode.Plus, LedId.Keyboard_S);
        AddSolid(root, SKColors.Red, LedId.Keyboard_A, LedId.Keyboard_S);

        using RenderFrame frame = Render(profile);

        // Inside the folder the layer is added to nothing, the folder then simply covers the red layer
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Blue);
        frame.GetColor(Keyboard, LedId.Keyboard_S).ShouldBeColor(SKColors.Magenta);
    }

    [Fact]
    public void BlendMode_DoesNotReachBeyondANestedFolder()
    {
        Profile profile = Harness.CreateProfile();
        Folder outer = Harness.AddFolder(profile.GetRootFolder(), "Outer");
        Folder nested = Harness.AddFolder(outer, "Nested");
        AddSolid(nested, SKColors.Blue, SKBlendMode.Plus, LedId.Keyboard_A);
        AddSolid(outer, SKColors.Red, LedId.Keyboard_A);

        using RenderFrame frame = Render(profile);

        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Blue);
    }

    [Fact]
    public void MultiplyInAFolder_LeavesTheColorUnchangedInsteadOfDarkeningWhatIsBelow()
    {
        Profile profile = Harness.CreateProfile();
        Folder root = profile.GetRootFolder();
        SKColor orange = new(255, 128, 0);
        Folder folder = Harness.AddFolder(root);
        AddSolid(folder, orange, SKBlendMode.Multiply, LedId.Keyboard_A);
        AddSolid(root, orange, SKBlendMode.Multiply, LedId.Keyboard_S);
        AddSolid(root, new SKColor(128, 128, 255), LedId.Keyboard_A, LedId.Keyboard_S);

        using RenderFrame frame = Render(profile);

        // Multiplying onto a transparent folder yields the color itself
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(orange);
        frame.GetColor(Keyboard, LedId.Keyboard_S).ShouldBeColor(new SKColor(128, 64, 0));
    }

    [Fact]
    public void MaskLayer_OnlyCutsThroughSiblingsInItsFolder()
    {
        Profile profile = Harness.CreateProfile();
        Folder root = profile.GetRootFolder();
        // In a folder: the mask punches a hole in the red layer, the green layer below the folder shows through
        Folder folder = Harness.AddFolder(root);
        AddSolid(folder, SKColors.White, SKBlendMode.DstOut, LedId.Keyboard_S);
        AddSolid(folder, SKColors.Red, LedId.Keyboard_A, LedId.Keyboard_S, LedId.Keyboard_D);
        AddSolid(root, SKColors.Lime, LedId.Keyboard_A, LedId.Keyboard_S, LedId.Keyboard_D);
        // In the root folder: the mask punches a hole through everything below it
        AddSolid(root, SKColors.White, SKBlendMode.DstOut, LedId.Keyboard_J, order: 0);
        AddSolid(root, SKColors.Red, LedId.Keyboard_H, LedId.Keyboard_J, LedId.Keyboard_K);
        AddSolid(root, SKColors.Lime, LedId.Keyboard_H, LedId.Keyboard_J, LedId.Keyboard_K);

        using RenderFrame frame = Render(profile);

        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
        frame.GetColor(Keyboard, LedId.Keyboard_S).ShouldBeColor(SKColors.Lime);
        frame.GetColor(Keyboard, LedId.Keyboard_D).ShouldBeColor(SKColors.Red);
        frame.GetColor(Keyboard, LedId.Keyboard_H).ShouldBeColor(SKColors.Red);
        frame.GetColor(Keyboard, LedId.Keyboard_J).ShouldBeColor(SKColors.Black);
        frame.GetColor(Keyboard, LedId.Keyboard_K).ShouldBeColor(SKColors.Red);
    }

    [Fact]
    public void BlendMode_IsCombinedWithTheLayerOpacity()
    {
        Profile profile = Harness.CreateProfile();
        Folder folder = Harness.AddFolder(profile.GetRootFolder());
        Layer blending = AddSolid(folder, SKColors.Blue, SKBlendMode.Plus, LedId.Keyboard_A);
        blending.Transform.Opacity.SetCurrentValue(50);
        AddSolid(folder, SKColors.Red, LedId.Keyboard_A);

        using RenderFrame frame = Render(profile);

        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(new SKColor(255, 0, 127));
    }

    [Fact]
    public void FolderEffect_AppliesToTheBlendedResultOfItsChildren()
    {
        Profile profile = Harness.CreateProfile();
        Folder folder = Harness.AddFolder(profile.GetRootFolder());
        AddSolid(folder, SKColors.Blue, SKBlendMode.Plus, LedId.Keyboard_A);
        AddSolid(folder, SKColors.Red, LedId.Keyboard_A);
        Harness.AddEffect<OpacityTestEffect>(folder);

        using RenderFrame frame = Render(profile);

        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(new SKColor(127, 0, 127));
    }

    #endregion

    private Layer AddSolid(Folder parent, SKColor color, params LedId[] leds)
    {
        return AddSolid(parent, color, SKBlendMode.SrcOver, leds);
    }

    private Layer AddSolid(Folder parent, SKColor color, SKBlendMode blendMode, params LedId[] leds)
    {
        return AddSolid(parent, color, blendMode, null, leds);
    }

    private Layer AddSolid(Folder parent, SKColor color, SKBlendMode blendMode, LedId led, int order)
    {
        return AddSolid(parent, color, blendMode, order, led);
    }

    private Layer AddSolid(Folder parent, SKColor color, SKBlendMode blendMode, int? order, params LedId[] leds)
    {
        (Layer layer, SolidColorTestBrush brush) = Harness.AddLayer<SolidColorTestBrush>(parent, Leds(leds), $"{color} {blendMode}", order);
        brush.Properties.Color.SetCurrentValue(color);
        layer.General.BlendMode.SetCurrentValue(blendMode);
        return layer;
    }
}
