using Artemis.Profiles.Layers.Models;
using Caliburn.Micro;

namespace Artemis.ViewModels.Profiles
{
    public class LayerTweenViewModel : Screen
    {
        public LayerModel LayerModel { get; set; }

        public LayerTweenViewModel(LayerEditorViewModel editorViewModel)
        {
            LayerModel = editorViewModel.ProposedLayer;
            EaseFunctions = new BindableCollection<string> {"Linear", "In", "Out", "In/out"};
        }

        public BindableCollection<string> EaseFunctions { get; set; }
    }
}