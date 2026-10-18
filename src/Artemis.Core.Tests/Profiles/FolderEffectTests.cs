using Artemis.Core.LayerEffects;
using Artemis.Core.Tests.Infrastructure;
using Artemis.Core.Tests.Infrastructure.Plugins.LayerBrushes;
using Artemis.Core.Tests.Infrastructure.Plugins.LayerEffects;
using Artemis.Core.Tests.Infrastructure.Rendering;
using Artemis.Core.Tests.Infrastructure.VisualScripting;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core.Tests.Profiles;

/// <summary>
///     Most tests render a busy folder with and without an effect and check every LED: those covered by the folder must
///     change exactly as the effect dictates, all others must stay the same.
/// </summary>
public class FolderEffectTests : RenderTestBase
{
    private static readonly SKColor TintColor = new(255, 128, 64);

    [Fact]
    public void PreProcessColorFilter_AppliesToEveryChildAndNothingElse()
    {
        Profile profile = Harness.CreateProfile();
        BusyFolder busy = BusyFolder.Create(Harness, Keyboard, profile);
        ChannelSwapTestEffect swap = Harness.AddEffect<ChannelSwapTestEffect>(busy.Folder);

        (RenderFrame before, RenderFrame after) = RenderWithAndWithout(profile, swap);

        after.ShouldOnlyDifferFrom(before, busy.Leds, Swap);
    }

    [Fact]
    public void PreProcessOpacity_FadesEveryChildAndNothingElse()
    {
        Profile profile = Harness.CreateProfile();
        BusyFolder busy = BusyFolder.Create(Harness, Keyboard, profile);
        OpacityTestEffect opacity = Harness.AddEffect<OpacityTestEffect>(busy.Folder);

        (RenderFrame before, RenderFrame after) = RenderWithAndWithout(profile, opacity);

        after.ShouldOnlyDifferFrom(before, busy.Leds, c => c.AtOpacity(50));
    }

    [Fact]
    public void PostProcessTint_AppliesToEveryChildAndNothingElse()
    {
        Profile profile = Harness.CreateProfile();
        BusyFolder busy = BusyFolder.Create(Harness, Keyboard, profile);
        TintTestEffect tint = Harness.AddEffect<TintTestEffect>(busy.Folder);
        tint.Properties.Color.SetCurrentValue(TintColor);

        (RenderFrame before, RenderFrame after) = RenderWithAndWithout(profile, tint);

        after.ShouldOnlyDifferFrom(before, busy.Leds, c => Multiply(c, TintColor));
    }

    [Fact]
    public void NestedFolderEffect_OnlyAppliesToTheNestedChildren()
    {
        Profile profile = Harness.CreateProfile();
        BusyFolder busy = BusyFolder.Create(Harness, Keyboard, profile);
        ChannelSwapTestEffect swap = Harness.AddEffect<ChannelSwapTestEffect>(busy.Nested);

        (RenderFrame before, RenderFrame after) = RenderWithAndWithout(profile, swap);

        after.ShouldOnlyDifferFrom(before, busy.NestedLeds, Swap);
    }

    [Fact]
    public void OuterAndNestedFolderEffects_Compose()
    {
        Profile profile = Harness.CreateProfile();
        BusyFolder busy = BusyFolder.Create(Harness, Keyboard, profile);
        ChannelSwapTestEffect swap = Harness.AddEffect<ChannelSwapTestEffect>(busy.Folder);
        OpacityTestEffect opacity = Harness.AddEffect<OpacityTestEffect>(busy.Nested);

        (RenderFrame before, RenderFrame after) = RenderWithAndWithout(profile, swap, opacity);

        // The nested children get both, the rest of the folder only gets the swap
        ArtemisLed[] outerOnly = busy.Leds.Except(busy.NestedLeds).ToArray();
        foreach (ArtemisLed led in busy.NestedLeds)
            after.LedColors[led].ShouldBeColor(Swap(before.LedColors[led]).AtOpacity(50), 3);
        after.ShouldOnlyDifferFrom(before, outerOnly, Swap, busy.NestedLeds);
    }

    [Fact]
    public void RootFolderEffect_AppliesToTheWholeProfile()
    {
        Profile profile = Harness.CreateProfile();
        BusyFolder busy = BusyFolder.Create(Harness, Keyboard, profile);
        ChannelSwapTestEffect swap = Harness.AddEffect<ChannelSwapTestEffect>(profile.GetRootFolder());

        (RenderFrame before, RenderFrame after) = RenderWithAndWithout(profile, swap);

        after.ShouldOnlyDifferFrom(before, busy.Leds.Concat(busy.Outside.Leds).ToArray(), Swap);
    }

    [Fact]
    public void FolderOpacityEffect_FadesOverlappingChildrenAsAWhole()
    {
        Profile profile = Harness.CreateProfile();
        Folder folder = Harness.AddFolder(profile.GetRootFolder());
        (_, SolidColorTestBrush top) = Harness.AddLayer<SolidColorTestBrush>(folder, [Led(LedId.Keyboard_A)], "Top");
        (_, SolidColorTestBrush bottom) = Harness.AddLayer<SolidColorTestBrush>(folder, [Led(LedId.Keyboard_A)], "Bottom");
        top.Properties.Color.SetCurrentValue(SKColors.Lime);
        bottom.Properties.Color.SetCurrentValue(SKColors.Red);
        Harness.AddEffect<OpacityTestEffect>(folder);

        using RenderFrame frame = Render(profile);

        // The children are flattened before the folder is faded, the bottom layer does not shine through
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Lime.AtOpacity(50));
    }

    [Fact]
    public void FolderOpacityEffect_LetsElementsBelowTheFolderShowThrough()
    {
        Profile profile = Harness.CreateProfile();
        Folder folder = Harness.AddFolder(profile.GetRootFolder());
        (_, SolidColorTestBrush inFolder) = Harness.AddLayer<SolidColorTestBrush>(folder, [Led(LedId.Keyboard_A)], "In folder");
        (_, SolidColorTestBrush below) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)], "Below folder");
        inFolder.Properties.Color.SetCurrentValue(SKColors.Lime);
        below.Properties.Color.SetCurrentValue(SKColors.Red);
        Harness.AddEffect<OpacityTestEffect>(folder);

        using RenderFrame frame = Render(profile);

        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Lime.AtOpacity(50, over: SKColors.Red));
    }

    [Fact]
    public void FolderOpacityEffectAtZero_SkipsRenderingTheChildren()
    {
        Profile profile = Harness.CreateProfile();
        BusyFolder busy = BusyFolder.Create(Harness, Keyboard, profile);
        OpacityTestEffect opacity = Harness.AddEffect<OpacityTestEffect>(busy.Folder);
        opacity.Properties.Opacity.SetCurrentValue(0);

        (RenderFrame before, RenderFrame after) = RenderWithAndWithout(profile, opacity);

        after.ShouldOnlyDifferFrom(before, busy.Leds, _ => SKColors.Black);
    }

    [Fact]
    public void FolderPostProcessEffect_LeavesTheGapsBetweenChildrenUntouched()
    {
        Profile profile = Harness.CreateProfile();
        Folder folder = Harness.AddFolder(profile.GetRootFolder());
        Harness.AddLayer<SolidColorTestBrush>(folder, [Led(LedId.Keyboard_Q)], "Q");
        Harness.AddLayer<SolidColorTestBrush>(folder, [Led(LedId.Keyboard_P)], "P");
        (_, SolidColorTestBrush below) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), TopLetterRow, "Below");
        below.Properties.Color.SetCurrentValue(SKColors.White);
        Harness.AddEffect<TintTestEffect>(folder);

        using RenderFrame frame = Render(profile);

        // The tint draws over the whole folder bounds, but inside the folder's own layer which is transparent in between
        frame.GetColor(Keyboard, LedId.Keyboard_Q).ShouldBeColor(SKColors.Red);
        frame.GetColor(Keyboard, LedId.Keyboard_T).ShouldBeColor(SKColors.White);
        frame.GetColor(Keyboard, LedId.Keyboard_P).ShouldBeColor(SKColors.Red);
    }

    [Fact]
    public void FolderConditionNotMet_HidesEveryChildIncludingTheEffect()
    {
        Profile profile = Harness.CreateProfile();
        BusyFolder busy = BusyFolder.Create(Harness, Keyboard, profile);
        Harness.AddEffect<ChannelSwapTestEffect>(busy.Folder);
        TestBooleanNode condition = TestConditions.UseStaticCondition(busy.Folder, true, stopMode: StaticStopMode.SkipToEnd);

        using RenderFrame met = Harness.Render(profile, TimeSpan.FromSeconds(0.1));
        SaveOutput(met, TestName + "_met");
        condition.Value = false;
        using RenderFrame notMet = Harness.Render(profile, TimeSpan.FromSeconds(0.1));
        SaveOutput(notMet, TestName + "_not_met");

        notMet.ShouldOnlyDifferFrom(met, busy.Leds, _ => SKColors.Black);
    }

    /// <summary>
    ///     Renders a frame with the effects disabled and a frame with them enabled. The busy folder has no animation so the
    ///     frames only differ by the effects.
    /// </summary>
    private (RenderFrame Before, RenderFrame After) RenderWithAndWithout(Profile profile, params BaseLayerEffect[] effects)
    {
        foreach (BaseLayerEffect effect in effects)
            effect.BaseProperties!.IsEnabled.SetCurrentValue(false);
        RenderFrame before = Track(Harness.Render(profile, TimeSpan.FromSeconds(0.1)));
        SaveOutput(before, TestName + "_without");

        foreach (BaseLayerEffect effect in effects)
            effect.BaseProperties!.IsEnabled.SetCurrentValue(true);
        RenderFrame after = Track(Harness.Render(profile, TimeSpan.FromSeconds(0.1)));
        SaveOutput(after, TestName + "_with");

        return (before, after);
    }

    private static SKColor Swap(SKColor color)
    {
        return new SKColor(color.Blue, color.Green, color.Red, color.Alpha);
    }

    private static SKColor Multiply(SKColor color, SKColor with)
    {
        return new SKColor((byte) (color.Red * with.Red / 255), (byte) (color.Green * with.Green / 255), (byte) (color.Blue * with.Blue / 255));
    }
}
