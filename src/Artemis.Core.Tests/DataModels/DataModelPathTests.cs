using Artemis.Core.Tests.Infrastructure.Plugins;
using Artemis.Core.Tests.Infrastructure.Plugins.Modules;

namespace Artemis.Core.Tests.DataModels;

public class DataModelPathTests : IDisposable
{
    private readonly TestPlugin _plugin = new();
    private readonly TestModule _module;

    public DataModelPathTests()
    {
        _module = _plugin.AddFeature(new TestModule());
    }

    [Fact]
    public void Path_ReadsTheCurrentValue()
    {
        _module.DataModel.Integer = 5;
        using DataModelPath path = new(_module.DataModel, "Integer");

        path.IsValid.ShouldBeTrue();
        path.GetValue().ShouldBe(5);

        _module.DataModel.Integer = 6;
        path.GetValue().ShouldBe(6);
    }

    [Fact]
    public void Path_IsRestoredFromItsEntityThroughTheDataModelStore()
    {
        _module.DataModel.Child.Integer = 7;
        using DataModelPath path = new(_module.DataModel, "Child.Integer");
        path.Save();

        using DataModelPath restored = new(path.Entity);

        restored.IsValid.ShouldBeTrue();
        restored.Target.ShouldBe(_module.DataModel);
        restored.GetValue().ShouldBe(7);
    }

    public void Dispose()
    {
        _plugin.Dispose();
    }
}
