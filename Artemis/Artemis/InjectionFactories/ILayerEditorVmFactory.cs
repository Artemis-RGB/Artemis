using Artemis.Models.Interfaces;
using Artemis.Profiles.Layers.Models;
using Artemis.ViewModels.Profiles;

namespace Artemis.InjectionFactories
{
    public interface ILayerEditorVmFactory
    {
        LayerEditorViewModel CreateLayerEditorVm(IDataModel dataModel, LayerModel layer);
    }
}