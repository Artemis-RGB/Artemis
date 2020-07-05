using System.Linq;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.Conditions;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared.DataModelVisualization.Shared;
using Artemis.UI.Shared.Events;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Interfaces;

namespace Artemis.UI.Screens.Module.ProfileEditor.DisplayConditions
{
    public class DisplayConditionsViewModel : ProfileEditorPanelViewModel
    {
        private readonly IDisplayConditionsVmFactory _displayConditionsVmFactory;
        private readonly IDataModelVisualizationService _dataModelVisualizationService;
        private DisplayConditionGroupViewModel _rootGroup;

        public DisplayConditionsViewModel(IProfileEditorService profileEditorService, IDataModelVisualizationService dataModelVisualizationService,
            IDisplayConditionsVmFactory displayConditionsVmFactory)
        {
            _dataModelVisualizationService = dataModelVisualizationService;
            _displayConditionsVmFactory = displayConditionsVmFactory;
            profileEditorService.ProfileElementSelected += ProfileEditorServiceOnProfileElementSelected;
        }

        public DisplayConditionGroupViewModel RootGroup
        {
            get => _rootGroup;
            set => SetAndNotify(ref _rootGroup, value);
        }

        private void ProfileEditorServiceOnProfileElementSelected(object sender, ProfileElementEventArgs e)
        {
            _dataModelVisualizationService.BustCache();

            if (e.ProfileElement is Layer layer)
            {
                // Ensure the layer has a root display condition group
                if (layer.DisplayConditionGroup == null)
                    layer.DisplayConditionGroup = new DisplayConditionGroup();

                RootGroup = _displayConditionsVmFactory.DisplayConditionGroupViewModel(layer.DisplayConditionGroup, null);
                RootGroup.IsRootGroup = true;
            }
            else
                RootGroup = null;
        }
    }
}