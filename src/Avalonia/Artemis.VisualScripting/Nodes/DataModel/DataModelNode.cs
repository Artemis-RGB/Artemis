using Artemis.Core;
using Artemis.Storage.Entities.Profile;
using Artemis.VisualScripting.Nodes.DataModel.CustomViewModels;
using Avalonia.Threading;

namespace Artemis.VisualScripting.Nodes.DataModel;

[Node("Data Model-Value", "Outputs a selectable data model value.", "Data Model")]
public class DataModelNode : Node<DataModelPathEntity, DataModelNodeCustomViewModel>, IDisposable
{
    private DataModelPath? _dataModelPath;

    public DataModelNode() : base("Data Model", "Outputs a selectable data model value")
    {
        Output = CreateOutputPin(typeof(object));
    }

    public INodeScript? Script { get; private set; }
    public OutputPin Output { get; }

    public DataModelPath? DataModelPath
    {
        get => _dataModelPath;
        set => SetAndNotify(ref _dataModelPath, value);
    }

    public override void Initialize(INodeScript script)
    {
        Script = script;

        if (Storage == null)
            return;

        DataModelPath = new DataModelPath(Storage);
        DataModelPath.PathValidated += DataModelPathOnPathValidated;

        UpdateOutputPin();
    }

    public override void Evaluate()
    {
        if (DataModelPath == null || !DataModelPath.IsValid)
            return;

        object? pathValue = DataModelPath.GetValue();
        if (pathValue == null)
        {
            if (!Output.Type.IsValueType)
                Output.Value = null;
        }
        else
        {
            Output.Value = Output.Type == typeof(Numeric) ? new Numeric(pathValue) : pathValue;
        }
    }

    public void UpdateOutputPin()
    {
        Type? type = DataModelPath?.GetPropertyType();
        if (Numeric.IsTypeCompatible(type))
            type = typeof(Numeric);
        type ??= typeof(object);

        if (Output.Type != type)
            Output.ChangeType(type);
    }

    private void DataModelPathOnPathValidated(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(UpdateOutputPin);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        DataModelPath?.Dispose();
    }
}