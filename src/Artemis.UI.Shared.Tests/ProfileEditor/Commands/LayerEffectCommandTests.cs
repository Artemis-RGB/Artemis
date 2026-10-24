using Artemis.Core.LayerEffects;
using Artemis.Core.Tests.Infrastructure.Plugins.LayerEffects;
using Artemis.UI.Shared.Services.ProfileEditor.Commands;
using Artemis.UI.Shared.Tests.Infrastructure;
using RGB.NET.Core;

namespace Artemis.UI.Shared.Tests.ProfileEditor.Commands;

/// <summary>
///     Adding, removing and renaming effects
/// </summary>
public class LayerEffectCommandTests : ProfileEditorTestBase
{
    private readonly Layer _layer;

    public LayerEffectCommandTests()
    {
        _layer = AddLayer("Layer", null, LedId.Keyboard_A);
        _layer.Enable();
    }

    [Fact]
    public void AddLayerEffect_AddsAndEnablesTheEffect()
    {
        OpacityTestEffect effect = NewEffect<OpacityTestEffect>(_layer);
        AddLayerEffect command = new(_layer, effect);

        command.Execute();

        command.DisplayName.ShouldBe("Add layer effect");
        _layer.LayerEffects.ShouldBe([effect]);
        effect.ProfileElement.ShouldBe(_layer);
        effect.Enabled.ShouldBeTrue();
    }

    [Fact]
    public void AddLayerEffect_NewEffectsGoOnTop()
    {
        TintTestEffect existing = Harness.AddEffect<TintTestEffect>(_layer);
        OpacityTestEffect effect = NewEffect<OpacityTestEffect>(_layer);

        new AddLayerEffect(_layer, effect).Execute();

        // A new effect has order 0, so it sorts before the effects already there
        _layer.LayerEffects.ShouldBe([effect, existing]);
    }

    [Fact]
    public void AddLayerEffect_Undo_RestoresTheProfile()
    {
        Harness.AddEffect<TintTestEffect>(_layer);
        string before = Snapshot();
        OpacityTestEffect effect = NewEffect<OpacityTestEffect>(_layer);
        AddLayerEffect command = new(_layer, effect);

        command.Execute();
        command.Undo();

        _layer.LayerEffects.ShouldNotContain(effect);
        effect.Enabled.ShouldBeFalse();
        Snapshot().ShouldBe(before);
    }

    [Fact]
    public void AddLayerEffect_Dispose_DisposesTheEffectOnlyWhenNotAdded()
    {
        OpacityTestEffect added = NewEffect<OpacityTestEffect>(_layer);
        AddLayerEffect addedCommand = new(_layer, added);
        addedCommand.Execute();
        TintTestEffect undone = NewEffect<TintTestEffect>(_layer);
        AddLayerEffect undoneCommand = new(_layer, undone);
        undoneCommand.Execute();
        undoneCommand.Undo();
        addedCommand.Dispose();
        undoneCommand.Dispose();

        IsDisposed(added.Properties).ShouldBeFalse();
        IsDisposed(undone.Properties).ShouldBeTrue();
    }

    [Fact]
    public void RemoveLayerEffect_RemovesAndDisablesTheEffect()
    {
        OpacityTestEffect effect = Harness.AddEffect<OpacityTestEffect>(_layer);
        RemoveLayerEffect command = new(effect);

        command.Execute();

        command.DisplayName.ShouldBe("Remove layer effect");
        _layer.LayerEffects.ShouldBeEmpty();
        effect.Enabled.ShouldBeFalse();
    }

    [Fact]
    public void RemoveLayerEffect_Undo_OfTheBottomEffect_RestoresTheProfile()
    {
        Harness.AddEffect<TintTestEffect>(_layer);
        Harness.AddEffect<OpacityTestEffect>(_layer);
        BaseLayerEffect[] effects = _layer.LayerEffects.ToArray();
        string before = Snapshot();
        RemoveLayerEffect command = new(effects[1]);

        command.Execute();
        command.Undo();

        _layer.LayerEffects.ShouldBe(effects);
        effects[1].Enabled.ShouldBeTrue();
        Snapshot().ShouldBe(before);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void RemoveLayerEffect_Undo_OfAnEffectAboveAnother_PutsItBackInItsPlace(int index)
    {
        Harness.AddEffect<TintTestEffect>(_layer);
        Harness.AddEffect<OpacityTestEffect>(_layer);
        Harness.AddEffect<ChannelSwapTestEffect>(_layer);
        List<BaseLayerEffect> effects = _layer.LayerEffects.ToList();
        BaseLayerEffect removed = effects[index];
        string before = Snapshot();
        RemoveLayerEffect command = new(removed);

        command.Execute();
        command.Undo();

        // Effects apply in order, so this keeps the layer looking the same
        _layer.LayerEffects.ShouldBe(effects);
        _layer.LayerEffects.Select(e => e.Order).ShouldBe([1, 2, 3]);
        Snapshot().ShouldBe(before);
    }

    [Fact]
    public void RemoveLayerEffect_Dispose_DisposesTheEffectOnlyWhenRemoved()
    {
        OpacityTestEffect removed = Harness.AddEffect<OpacityTestEffect>(_layer);
        RemoveLayerEffect removedCommand = new(removed);
        removedCommand.Execute();
        TintTestEffect undone = Harness.AddEffect<TintTestEffect>(_layer);
        RemoveLayerEffect undoneCommand = new(undone);
        undoneCommand.Execute();
        undoneCommand.Undo();
        removedCommand.Dispose();
        undoneCommand.Dispose();

        IsDisposed(removed.Properties).ShouldBeTrue();
        IsDisposed(undone.Properties).ShouldBeFalse();
    }

    [Fact]
    public void RemoveLayerEffect_FromAFolder_CanBeUndone()
    {
        Folder folder = Harness.AddFolder(Root, "Folder");
        folder.Enable();
        TintTestEffect effect = Harness.AddEffect<TintTestEffect>(folder);
        string before = Snapshot();
        RemoveLayerEffect command = new(effect);

        command.Execute();
        folder.LayerEffects.ShouldBeEmpty();

        command.Undo();
        folder.LayerEffects.ShouldBe([effect]);
        Snapshot().ShouldBe(before);
    }

    [Fact]
    public void RenameLayerEffect_RenamesAndMarksTheEffectAsRenamed()
    {
        BaseLayerEffect effect = Harness.AddEffect<OpacityTestEffect>(_layer);
        string originalName = effect.Name;
        string before = Snapshot();
        RenameLayerEffect command = new(effect, "Renamed");

        command.Execute();
        command.DisplayName.ShouldBe("Rename layer effect");
        effect.Name.ShouldBe("Renamed");
        effect.HasBeenRenamed.ShouldBeTrue();

        command.Undo();
        effect.Name.ShouldBe(originalName);
        effect.HasBeenRenamed.ShouldBeFalse();
        Snapshot().ShouldBe(before);
    }

    [Fact]
    public void RenameLayerEffect_Undo_KeepsAnEarlierRename()
    {
        BaseLayerEffect effect = Harness.AddEffect<OpacityTestEffect>(_layer);
        new RenameLayerEffect(effect, "First").Execute();
        RenameLayerEffect command = new(effect, "Second");

        command.Execute();
        command.Undo();

        effect.Name.ShouldBe("First");
        effect.HasBeenRenamed.ShouldBeTrue();
    }
}
