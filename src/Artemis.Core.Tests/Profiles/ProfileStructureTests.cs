using Artemis.Core.Tests.Infrastructure;
using Artemis.Core.Tests.Infrastructure.Plugins.LayerBrushes;
using Artemis.Core.Tests.Infrastructure.Rendering;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core.Tests.Profiles;

public class ProfileStructureTests : RenderTestBase
{
    [Fact]
    public void EmptyProfile_RendersAllLedsBlack()
    {
        Profile profile = Harness.CreateProfile();

        using RenderFrame frame = Render(profile);

        frame.LedColors.Values.ShouldAllBe(c => c == SKColors.Black);
    }

    [Fact]
    public void Layer_OnlyColorsItsOwnLeds()
    {
        Profile profile = Harness.CreateProfile();
        (_, SolidColorTestBrush brush) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), Leds(LedId.Keyboard_W, LedId.Keyboard_A, LedId.Keyboard_S, LedId.Keyboard_D));
        brush.Properties.Color.SetCurrentValue(SKColors.Lime);

        using RenderFrame frame = Render(profile);

        frame.GetColor(Keyboard, LedId.Keyboard_W).ShouldBeColor(SKColors.Lime);
        frame.GetColor(Keyboard, LedId.Keyboard_D).ShouldBeColor(SKColors.Lime);
        frame.GetColor(Keyboard, LedId.Keyboard_Q).ShouldBeColor(SKColors.Black);
        frame.GetColor(Keyboard, LedId.Keyboard_E).ShouldBeColor(SKColors.Black);
    }

    [Fact]
    public void LayerWithoutLeds_RendersNothing()
    {
        Profile profile = Harness.CreateProfile();
        Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), []);

        using RenderFrame frame = Render(profile);

        frame.LedColors.Values.ShouldAllBe(c => c == SKColors.Black);
    }

    [Fact]
    public void FirstChild_RendersOnTop()
    {
        Profile profile = Harness.CreateProfile();
        (_, SolidColorTestBrush top) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), Leds(LedId.Keyboard_A, LedId.Keyboard_S), "Top");
        (_, SolidColorTestBrush bottom) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), Leds(LedId.Keyboard_S, LedId.Keyboard_D), "Bottom");
        top.Properties.Color.SetCurrentValue(SKColors.Red);
        bottom.Properties.Color.SetCurrentValue(SKColors.Blue);

        using RenderFrame frame = Render(profile);

        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
        frame.GetColor(Keyboard, LedId.Keyboard_S).ShouldBeColor(SKColors.Red);
        frame.GetColor(Keyboard, LedId.Keyboard_D).ShouldBeColor(SKColors.Blue);
    }

    [Fact]
    public void ChildAddedAtOrderZero_RendersOnTop()
    {
        Profile profile = Harness.CreateProfile();
        (_, SolidColorTestBrush first) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)], "First");
        (Layer insertedLayer, SolidColorTestBrush inserted) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)], "Inserted", order: 0);
        first.Properties.Color.SetCurrentValue(SKColors.Red);
        inserted.Properties.Color.SetCurrentValue(SKColors.Blue);

        using RenderFrame frame = Render(profile);

        profile.GetRootFolder().Children[0].ShouldBe(insertedLayer);
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Blue);
    }

    [Fact]
    public void ChildOrder_IsStreamlinedWhenInserting()
    {
        Profile profile = Harness.CreateProfile();
        Folder root = profile.GetRootFolder();
        (Layer a, _) = Harness.AddLayer<SolidColorTestBrush>(root, [], "A");
        (Layer b, _) = Harness.AddLayer<SolidColorTestBrush>(root, [], "B");
        (Layer c, _) = Harness.AddLayer<SolidColorTestBrush>(root, [], "C", order: 1);

        root.Children.ShouldBe([a, c, b]);
        root.Children.Select(child => child.Order).ShouldBe([1, 2, 3]);
    }

    [Fact]
    public void SuspendedLayer_IsNotRendered()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, _) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);
        layer.Suspended = true;

        using RenderFrame frame = Render(profile);

        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Black);
    }

    [Fact]
    public void BusyFolder_RendersEveryChild()
    {
        Profile profile = Harness.CreateProfile();
        BusyFolder busy = BusyFolder.Create(Harness, Keyboard, profile);

        using RenderFrame frame = Render(profile);

        foreach (ArtemisLed led in busy.Leds.Concat(busy.Outside.Leds))
            frame.LedColors[led].ShouldNotBe(SKColors.Black, $"{led.RgbLed.Id} should be lit");
        frame.LedColors.Where(c => !busy.Leds.Contains(c.Key) && !busy.Outside.Leds.Contains(c.Key)).ShouldAllBe(c => c.Value == SKColors.Black);
    }

    [Fact]
    public void SuspendedFolder_HidesAllOfItsChildren()
    {
        Profile profile = Harness.CreateProfile();
        BusyFolder busy = BusyFolder.Create(Harness, Keyboard, profile);
        busy.Folder.Suspended = true;

        using RenderFrame frame = Render(profile);

        foreach (ArtemisLed led in busy.Leds)
            frame.LedColors[led].ShouldBeColor(SKColors.Black, 2, led.RgbLed.Id.ToString());
        foreach (ArtemisLed led in busy.Outside.Leds)
            frame.LedColors[led].ShouldNotBe(SKColors.Black);
    }

    [Fact]
    public void SuspendedNestedFolder_OnlyHidesTheNestedChildren()
    {
        Profile profile = Harness.CreateProfile();
        BusyFolder busy = BusyFolder.Create(Harness, Keyboard, profile);
        busy.Nested.Suspended = true;

        using RenderFrame frame = Render(profile);

        foreach (ArtemisLed led in busy.Leds)
        {
            if (busy.NestedLeds.Contains(led))
                frame.LedColors[led].ShouldBeColor(SKColors.Black, 2, led.RgbLed.Id.ToString());
            else
                frame.LedColors[led].ShouldNotBe(SKColors.Black, led.RgbLed.Id.ToString());
        }
    }

    [Fact]
    public void SuspendingARenderingFolder_StopsRenderingItsChildren()
    {
        Profile profile = Harness.CreateProfile();
        BusyFolder busy = BusyFolder.Create(Harness, Keyboard, profile);
        using RenderFrame before = Harness.Render(profile, TimeSpan.FromSeconds(0.1));

        busy.Folder.Suspended = true;
        using RenderFrame after = Render(profile);

        busy.Folder.Enabled.ShouldBeFalse();
        foreach (ArtemisLed led in busy.Leds)
        {
            before.LedColors[led].ShouldNotBe(SKColors.Black, led.RgbLed.Id.ToString());
            after.LedColors[led].ShouldBeColor(SKColors.Black, 2, led.RgbLed.Id.ToString());
        }
    }

    [Fact]
    public void UnsuspendedFolder_StartsRenderingItsChildren()
    {
        Profile profile = Harness.CreateProfile();
        BusyFolder busy = BusyFolder.Create(Harness, Keyboard, profile);
        busy.Folder.Suspended = true;

        using RenderFrame before = Harness.Render(profile, TimeSpan.FromSeconds(0.1));
        busy.Folder.Suspended = false;
        using RenderFrame after = Render(profile);

        foreach (ArtemisLed led in busy.Leds)
        {
            before.LedColors[led].ShouldBeColor(SKColors.Black, 2, led.RgbLed.Id.ToString());
            after.LedColors[led].ShouldNotBe(SKColors.Black, led.RgbLed.Id.ToString());
        }
    }

    [Fact]
    public void RemovedLayer_StopsRendering()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, _) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);
        using (RenderFrame before = Harness.Render(profile, TimeSpan.FromSeconds(0.1)))
            before.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);

        profile.GetRootFolder().RemoveChild(layer);

        using RenderFrame after = Render(profile);
        after.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Black);
    }

    [Fact]
    public void ProfileWithoutFade_IsFullyVisibleOnFirstFrame()
    {
        Profile profile = Harness.CreateProfile();
        Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);

        using RenderFrame frame = Render(profile);

        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
    }

    [Fact]
    public void ProfileWithFade_FadesInOverOneSecond()
    {
        Profile profile = Harness.CreateProfile();
        profile.Configuration.FadeInAndOut = true;
        Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);

        using RenderTimeline timeline = RenderTimeline(profile, 0.5, 1, 1.5);

        // The fade uses a cubic ease-in-out, which is at exactly half at the halfway point
        timeline[0].GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(new SKColor(127, 0, 0));
        timeline[1].GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
        timeline[2].GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
    }
}
