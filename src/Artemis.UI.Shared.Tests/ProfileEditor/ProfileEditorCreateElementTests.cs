using Artemis.Core.Tests.Infrastructure.Plugins.LayerBrushes;
using Artemis.UI.Shared.Tests.Infrastructure;
using RGB.NET.Core;

namespace Artemis.UI.Shared.Tests.ProfileEditor;

/// <summary>
///     Adding new folders and layers through the editor
/// </summary>
public class ProfileEditorCreateElementTests : ProfileEditorTestBase
{
    [Fact]
    public async Task CreateAndAddFolder_InAFolder_AddsItAtTheTop()
    {
        Layer existing = AddLayer("Existing", null, LedId.Keyboard_A);
        await StartEditing();

        Folder folder = Service.CreateAndAddFolder(Root);

        folder.Name.ShouldBe("New folder");
        Root.Children.ShouldBe([folder, existing]);
    }

    [Fact]
    public async Task CreateAndAddFolder_GivesEachFolderAUniqueName()
    {
        await StartEditing();

        Service.CreateAndAddFolder(Root);
        Folder second = Service.CreateAndAddFolder(Root);

        second.Name.ShouldBe("New folder (2)");
    }

    [Fact]
    public async Task CreateAndAddFolder_OnALayer_AddsItBelowTheLayer()
    {
        Layer first = AddLayer("First", null, LedId.Keyboard_A);
        Layer second = AddLayer("Second", null, LedId.Keyboard_B);
        await StartEditing();

        Folder folder = Service.CreateAndAddFolder(first);

        // The layer's order is 1-based while AddChild takes a 0-based index, so the folder lands right below the layer
        Root.Children.ShouldBe([first, folder, second]);
    }

    [Fact]
    public async Task CreateAndAddFolder_CanBeUndone()
    {
        await StartEditing();
        string before = Snapshot();

        Folder folder = Service.CreateAndAddFolder(Root);
        Undo();

        Root.Children.ShouldNotContain(folder);
        Snapshot().ShouldBe(before);
    }

    [Fact]
    public async Task CreateAndAddLayer_InAFolder_CoversEveryEnabledLedAndGetsTheDefaultBrush()
    {
        LayerBrushService.DefaultBrush = BrushDescriptor<SolidColorTestBrush>();
        Folder folder = Harness.AddFolder(Root, "Folder");
        Layer existing = AddLayer("Existing", folder, LedId.Keyboard_A);
        await StartEditing();

        Layer layer = Service.CreateAndAddLayer(folder);

        layer.Name.ShouldBe("New layer");
        folder.Children.ShouldBe([layer, existing]);
        layer.Leds.ShouldBe(Keyboard.Leds, ignoreOrder: true);
        layer.LayerBrush.ShouldBeOfType<SolidColorTestBrush>();
        layer.Enabled.ShouldBeTrue();
    }

    [Fact]
    public async Task CreateAndAddLayer_WithoutDefaultBrush_HasNoBrush()
    {
        await StartEditing();

        Layer layer = Service.CreateAndAddLayer(Root);

        layer.LayerBrush.ShouldBeNull();
    }

    [Fact]
    public async Task CreateAndAddLayer_OnALayer_AddsItBelowTheLayer()
    {
        Layer first = AddLayer("First", null, LedId.Keyboard_A);
        Layer second = AddLayer("Second", null, LedId.Keyboard_B);
        await StartEditing();

        Layer layer = Service.CreateAndAddLayer(first);

        Root.Children.ShouldBe([first, layer, second]);
    }

    [Fact]
    public async Task CreateAndAddLayer_OnALayer_PicksANameNotInUseNextToIt()
    {
        Layer existing = AddLayer("New layer", null, LedId.Keyboard_A);
        await StartEditing();

        Layer layer = Service.CreateAndAddLayer(existing);

        layer.Parent.ShouldBe(Root);
        layer.Name.ShouldNotBe("New layer");
        Root.Children.Count(c => c.Name == "New layer").ShouldBe(1);
    }

    [Fact]
    public async Task CreateAndAddLayer_CanBeUndone()
    {
        await StartEditing();
        string before = Snapshot();

        Layer layer = Service.CreateAndAddLayer(Root);
        Undo();

        Root.Children.ShouldNotContain(layer);
        Snapshot().ShouldBe(before);
    }
}
