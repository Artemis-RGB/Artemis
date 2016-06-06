using Artemis.Models.Interfaces;
using Artemis.Models.Profiles;
using Artemis.Services;
using Artemis.ViewModels.Profiles;

namespace Artemis.InjectionFactories
{
    public interface ILayerEditorVmFactory
    {
        LayerEditorViewModel CreateLayerEditorVm(IDataModel dataModel, LayerModel layer);
    }
}