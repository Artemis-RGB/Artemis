using System.Linq;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.DisplayConditions
{
    public class DisplayConditionsViewModel : Conductor<DisplayConditionGroupViewModel>, IProfileEditorPanelViewModel
    {
        private readonly IDisplayConditionsVmFactory _displayConditionsVmFactory;
        private readonly IProfileEditorService _profileEditorService;
        private RenderProfileElement _renderProfileElement;
        private int _transitionerIndex;

        public DisplayConditionsViewModel(IProfileEditorService profileEditorService, IDisplayConditionsVmFactory displayConditionsVmFactory)
        {
            _profileEditorService = profileEditorService;
            _displayConditionsVmFactory = displayConditionsVmFactory;
        }

        public int TransitionerIndex
        {
            get => _transitionerIndex;
            set => SetAndNotify(ref _transitionerIndex, value);
        }


        public RenderProfileElement RenderProfileElement
        {
            get => _renderProfileElement;
            set => SetAndNotify(ref _renderProfileElement, value);
        }

        public bool DisplayContinuously
        {
            get => RenderProfileElement?.DisplayContinuously ?? false;
            set
            {
                if (RenderProfileElement == null || RenderProfileElement.DisplayContinuously == value) return;
                RenderProfileElement.DisplayContinuously = value;
                _profileEditorService.UpdateSelectedProfileElement();
            }
        }

        public bool AlwaysFinishTimeline
        {
            get => RenderProfileElement?.AlwaysFinishTimeline ?? false;
            set
            {
                if (RenderProfileElement == null || RenderProfileElement.AlwaysFinishTimeline == value) return;
                RenderProfileElement.AlwaysFinishTimeline = value;
                _profileEditorService.UpdateSelectedProfileElement();
            }
        }

        public bool ConditionBehaviourEnabled => RenderProfileElement != null;

        protected override void OnActivate()
        {
            _profileEditorService.ProfileElementSelected += ProfileEditorServiceOnProfileElementSelected;
        }

        protected override void OnDeactivate()
        {
            _profileEditorService.ProfileElementSelected -= ProfileEditorServiceOnProfileElementSelected;
        }

        private void ProfileEditorServiceOnProfileElementSelected(object sender, RenderProfileElementEventArgs e)
        {
            RenderProfileElement = e.RenderProfileElement;
            NotifyOfPropertyChange(nameof(DisplayContinuously));
            NotifyOfPropertyChange(nameof(AlwaysFinishTimeline));
            NotifyOfPropertyChange(nameof(ConditionBehaviourEnabled));

            if (e.RenderProfileElement == null)
            {
                ActiveItem = null;
                return;
            }

            // Ensure the layer has a root display condition group
            if (e.RenderProfileElement.DisplayConditionGroup == null)
                e.RenderProfileElement.DisplayConditionGroup = new DisplayConditionGroup(null);

            ActiveItem = _displayConditionsVmFactory.DisplayConditionGroupViewModel(e.RenderProfileElement.DisplayConditionGroup, false);
            ActiveItem.IsRootGroup = true;
            ActiveItem.Update();

            // Only show the intro to conditions once, and only if the layer has no conditions
            if (TransitionerIndex != 1)
                TransitionerIndex = ActiveItem.Items.Any() ? 1 : 0;
        }
    }
}