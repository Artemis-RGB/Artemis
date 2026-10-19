using Artemis.Core.LayerBrushes;
using Artemis.Core.Tests.Infrastructure.Plugins.LayerBrushes;
using Artemis.UI.Shared.Services.ProfileEditor.Commands;
using Artemis.UI.Shared.Tests.Infrastructure;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.UI.Shared.Tests.ProfileEditor.Commands;

/// <summary>
///     Changing the brush of a layer and the LEDs it covers
/// </summary>
public class LayerBrushAndLedCommandTests : ProfileEditorTestBase
{
    private readonly Layer _layer;
    private readonly SolidColorTestBrush _brush;

    public LayerBrushAndLedCommandTests()
    {
        (_layer, _brush) = Harness.AddLayer<SolidColorTestBrush>(Root, Leds(LedId.Keyboard_A, LedId.Keyboard_S), "Layer");
        _layer.Enable();
    }

    [Fact]
    public void ChangeLayerBrush_GivesTheLayerANewBrush()
    {
        ChangeLayerBrush command = new(_layer, BrushDescriptor<GradientTestBrush>());

        command.Execute();

        command.DisplayName.ShouldBe("Change layer brush");
        GradientTestBrush newBrush = _layer.LayerBrush.ShouldBeOfType<GradientTestBrush>();
        newBrush.Enabled.ShouldBeTrue();
        _layer.General.BrushReference.CurrentValue!.BrushType.ShouldBe(nameof(GradientTestBrush));
        // The old brush is only disabled, so undo can put it back
        _brush.Enabled.ShouldBeFalse();
        IsDisposed(_brush.Properties).ShouldBeFalse();
    }

    [Fact]
    public void ChangeLayerBrush_Undo_PutsTheSameBrushBackWithItsSettings()
    {
        _brush.Properties.Color.SetCurrentValue(SKColors.Lime);
        string before = Snapshot();
        ChangeLayerBrush command = new(_layer, BrushDescriptor<GradientTestBrush>());

        command.Execute();
        command.Undo();

        _layer.LayerBrush.ShouldBe(_brush);
        _brush.Enabled.ShouldBeTrue();
        _brush.Properties.Color.BaseValue.ShouldBe(SKColors.Lime);
        Snapshot().ShouldBe(before);
    }

    [Fact]
    public void ChangeLayerBrush_Redo_ReusesTheSameNewBrush()
    {
        ChangeLayerBrush command = new(_layer, BrushDescriptor<GradientTestBrush>());
        command.Execute();
        BaseLayerBrush newBrush = _layer.LayerBrush!;

        command.Undo();
        command.Execute();

        _layer.LayerBrush.ShouldBe(newBrush);
    }

    [Fact]
    public void ChangeLayerBrush_Undo_OnALayerWithoutBrush_KeepsTheNewBrush()
    {
        Layer layer = new(Root, "Layer without brush");
        Root.AddChild(layer);
        ChangeLayerBrush command = new(layer, BrushDescriptor<GradientTestBrush>());

        command.Execute();
        command.Undo();

        // Pinned as-is: with no brush to go back to, undo leaves the new brush (and the brush reference) in place
        layer.LayerBrush.ShouldBeOfType<GradientTestBrush>();
    }

    [Fact]
    public void ChangeLayerBrush_Dispose_DisposesTheBrushNoLongerInUse()
    {
        ChangeLayerBrush executed = new(_layer, BrushDescriptor<GradientTestBrush>());
        executed.Execute();
        GradientTestBrush newBrush = (GradientTestBrush) _layer.LayerBrush!;

        executed.Dispose();

        IsDisposed(_brush.Properties).ShouldBeTrue();
        IsDisposed(newBrush.Properties).ShouldBeFalse();
    }

    [Fact]
    public void ChangeLayerBrush_Dispose_AfterUndo_DisposesTheNewBrush()
    {
        ChangeLayerBrush undone = new(_layer, BrushDescriptor<GradientTestBrush>());
        undone.Execute();
        GradientTestBrush newBrush = (GradientTestBrush) _layer.LayerBrush!;
        undone.Undo();

        undone.Dispose();

        IsDisposed(_brush.Properties).ShouldBeFalse();
        IsDisposed(newBrush.Properties).ShouldBeTrue();
    }

    [Fact]
    public void ChangeLayerLeds_ReplacesTheLeds()
    {
        ChangeLayerLeds command = new(_layer, [Led(LedId.Keyboard_Q), Led(LedId.Keyboard_W)]);

        command.Execute();

        command.DisplayName.ShouldBe("Change layer LEDs");
        _layer.Leds.ShouldBe(Leds(LedId.Keyboard_Q, LedId.Keyboard_W));
    }

    [Fact]
    public void ChangeLayerLeds_Undo_RestoresTheOriginalLedsInOrder()
    {
        string before = Snapshot();
        ChangeLayerLeds command = new(_layer, [Led(LedId.Keyboard_Q)]);

        command.Execute();
        command.Undo();

        _layer.Leds.ShouldBe(Leds(LedId.Keyboard_A, LedId.Keyboard_S));
        Snapshot().ShouldBe(before);
    }

    [Fact]
    public void ChangeLayerLeds_ToNone_AndBack()
    {
        string before = Snapshot();
        ChangeLayerLeds command = new(_layer, []);

        command.Execute();
        _layer.Leds.ShouldBeEmpty();

        command.Undo();
        Snapshot().ShouldBe(before);
    }

    [Fact]
    public void ChangeLayerLeds_KeepsItsOwnCopyOfTheOriginalLeds()
    {
        ChangeLayerLeds command = new(_layer, [Led(LedId.Keyboard_Q)]);
        // Changes made after the command was created, for example by an earlier command in a scope, are not undone
        _layer.AddLed(Led(LedId.Keyboard_D));

        command.Execute();
        command.Undo();

        _layer.Leds.ShouldBe(Leds(LedId.Keyboard_A, LedId.Keyboard_S));
    }

    [Fact]
    public void ApplyAdaptionHints_ReplacesTheLedsWithTheOnesTheHintsPick()
    {
        _layer.Adapter.Add(new SingleLedAdaptionHint {LedId = LedId.Keyboard_Z});
        _layer.Adapter.Add(new SingleLedAdaptionHint {LedId = LedId.Keyboard_X});
        ApplyAdaptionHints command = new(_layer, [Keyboard]);

        command.Execute();

        command.DisplayName.ShouldBe("Apply adaption hints");
        _layer.Leds.ShouldBe(Leds(LedId.Keyboard_Z, LedId.Keyboard_X));
    }

    [Fact]
    public void ApplyAdaptionHints_Undo_RestoresTheOriginalLeds()
    {
        _layer.Adapter.Add(new SingleLedAdaptionHint {LedId = LedId.Keyboard_Z});
        string before = Snapshot();
        ApplyAdaptionHints command = new(_layer, [Keyboard]);

        command.Execute();
        command.Undo();

        _layer.Leds.ShouldBe(Leds(LedId.Keyboard_A, LedId.Keyboard_S));
        Snapshot().ShouldBe(before);
    }

    [Fact]
    public void ApplyAdaptionHints_WithoutDevices_LeavesTheLayerWithoutLeds()
    {
        _layer.Adapter.Add(new SingleLedAdaptionHint {LedId = LedId.Keyboard_Z});

        new ApplyAdaptionHints(_layer, []).Execute();

        _layer.Leds.ShouldBeEmpty();
    }
}
