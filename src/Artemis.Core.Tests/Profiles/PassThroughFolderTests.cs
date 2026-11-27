using Artemis.Core.Tests.Infrastructure;
using Artemis.Core.Tests.Infrastructure.Plugins.LayerBrushes;
using Artemis.Core.Tests.Infrastructure.Plugins.LayerEffects;
using Artemis.Core.Tests.Infrastructure.Rendering;
using Artemis.Storage.Entities.Profile;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core.Tests.Profiles;

/// <summary>
///     Pass-through folders render their children straight onto whatever is below the folder, so the children's blend modes
///     reach outside the folder. Effects that only draw around the content, like a glow, still work on the folder as a whole.
///     The glow test effect grows content by 10 units, about half a key.
/// </summary>
public class PassThroughFolderTests : RenderTestBase
{
    [Theory]
    [InlineData(true, "#FF00FF")]
    [InlineData(false, "#FF0000")]
    public void ChildBlendMode_ReachesElementsBelowTheFolderOnlyWhenPassingThrough(bool passThrough, string expected)
    {
        Profile profile = Harness.CreateProfile();
        Folder folder = Harness.AddFolder(profile.GetRootFolder());
        folder.General.BlendMode.SetCurrentValue(passThrough ? FolderBlendMode.PassThrough : FolderBlendMode.SrcOver);
        AddSolid(folder, LedId.Keyboard_A, SKColors.Red, SKBlendMode.Plus);
        AddSolid(profile.GetRootFolder(), LedId.Keyboard_A, SKColors.Blue, SKBlendMode.SrcOver, "Below");

        using RenderFrame frame = Render(profile);

        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColor.Parse(expected));
    }

    [Theory]
    [InlineData(true, "#FF00FF")]
    [InlineData(false, "#FF0000")]
    public void RootFolderBlendMode_ReachesProfilesBelowOnlyWhenPassingThrough(bool passThrough, string expected)
    {
        Profile below = Harness.CreateProfile("Below");
        AddSolid(below.GetRootFolder(), LedId.Keyboard_A, SKColors.Blue, SKBlendMode.SrcOver);
        Profile tint = Harness.CreateProfile("Tint");
        tint.Configuration.BlendMode = passThrough ? FolderBlendMode.PassThrough : FolderBlendMode.SrcOver;
        AddSolid(tint.GetRootFolder(), LedId.Keyboard_A, SKColors.Red, SKBlendMode.Plus);

        // Profiles are rendered one after the other onto the same canvas
        using RenderFrame frame = Harness.Render(canvas =>
        {
            foreach (Profile profile in new[] {below, tint})
            {
                profile.Update(0.1);
                profile.Render(canvas, SKPointI.Empty, null);
            }
        });
        SaveOutput(frame);

        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColor.Parse(expected));
    }

    [Fact]
    public void Glow_GrowsAroundTheChildrenTogetherWithoutCoveringThem()
    {
        Profile profile = Harness.CreateProfile();
        Folder folder = Harness.AddFolder(profile.GetRootFolder());
        folder.General.BlendMode.SetCurrentValue(FolderBlendMode.PassThrough);
        AddSolid(folder, LedId.Keyboard_A, SKColors.Red, SKBlendMode.SrcOver);
        AddSolid(folder, LedId.Keyboard_S, SKColors.Blue, SKBlendMode.SrcOver);
        Harness.AddEffect<GlowTestEffect>(folder);

        using RenderFrame frame = Render(profile);

        // Neither child's glow reaches the other, the glow only grows outwards from the two of them
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
        frame.GetColor(Keyboard, LedId.Keyboard_S).ShouldBeColor(SKColors.Blue);
        frame.GetColor(Keyboard, LedId.Keyboard_CapsLock).Red.ShouldBeGreaterThan((byte) 32);
        frame.GetColor(Keyboard, LedId.Keyboard_D).Blue.ShouldBeGreaterThan((byte) 32);
        frame.GetColor(Keyboard, LedId.Keyboard_G).ShouldBeColor(SKColors.Black);
    }

    [Fact]
    public void Glow_CoversElementsBelowWhileTheChildrenBlendWithThem()
    {
        Profile profile = Harness.CreateProfile();
        Folder folder = Harness.AddFolder(profile.GetRootFolder());
        folder.General.BlendMode.SetCurrentValue(FolderBlendMode.PassThrough);
        AddSolid(folder, LedId.Keyboard_A, SKColors.Red, SKBlendMode.Plus);
        Harness.AddEffect<GlowTestEffect>(folder);
        (_, SolidColorTestBrush below) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), Leds(LedId.Keyboard_A, LedId.Keyboard_S), "Below");
        below.Properties.Color.SetCurrentValue(SKColors.Blue);

        using RenderFrame frame = Render(profile);

        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Magenta);
        // The glow covers part of S, the rest of S remains blue
        SKColor s = frame.GetColor(Keyboard, LedId.Keyboard_S);
        s.Red.ShouldBeGreaterThan((byte) 32);
        s.Blue.ShouldBeGreaterThan((byte) 32);
        ((int) s.Red + s.Blue).ShouldBeInRange(250, 260);
    }

    [Fact]
    public void Glow_LeavesTranslucentChildrenAsTheyAre()
    {
        Profile profile = Harness.CreateProfile();
        Folder folder = Harness.AddFolder(profile.GetRootFolder());
        folder.General.BlendMode.SetCurrentValue(FolderBlendMode.PassThrough);
        Layer child = AddSolid(folder, LedId.Keyboard_A, SKColors.Red, SKBlendMode.SrcOver);
        child.Transform.Opacity.SetCurrentValue(50);
        GlowTestEffect glow = Harness.AddEffect<GlowTestEffect>(folder);
        AddSolid(profile.GetRootFolder(), LedId.Keyboard_A, SKColors.Blue, SKBlendMode.SrcOver, "Below");

        using RenderFrame withGlow = Render(profile);
        glow.Properties.IsEnabled.SetCurrentValue(false);
        using RenderFrame withoutGlow = Render(profile);

        withGlow.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(withoutGlow.GetColor(Keyboard, LedId.Keyboard_A));
    }

    [Fact]
    public void ContentEffect_MakesThePassThroughFolderIsolated()
    {
        Profile profile = Harness.CreateProfile();
        Folder folder = Harness.AddFolder(profile.GetRootFolder());
        folder.General.BlendMode.SetCurrentValue(FolderBlendMode.PassThrough);
        AddSolid(folder, LedId.Keyboard_A, SKColors.Red, SKBlendMode.Plus);
        Harness.AddEffect<ChannelSwapTestEffect>(folder);
        AddSolid(profile.GetRootFolder(), LedId.Keyboard_A, SKColors.Lime, SKBlendMode.SrcOver, "Below");

        using RenderFrame frame = Render(profile);

        // Red is added to nothing inside the folder and swapped to blue, passing through it would have been added to the lime
        folder.RendersPassThrough.ShouldBeFalse();
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Blue);
    }

    [Fact]
    public void DisabledContentEffect_StillMakesThePassThroughFolderIsolated()
    {
        Profile profile = Harness.CreateProfile();
        Folder folder = Harness.AddFolder(profile.GetRootFolder());
        folder.General.BlendMode.SetCurrentValue(FolderBlendMode.PassThrough);
        AddSolid(folder, LedId.Keyboard_A, SKColors.Red, SKBlendMode.SrcOver);
        ChannelSwapTestEffect swap = Harness.AddEffect<ChannelSwapTestEffect>(folder);
        swap.Properties.IsEnabled.SetCurrentValue(false);

        folder.RendersPassThrough.ShouldBeFalse();
    }

    [Fact]
    public void PassThrough_SurvivesSavingAndLoading()
    {
        Profile profile = Harness.CreateProfile();
        Folder folder = Harness.AddFolder(profile.GetRootFolder());
        folder.General.BlendMode.SetCurrentValue(FolderBlendMode.PassThrough);
        AddSolid(folder, LedId.Keyboard_A, SKColors.Red, SKBlendMode.SrcOver);

        Profile loaded = Harness.SaveAndReload(profile);

        loaded.GetRootFolder().Children.OfType<Folder>().Single().General.BlendMode.CurrentValue.ShouldBe(FolderBlendMode.PassThrough);
        loaded.GetRootFolder().BlendMode.ShouldBe(FolderBlendMode.SrcOver);
    }

    [Fact]
    public void FolderWithoutGeneralProperties_LoadsWithTheDefaultBlendMode()
    {
        Profile profile = Harness.CreateProfile();
        Folder folder = Harness.AddFolder(profile.GetRootFolder());
        folder.General.BlendMode.SetCurrentValue(FolderBlendMode.PassThrough);
        AddSolid(folder, LedId.Keyboard_A, SKColors.Red, SKBlendMode.Plus);
        AddSolid(profile.GetRootFolder(), LedId.Keyboard_A, SKColors.Blue, SKBlendMode.SrcOver, "Below");

        // Profiles saved before folders had general properties
        ProfileEntity entity = Harness.Save(profile);
        foreach (FolderEntity folderEntity in entity.Folders)
            folderEntity.GeneralPropertyGroup = null;
        Profile loaded = Harness.Load(entity);
        using RenderFrame frame = Render(loaded);

        loaded.GetRootFolder().Children.OfType<Folder>().Single().General.BlendMode.CurrentValue.ShouldBe(FolderBlendMode.SrcOver);
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
    }

    [Fact]
    public void ProfileBlendMode_SurvivesSavingAndLoading()
    {
        ProfileConfiguration configuration = new(new ProfileCategory("Tests", 0), "Test profile", "TestTube");
        configuration.BlendMode.ShouldBe(FolderBlendMode.SrcOver);
        configuration.BlendMode = FolderBlendMode.PassThrough;

        configuration.Save();
        configuration.BlendMode = FolderBlendMode.Multiply;
        configuration.Load();

        configuration.BlendMode.ShouldBe(FolderBlendMode.PassThrough);
        configuration.Dispose();
    }

    [Fact]
    public void ProfileWithoutBlendMode_LoadsWithTheDefaultBlendMode()
    {
        ProfileConfiguration configuration = new(new ProfileCategory("Tests", 0), "Test profile", "TestTube");

        // Profiles saved before profiles had a blend mode
        configuration.Entity.ProfileConfiguration.BlendMode = null;
        configuration.Load();

        configuration.BlendMode.ShouldBe(FolderBlendMode.SrcOver);
        configuration.Dispose();
    }

    [Fact]
    public void FolderBlendMode_MatchesEverySkiaBlendModeByName()
    {
        Enum.GetValues<FolderBlendMode>().Length.ShouldBe(Enum.GetValues<SKBlendMode>().Length + 1);
        foreach (SKBlendMode blendMode in Enum.GetValues<SKBlendMode>())
        {
            FolderBlendMode folderBlendMode = Enum.Parse<FolderBlendMode>(blendMode.ToString());
            FolderGeneralProperties.ToSKBlendMode(folderBlendMode).ShouldBe(blendMode);
            FolderGeneralProperties.FromSKBlendMode(blendMode).ShouldBe(folderBlendMode);
        }

        FolderGeneralProperties.ToSKBlendMode(FolderBlendMode.PassThrough).ShouldBe(SKBlendMode.SrcOver);
    }

    private Layer AddSolid(Folder parent, LedId ledId, SKColor color, SKBlendMode blendMode, string name = "Test layer")
    {
        (Layer layer, SolidColorTestBrush brush) = Harness.AddLayer<SolidColorTestBrush>(parent, [Led(ledId)], name);
        brush.Properties.Color.SetCurrentValue(color);
        layer.General.BlendMode.SetCurrentValue(blendMode);
        return layer;
    }
}
