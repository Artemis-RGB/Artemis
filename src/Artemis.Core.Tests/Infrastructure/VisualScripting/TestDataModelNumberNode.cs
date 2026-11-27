using Artemis.Storage.Entities.Profile;

namespace Artemis.Core.Tests.Infrastructure.VisualScripting;

/// <summary>
///     A node that outputs the number found at a data model path, like the data model node in the Artemis.Plugins
///     repository. The path is stored, so it survives saving and loading a script when the node is created through
///     <see cref="Plugins.Nodes.DataModelTestNodeProvider" />.
/// </summary>
[Node("Test data model number", "Outputs the number at a data model path", "Tests", OutputType = typeof(Numeric))]
public class TestDataModelNumberNode : Node<DataModelPathEntity>, IDisposable
{
    private DataModelPath? _path;

    public TestDataModelNumberNode()
    {
        Output = CreateOutputPin<Numeric>();
        StorageModified += (_, _) => ClearPath();
    }

    public OutputPin<Numeric> Output { get; }

    /// <summary>
    ///     Gets the path the node reads, created from storage the first time the node is evaluated
    /// </summary>
    public DataModelPath? Path => _path;

    /// <summary>
    ///     Makes the node read the value at the provided path
    /// </summary>
    public void UsePath(DataModelPath path)
    {
        path.Save();
        Storage = new DataModelPathEntity {Path = path.Entity.Path, DataModelId = path.Entity.DataModelId, Type = path.Entity.Type};
    }

    public override void Evaluate()
    {
        if (_path == null && Storage != null)
            _path = new DataModelPath(Storage);

        Output.Value = new Numeric(_path?.GetValue());
    }

    public override IEnumerable<PluginFeature> GetFeatureDependencies()
    {
        return base.GetFeatureDependencies().Concat(_path?.GetFeatureDependencies() ?? []);
    }

    public void Dispose()
    {
        ClearPath();
    }

    private void ClearPath()
    {
        _path?.Dispose();
        _path = null;
    }
}
