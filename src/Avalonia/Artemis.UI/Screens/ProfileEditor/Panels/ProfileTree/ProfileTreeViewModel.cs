using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared.Services.Interfaces;
using Artemis.UI.Shared.Services.ProfileEditor;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree
{
    public class ProfileTreeViewModel : TreeItemViewModel
    {
        private TreeItemViewModel? _selectedChild;

        public ProfileTreeViewModel(IWindowService windowService, IProfileEditorService profileEditorService, IProfileEditorVmFactory profileEditorVmFactory, IRgbService rgbService)
            : base(null, null, windowService, profileEditorService, rgbService, profileEditorVmFactory)
        {
            this.WhenActivated(d =>
            {
                profileEditorService.ProfileConfiguration.WhereNotNull().Subscribe(configuration =>
                {
                    if (configuration.Profile == null)
                    {
                        windowService.ShowConfirmContentDialog("Failed to load profile", "It appears that this profile is corrupt and cannot be loaded. Please check your logs.", "Confirm", null);
                        return;
                    }

                    ProfileElement = configuration.Profile.GetRootFolder();
                    SubscribeToProfileElement(d);
                    CreateTreeItems();
                }).DisposeWith(d);

                profileEditorService.ProfileElement.Subscribe(SelectCurrentProfileElement).DisposeWith(d);
            });

            this.WhenAnyValue(vm => vm.SelectedChild).Subscribe(model =>
            {
                if (model?.ProfileElement is RenderProfileElement renderProfileElement)
                    profileEditorService.ChangeCurrentProfileElement(renderProfileElement);
            });
        }
        
        public TreeItemViewModel? SelectedChild
        {
            get => _selectedChild;
            set => RaiseAndSetIfChanged(ref _selectedChild, value);
        }

        private void SelectCurrentProfileElement(RenderProfileElement? element)
        {
            if (SelectedChild?.ProfileElement == element)
                return;

            // Find the tree item belonging to the selected element
            List<TreeItemViewModel> treeItems = GetAllTreeItems(Children);
            TreeItemViewModel? selected = treeItems.FirstOrDefault(e => e.ProfileElement == element);

            // Walk up the tree, expanding parents
            TreeItemViewModel? currentParent = selected?.Parent;
            while (currentParent != null)
            {
                currentParent.IsExpanded = true;
                currentParent = currentParent.Parent;
            }

            SelectedChild = selected;
        }

        private List<TreeItemViewModel> GetAllTreeItems(ObservableCollection<TreeItemViewModel> treeItems)
        {
            List<TreeItemViewModel> result = new();
            foreach (TreeItemViewModel treeItemViewModel in treeItems)
            {
                result.Add(treeItemViewModel);
                if (treeItemViewModel.Children.Any())
                    result.AddRange(GetAllTreeItems(treeItemViewModel.Children));
            }

            return result;
        }
    }
}