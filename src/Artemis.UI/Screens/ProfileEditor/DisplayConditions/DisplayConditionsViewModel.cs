using System.Linq;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.Conditions;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared.Events;
using Artemis.UI.Shared.Services.Interfaces;

namespace Artemis.UI.Screens.ProfileEditor.DisplayConditions
{
    public class DisplayConditionsViewModel : ProfileEditorPanelViewModel
    {
        private readonly IDisplayConditionsVmFactory _displayConditionsVmFactory;
        private readonly IProfileEditorService _profileEditorService;
        private RenderProfileElement _renderProfileElement;
        private DisplayConditionGroupViewModel _rootGroup;
        private int _transitionerIndex;
        private bool _displayContinuously;
        private bool _alwaysFinishTimeline;

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

        public DisplayConditionGroupViewModel RootGroup
        {
            get => _rootGroup;
            set => SetAndNotify(ref _rootGroup, value);
        }

        public RenderProfileElement RenderProfileElement
        {
            get => _renderProfileElement;
            set => SetAndNotify(ref _renderProfileElement, value);
        }

        public bool DisplayContinuously
        {
            get => _displayContinuously;
            set
            {
                if (!SetAndNotify(ref _displayContinuously, value)) return;
                _profileEditorService.UpdateSelectedProfileElement();
            }
        }

        public bool AlwaysFinishTimeline
        {
            get => _alwaysFinishTimeline;
            set
            {
                if (!SetAndNotify(ref _alwaysFinishTimeline, value)) return;
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

            RootGroup?.Dispose();
            RootGroup = null;
        }

        private void ProfileEditorServiceOnProfileElementSelected(object sender, RenderProfileElementEventArgs e)
        {
            RenderProfileElement = e.RenderProfileElement;
            NotifyOfPropertyChange(nameof(ConditionBehaviourEnabled));

            _displayContinuously = RenderProfileElement?.DisplayContinuously ?? false;
            NotifyOfPropertyChange(nameof(DisplayContinuously));
            _alwaysFinishTimeline = RenderProfileElement?.AlwaysFinishTimeline ?? false;
            NotifyOfPropertyChange(nameof(AlwaysFinishTimeline));
            
            if (e.RenderProfileElement == null)
            {
                RootGroup?.Dispose();
                RootGroup = null;
                return;
            }

            // Ensure the layer has a root display condition group
            if (e.RenderProfileElement.DisplayConditionGroup == null)
                e.RenderProfileElement.DisplayConditionGroup = new DisplayConditionGroup(null);

            RootGroup?.Dispose();
            RootGroup = _displayConditionsVmFactory.DisplayConditionGroupViewModel(e.RenderProfileElement.DisplayConditionGroup, null, false);
            RootGroup.IsRootGroup = true;
            RootGroup.Update();

            // Only show the intro to conditions once, and only if the layer has no conditions
            if (TransitionerIndex != 1)
                TransitionerIndex = RootGroup.Children.Any() ? 1 : 0;
        }
    }
}