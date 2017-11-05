using System.Collections.Generic;
using System.Linq;
using Artemis.Managers;
using Artemis.Modules.Abstract;
using Artemis.ViewModels.Abstract;

namespace Artemis.ViewModels
{
    public sealed class GeneralViewModel : BaseViewModel
    {
        private readonly List<ModuleViewModel> _vms;

        public GeneralViewModel(List<ModuleViewModel> moduleViewModels, PreviewManager previewManager)
        {
            DisplayName = "General Lighting";
            _vms = moduleViewModels.Where(m => !m.ModuleModel.IsOverlay && !m.ModuleModel.IsBoundToProcess)
                .OrderByDescending(m => m.DisplayName == "General profile")
                .ThenBy(m => m.DisplayName).ToList();

            previewManager.PreviewViewModules.Clear();
            previewManager.PreviewViewModules.AddRange(moduleViewModels.Where(m => m.UsesProfileEditor));
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            Items.Clear();
            Items.AddRange(_vms);
        }
    }
}