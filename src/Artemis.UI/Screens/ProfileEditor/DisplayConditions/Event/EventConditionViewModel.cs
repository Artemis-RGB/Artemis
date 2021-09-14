using System;
using System.Collections.Specialized;
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
        private readonly INodeVmFactory _nodeVmFactory;
        private readonly IProfileEditorService _profileEditorService;
        private readonly IWindowManager _windowManager;

        public EventConditionViewModel(EventCondition eventCondition, IWindowManager windowManager, INodeVmFactory nodeVmFactory, IProfileEditorService profileEditorService)
        {
            _windowManager = windowManager;
            _nodeVmFactory = nodeVmFactory;
            _profileEditorService = profileEditorService;

            EventCondition = eventCondition;
            DisplayName = EventCondition.EventPath?.Segments.LastOrDefault()?.GetPropertyDescription()?.Name ?? "Invalid event";
            FilterTypes = new BindableCollection<Type> {typeof(IDataModelEvent) };
            Modules = new BindableCollection<Module>();

            if (_profileEditorService.SelectedProfileConfiguration?.Module != null)
                Modules.Add(_profileEditorService.SelectedProfileConfiguration.Module);
        }

        public EventCondition EventCondition { get; }
        public BindableCollection<Type> FilterTypes { get; }
        public BindableCollection<Module> Modules { get; }
        public bool CanDeleteEvent => ((EventsConditionViewModel) Parent).Items.Count > 1;

        public void DeleteEvent()
        {
            ((EventsConditionViewModel) Parent).DeleteEvent(this);
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

        #region Overrides of Screen

        /// <inheritdoc />
        protected override void OnInitialActivate()
        {
            ((EventsConditionViewModel) Parent).Items.CollectionChanged += ItemsOnCollectionChanged;
            base.OnInitialActivate();
        }

        /// <inheritdoc />
        protected override void OnClose()
        {
            ((EventsConditionViewModel) Parent).Items.CollectionChanged -= ItemsOnCollectionChanged;
            base.OnClose();
        }

        #endregion

        private void ItemsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyOfPropertyChange(nameof(CanDeleteEvent));
        }
    }
}