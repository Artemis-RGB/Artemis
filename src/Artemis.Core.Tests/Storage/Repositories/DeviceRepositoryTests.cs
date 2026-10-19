using Artemis.Core.Tests.Infrastructure.Services;
using Artemis.Storage.Entities.Surface;
using Artemis.Storage.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Artemis.Core.Tests.Storage.Repositories;

public class DeviceRepositoryTests : IDisposable
{
    private readonly ServiceHarness _services = new();
    private readonly IDeviceRepository _repository;

    public DeviceRepositoryTests()
    {
        _repository = _services.Resolve<IDeviceRepository>();
    }

    [Fact]
    public void Add_ThenGet_ReturnsEverythingThatWasStored()
    {
        DeviceEntity device = CreateDevice("Keyboard");
        device.X = 10.5f;
        device.Y = 20;
        device.Rotation = 90;
        device.Scale = 2;
        device.ZIndex = 3;
        device.RedScale = 0.5f;
        device.GreenScale = 0.75f;
        device.BlueScale = 1;
        device.IsEnabled = true;
        device.PhysicalLayout = 1;
        device.LogicalLayout = "NA";
        device.LayoutType = "Default";
        device.LayoutParameter = "layout.xml";
        device.InputIdentifiers.Add(new DeviceInputIdentifierEntity {InputProvider = "Windows", Identifier = "HID#1234"});
        device.InputMappings.Add(new InputMappingEntity {OriginalLedId = 1, MappedLedId = 2});
        device.Categories.AddRange([1, 4]);

        _repository.Add(device);

        DeviceEntity stored = _repository.Get("Keyboard").ShouldNotBeNull();
        stored.ShouldNotBeSameAs(device);
        stored.DeviceProvider.ShouldBe("Artemis.Plugins.Devices.Test");
        (stored.X, stored.Y, stored.Rotation, stored.Scale, stored.ZIndex).ShouldBe((10.5f, 20f, 90f, 2f, 3));
        (stored.RedScale, stored.GreenScale, stored.BlueScale, stored.IsEnabled).ShouldBe((0.5f, 0.75f, 1f, true));
        (stored.PhysicalLayout, stored.LogicalLayout, stored.LayoutType, stored.LayoutParameter).ShouldBe((1, "NA", "Default", "layout.xml"));
        stored.InputIdentifiers.ShouldHaveSingleItem().Identifier.ShouldBe("HID#1234");
        stored.InputIdentifiers[0].InputProvider.ShouldBe("Windows");
        stored.InputMappings.ShouldHaveSingleItem().MappedLedId.ShouldBe(2);
        stored.Categories.ShouldBe([1, 4]);
    }

    [Fact]
    public void Get_UnknownDevice_ReturnsNull()
    {
        _repository.Add(CreateDevice("Keyboard"));

        _repository.Get("Mouse").ShouldBeNull();
    }

    [Fact]
    public void GetAll_ReturnsAllDevices()
    {
        _repository.Add(CreateDevice("Keyboard"));
        _repository.Add(CreateDevice("Mouse"));

        _repository.GetAll().Select(d => d.Id).Order().ShouldBe(["Keyboard", "Mouse"]);
    }

    [Fact]
    public void Save_StoredDevice_StoresTheChanges()
    {
        DeviceEntity device = CreateDevice("Keyboard");
        _repository.Add(device);

        device.X = 100;
        device.InputMappings.Add(new InputMappingEntity {OriginalLedId = 5, MappedLedId = 6});
        _repository.Save(device);

        DeviceEntity stored = _repository.Get("Keyboard")!;
        stored.X.ShouldBe(100);
        stored.InputMappings.ShouldHaveSingleItem().OriginalLedId.ShouldBe(5);
    }

    [Fact]
    public void Save_DeviceThatWasNeverAdded_Throws()
    {
        // The ID of a device isn't generated, so saving takes it to be an existing device and finds nothing to update.
        // The device service adds new devices before it saves them.
        Should.Throw<DbUpdateConcurrencyException>(() => _repository.Save(CreateDevice("Keyboard")));
    }

    [Fact]
    public void SaveRange_StoresAllDevices()
    {
        DeviceEntity keyboard = CreateDevice("Keyboard");
        DeviceEntity mouse = CreateDevice("Mouse");
        _repository.Add(keyboard);
        _repository.Add(mouse);

        keyboard.ZIndex = 2;
        mouse.ZIndex = 1;
        _repository.SaveRange([keyboard, mouse]);

        _repository.GetAll().OrderBy(d => d.ZIndex).Select(d => d.Id).ShouldBe(["Mouse", "Keyboard"]);
    }

    [Fact]
    public void Remove_DeletesTheDevice()
    {
        DeviceEntity device = CreateDevice("Keyboard");
        _repository.Add(device);
        _repository.Add(CreateDevice("Mouse"));

        _repository.Remove(device);

        _repository.GetAll().ShouldHaveSingleItem().Id.ShouldBe("Mouse");
    }

    [Fact]
    public void Get_DeviceWithNullInputLists_ReturnsEmptyLists()
    {
        _repository.Add(CreateDevice("Keyboard"));
        using (var context = _services.CreateDbContext())
            context.Database.ExecuteSqlRaw("UPDATE Devices SET InputIdentifiers = 'null', InputMappings = 'null'");

        DeviceEntity stored = _repository.Get("Keyboard")!;

        stored.InputIdentifiers.ShouldBeEmpty();
        stored.InputMappings.ShouldBeEmpty();
    }

    private static DeviceEntity CreateDevice(string id)
    {
        return new DeviceEntity {Id = id, DeviceProvider = "Artemis.Plugins.Devices.Test", Scale = 1};
    }

    public void Dispose()
    {
        _services.Dispose();
    }
}
