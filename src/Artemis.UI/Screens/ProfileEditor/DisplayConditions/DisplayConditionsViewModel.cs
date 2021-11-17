using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.DisplayConditions
{
    public class DisplayConditionsViewModel : Conductor<Screen>, IProfileEditorPanelViewModel
    {
        private readonly IConditionVmFactory _conditionVmFactory;
        private readonly IProfileEditorService _profileEditorService;
        private DisplayConditionType _displayConditionType;
        private StaticCondition _staticCondition;
        private EventCondition _eventCondition;

        public DisplayConditionsViewModel(IProfileEditorService profileEditorService, IConditionVmFactory conditionVmFactory)
        {
            _profileEditorService = profileEditorService;
            _conditionVmFactory = conditionVmFactory;
        }

        public DisplayConditionType DisplayConditionType
        {
            get => _displayConditionType;
            set
            {
                if (!SetAndNotify(ref _displayConditionType, value)) return;
                ChangeConditionType();
            }
        }

        protected override void OnInitialActivate()
        {
            _profileEditorService.SelectedProfileElementChanged += ProfileEditorServiceOnSelectedProfileElementChanged;
            Update(_profileEditorService.SelectedProfileElement);

            base.OnInitialActivate();
        }

        protected override void OnClose()
        {
            _profileEditorService.SelectedProfileElementChanged -= ProfileEditorServiceOnSelectedProfileElementChanged;
            base.OnClose();
        }

        private void ChangeConditionType()
        {
            if (_profileEditorService.SelectedProfileElement == null)
                return;

            // Keep the old condition around in case the user changes their mind
            if (_profileEditorService.SelectedProfileElement.DisplayCondition is StaticCondition staticCondition)
                _staticCondition = staticCondition;
            else if (_profileEditorService.SelectedProfileElement.DisplayCondition is EventCondition eventCondition)
                _eventCondition = eventCondition;

            // If we have the old condition around put it back
            if (DisplayConditionType == DisplayConditionType.Static)
                _profileEditorService.SelectedProfileElement.DisplayCondition = _staticCondition ?? new StaticCondition(_profileEditorService.SelectedProfileElement);
            else if (DisplayConditionType == DisplayConditionType.Events)
                _profileEditorService.SelectedProfileElement.DisplayCondition = _eventCondition ?? new EventCondition(_profileEditorService.SelectedProfileElement);
            else
                _profileEditorService.SelectedProfileElement.DisplayCondition = null;

            _profileEditorService.SaveSelectedProfileElement();
            Update(_profileEditorService.SelectedProfileElement);
        }

        private void Update(RenderProfileElement renderProfileElement)
        {
            if (renderProfileElement == null)
            {
                ActiveItem = null;
                return;
            }

            if (renderProfileElement.DisplayCondition is StaticCondition staticCondition)
            {
                ActiveItem = _conditionVmFactory.StaticConditionViewModel(staticCondition);
                _displayConditionType = DisplayConditionType.Static;
            }
            else if (renderProfileElement.DisplayCondition is EventCondition eventsCondition)
            {
                ActiveItem = _conditionVmFactory.EventConditionViewModel(eventsCondition);
                _displayConditionType = DisplayConditionType.Events;
            }
            else
            {
                ActiveItem = null;
                _displayConditionType = DisplayConditionType.None;
            }

            NotifyOfPropertyChange(nameof(DisplayConditionType));
        }

        private void ProfileEditorServiceOnSelectedProfileElementChanged(object sender, RenderProfileElementEventArgs e)
        {
            if (_staticCondition != null && e.PreviousRenderProfileElement?.DisplayCondition != _staticCondition)
                _staticCondition.Dispose();
            if (_eventCondition != null && e.PreviousRenderProfileElement?.DisplayCondition != _eventCondition)
                _eventCondition.Dispose();

            Update(e.RenderProfileElement);
        }
    }

    public enum DisplayConditionType
    {
        None,
        Static,
        Events
    }
}