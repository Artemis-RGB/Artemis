using Artemis.Core.Tests.Infrastructure;
using Artemis.Core.Tests.Infrastructure.Plugins.LayerBrushes;
using Artemis.Core.Tests.Infrastructure.Plugins.LayerEffects;
using Artemis.Core.Tests.Infrastructure.Rendering;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core.Tests.Profiles;

/// <summary>
///     Effects that render beyond the element itself. Layers clip to their own LEDs, folders don't clip at all.
/// </summary>
public class GlowTests : RenderTestBase
{
    [Fact]
    public void LayerGlow_IsClippedToTheLedsOfTheLayer()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, _) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);
        Harness.AddEffect<GlowTestEffect>(layer);

        using RenderFrame frame = Render(profile);

        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
        frame.GetColor(Keyboard, LedId.Keyboard_S).ShouldBeColor(SKColors.Black);
        frame.GetColor(Keyboard, LedId.Keyboard_CapsLock).ShouldBeColor(SKColors.Black);
    }

    [Fact]
    public void FolderGlow_SpillsOntoNeighbouringLeds()
    {
        Profile profile = Harness.CreateProfile();
        Folder folder = Harness.AddFolder(profile.GetRootFolder());
        Harness.AddLayer<SolidColorTestBrush>(folder, [Led(LedId.Keyboard_A)]);
        Harness.AddEffect<GlowTestEffect>(folder);

        using RenderFrame frame = Render(profile);

        // S and Caps Lock are not part of any layer, the glow reaches them anyway
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
        frame.GetColor(Keyboard, LedId.Keyboard_S).Red.ShouldBeGreaterThan((byte) 32);
        frame.GetColor(Keyboard, LedId.Keyboard_CapsLock).Red.ShouldBeGreaterThan((byte) 32);
        frame.GetColor(Keyboard, LedId.Keyboard_F).ShouldBeColor(SKColors.Black);
    }

    [Fact]
    public void FolderGlow_IsCoveredByElementsAboveTheFolder()
    {
        Profile profile = Harness.CreateProfile();
        (_, SolidColorTestBrush above) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_S)], "Above");
        above.Properties.Color.SetCurrentValue(SKColors.Blue);
        Folder folder = Harness.AddFolder(profile.GetRootFolder());
        Harness.AddLayer<SolidColorTestBrush>(folder, [Led(LedId.Keyboard_A)]);
        Harness.AddEffect<GlowTestEffect>(folder);

        using RenderFrame frame = Render(profile);

        frame.GetColor(Keyboard, LedId.Keyboard_S).ShouldBeColor(SKColors.Blue);
    }

    [Fact]
    public void FolderGlow_CoversElementsBelowTheFolder()
    {
        Profile profile = Harness.CreateProfile();
        Folder folder = Harness.AddFolder(profile.GetRootFolder());
        Harness.AddLayer<SolidColorTestBrush>(folder, [Led(LedId.Keyboard_A)]);
        Harness.AddEffect<GlowTestEffect>(folder);
        (_, SolidColorTestBrush below) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_S)], "Below");
        below.Properties.Color.SetCurrentValue(SKColors.Lime);

        using RenderFrame frame = Render(profile);

        // The glow covers part of S, the rest of S remains green
        SKColor s = frame.GetColor(Keyboard, LedId.Keyboard_S);
        s.Red.ShouldBeGreaterThan((byte) 32);
        s.Green.ShouldBeGreaterThan((byte) 32);
        ((int) s.Red + s.Green).ShouldBeInRange(250, 260);
    }

    [Fact]
    public void FolderGlow_SpillsWithinItsParentFolderAndIsAffectedByItsEffects()
    {
        Profile profile = Harness.CreateProfile();
        Folder outer = Harness.AddFolder(profile.GetRootFolder(), "Outer");
        Folder glowing = Harness.AddFolder(outer, "Glowing");
        Harness.AddLayer<SolidColorTestBrush>(glowing, [Led(LedId.Keyboard_A)]);
        Harness.AddEffect<GlowTestEffect>(glowing);
        Harness.AddEffect<ChannelSwapTestEffect>(outer);

        using RenderFrame frame = Render(profile);

        // The spilled glow is part of the outer folder's content, so it gets swapped from red to blue as well
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Blue);
        SKColor s = frame.GetColor(Keyboard, LedId.Keyboard_S);
        s.Blue.ShouldBeGreaterThan((byte) 32);
        s.Red.ShouldBe((byte) 0);
    }

    [Fact]
    public void FolderGlow_GrowsTheBlendedResultOfItsChildren()
    {
        Profile profile = Harness.CreateProfile();
        Folder folder = Harness.AddFolder(profile.GetRootFolder());
        (Layer blue, SolidColorTestBrush blueBrush) = Harness.AddLayer<SolidColorTestBrush>(folder, [Led(LedId.Keyboard_A)], "Blue");
        blueBrush.Properties.Color.SetCurrentValue(SKColors.Blue);
        blue.General.BlendMode.SetCurrentValue(SKBlendMode.Plus);
        Harness.AddLayer<SolidColorTestBrush>(folder, [Led(LedId.Keyboard_A)], "Red");
        Harness.AddEffect<GlowTestEffect>(folder);

        using RenderFrame frame = Render(profile);

        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Magenta);
        SKColor s = frame.GetColor(Keyboard, LedId.Keyboard_S);
        ((int) s.Red).ShouldBeInRange(s.Blue - 2, s.Blue + 2);
        s.Red.ShouldBeGreaterThan((byte) 32);
    }

    [Fact]
    public void LayerGlowInAGlowingFolder_IsStillClippedBeforeTheFolderGrowsIt()
    {
        Profile profile = Harness.CreateProfile();
        Folder folder = Harness.AddFolder(profile.GetRootFolder());
        (Layer layer, _) = Harness.AddLayer<SolidColorTestBrush>(folder, [Led(LedId.Keyboard_A)]);
        GlowTestEffect layerGlow = Harness.AddEffect<GlowTestEffect>(layer);
        layerGlow.Properties.Radius.SetCurrentValue(40);
        Harness.AddEffect<GlowTestEffect>(folder);

        using RenderFrame frame = Render(profile);

        // A large layer glow would reach F, but it is clipped to A, the folder only grows A by its own radius
        frame.GetColor(Keyboard, LedId.Keyboard_S).Red.ShouldBeGreaterThan((byte) 32);
        frame.GetColor(Keyboard, LedId.Keyboard_F).ShouldBeColor(SKColors.Black);
    }
}
