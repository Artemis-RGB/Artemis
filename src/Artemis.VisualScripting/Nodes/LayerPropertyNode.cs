using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core;
using Artemis.VisualScripting.Nodes.CustomViewModels;

namespace Artemis.VisualScripting.Nodes
{
    [Node("Layer/Folder Property", "Outputs the property of a selected layer or folder")]
    public class LayerPropertyNode : Node<LayerPropertyNodeCustomViewModel>
    {
        private readonly object _layerPropertyLock = new();

        public INodeScript Script { get; private set; }
        public RenderProfileElement ProfileElement { get; private set; }
        public ILayerProperty LayerProperty { get; private set; }

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

                List<IDataBindingRegistration> list = LayerProperty.GetAllDataBindingRegistrations();
                int index = 0;
                foreach (IPin pin in Pins)
                {
                    OutputPin outputPin = (OutputPin) pin;
                    IDataBindingRegistration dataBindingRegistration = list[index];
                    index++;

                    // TODO: Is this really non-nullable?
                    outputPin.Value = dataBindingRegistration.GetValue();
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
                if (Script.Context is not Profile profile)
                    return;
                if (Storage is not LayerPropertyNodeEntity entity)
                    return;

                RenderProfileElement element = profile.GetAllRenderElements().FirstOrDefault(l => l.EntityId == entity.ElementId);

                ProfileElement = element;
                LayerProperty = element?.GetAllLayerProperties().FirstOrDefault(p => p.Path == entity.PropertyPath);
                CreatePins();
            }
        }

        public void ChangeProfileElement(RenderProfileElement profileElement)
        {
            lock (_layerPropertyLock)
            {
                ProfileElement = profileElement;
                LayerProperty = null;

                Storage = new LayerPropertyNodeEntity
                {
                    ElementId = ProfileElement?.EntityId ?? Guid.Empty,
                    PropertyPath = null
                };

                CreatePins();
            }
        }

        public void ChangeLayerProperty(ILayerProperty layerProperty)
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

            foreach (IDataBindingRegistration dataBindingRegistration in LayerProperty.GetAllDataBindingRegistrations())
                CreateOutputPin(dataBindingRegistration.ValueType, dataBindingRegistration.DisplayName);
        }

        private void ProfileOnChildRemoved(object? sender, EventArgs e)
        {
            if (Script.Context is not Profile profile)
                return;

            if (!profile.GetAllRenderElements().Contains(ProfileElement))
                ChangeProfileElement(null);
        }
    }

    public class LayerPropertyNodeEntity
    {
        public Guid ElementId { get; set; }
        public string PropertyPath { get; set; }
    }
}