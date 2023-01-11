using Artemis.Core;
using Artemis.VisualScripting.Nodes.External.Screens;

namespace Artemis.VisualScripting.Nodes.External;

// [Node("Layer/Folder Property", "Outputs the property of a selected layer or folder", "External")]
public class LayerPropertyNode : Node<LayerPropertyNodeEntity, LayerPropertyNodeCustomViewModel>
{
    private readonly object _layerPropertyLock = new();
    private readonly List<OutputPin> _pinBucket = new();

    public LayerPropertyNode() : base("Layer/Folder Property", "Outputs the property of a selected layer or folder")
    {
    }

    public INodeScript? Script { get; private set; }
    public RenderProfileElement? ProfileElement { get; private set; }
    public ILayerProperty? LayerProperty { get; private set; }

    public override void Evaluate()
    {
        lock (_layerPropertyLock)
        {
            // In this case remove the pins so no further evaluations occur
            if (LayerProperty == null)
            {
                CreatePins();
                return;
            }

            List<IDataBindingProperty> list = LayerProperty.BaseDataBinding.Properties.ToList();
            int index = 0;
            foreach (IPin pin in Pins)
            {
                OutputPin outputPin = (OutputPin) pin;
                IDataBindingProperty dataBindingProperty = list[index];
                index++;

                // TODO: Is this really non-nullable?
                outputPin.Value = dataBindingProperty.GetValue();
            }
        }
    }

    public override void Initialize(INodeScript script)
    {
        Script = script;

        if (script.Context is Profile profile)
            profile.ChildRemoved += ProfileOnChildRemoved;

        LoadLayerProperty();
    }

    public void LoadLayerProperty()
    {
        lock (_layerPropertyLock)
        {
            if (Script?.Context is not Profile profile || Storage == null)
                return;

            RenderProfileElement? element = profile.GetAllRenderElements().FirstOrDefault(l => l.EntityId == Storage.ElementId);

            ProfileElement = element;
            LayerProperty = element?.GetAllLayerProperties().FirstOrDefault(p => p.Path == Storage.PropertyPath);
            CreatePins();
        }
    }

    public void ChangeProfileElement(RenderProfileElement? profileElement)
    {
        lock (_layerPropertyLock)
        {
            ProfileElement = profileElement;
            LayerProperty = profileElement?.GetAllLayerProperties().FirstOrDefault();

            Storage = new LayerPropertyNodeEntity
            {
                ElementId = ProfileElement?.EntityId ?? Guid.Empty,
                PropertyPath = null
            };

            CreatePins();
        }
    }

    public void ChangeLayerProperty(ILayerProperty? layerProperty)
    {
        lock (_layerPropertyLock)
        {
            LayerProperty = layerProperty;

            Storage = new LayerPropertyNodeEntity
            {
                ElementId = ProfileElement?.EntityId ?? Guid.Empty,
                PropertyPath = LayerProperty?.Path
            };

            CreatePins();
        }
    }

    private void CreatePins()
    {
        while (Pins.Any())
            RemovePin((Pin) Pins.First());

        if (LayerProperty == null)
            return;

        foreach (IDataBindingProperty dataBindingRegistration in LayerProperty.BaseDataBinding.Properties)
            CreateOrAddOutputPin(dataBindingRegistration.ValueType, dataBindingRegistration.DisplayName);
    }

    /// <summary>
    ///     Creates or adds an input pin to the node using a bucket.
    ///     The bucket might grow a bit over time as the user edits the node but pins won't get lost, enabling undo/redo in the
    ///     editor.
    /// </summary>
    private new void CreateOrAddOutputPin(Type valueType, string displayName)
    {
        // Grab the first pin from the bucket that isn't on the node yet
        OutputPin? pin = _pinBucket.FirstOrDefault(p => !Pins.Contains(p));

        // If there is none, create a new one and add it to the bucket
        if (pin == null)
        {
            pin = CreateOutputPin(valueType, displayName);
            _pinBucket.Add(pin);
        }
        // If there was a pin in the bucket, update it's type and display name and reuse it
        else
        {
            pin.ChangeType(valueType);
            pin.Name = displayName;
            AddPin(pin);
        }
    }

    private void ProfileOnChildRemoved(object? sender, EventArgs e)
    {
        if (Script?.Context is not Profile profile)
            return;

        if (ProfileElement == null || !profile.GetAllRenderElements().Contains(ProfileElement))
            ChangeProfileElement(null);
    }
}

public class LayerPropertyNodeEntity
{
    public Guid ElementId { get; set; }
    public string? PropertyPath { get; set; }
}