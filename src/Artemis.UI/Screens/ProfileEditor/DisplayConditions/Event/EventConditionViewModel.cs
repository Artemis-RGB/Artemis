using System;
using System.Linq;
using System.Windows.Input;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.DisplayConditions.Event
{
    public class EventConditionViewModel : Screen
    {
        private readonly IProfileEditorService _profileEditorService;
        private readonly IWindowManager _windowManager;
        private readonly INodeVmFactory _nodeVmFactory;

        public EventConditionViewModel(EventCondition eventCondition, IProfileEditorService profileEditorService, IWindowManager windowManager, INodeVmFactory nodeVmFactory)
        {
            _profileEditorService = profileEditorService;
            _windowManager = windowManager;
            _nodeVmFactory = nodeVmFactory;
            EventCondition = eventCondition;
        }

        public EventCondition EventCondition { get; }
        public BindableCollection<Type> FilterTypes { get; }
        public BindableCollection<Module> Modules { get; }

        public TimeLineEventOverlapMode EventOverlapMode
        {
            get => EventCondition.EventOverlapMode;
            set
            {
                if (EventCondition.EventOverlapMode == value) return;
                EventCondition.EventOverlapMode = value;
                _profileEditorService.SaveSelectedProfileElement();
            }
        }

        public void DataModelPathSelected(object sender, DataModelSelectedEventArgs e)
        {
            EventCondition.UpdateEventNode();
            DisplayName = EventCondition.EventPath?.Segments.LastOrDefault()?.GetPropertyDescription()?.Name ?? "Invalid event";
        }

        public void ScriptGridMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;

            _windowManager.ShowDialog(_nodeVmFactory.NodeScriptWindowViewModel(EventCondition.Script));
            _profileEditorService.SaveSelectedProfileElement();
        }
    }
}