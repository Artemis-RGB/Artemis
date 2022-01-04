﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Services.ProfileEditor;
using Artemis.UI.Shared.Services.Interfaces;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree
{
    public class ProfileTreeViewModel : TreeItemViewModel
    {
        private TreeItemViewModel? _selectedChild;

        public ProfileTreeViewModel(IWindowService windowService, IProfileEditorService profileEditorService, IProfileEditorVmFactory profileEditorVmFactory)
            : base(null, null, windowService, profileEditorService, profileEditorVmFactory)
        {
            this.WhenActivated(d =>
            {
                profileEditorService.ProfileConfiguration.WhereNotNull().Subscribe(configuration =>
                {
                    ProfileElement = configuration.Profile!.GetRootFolder();
                    SubscribeToProfileElement(d);
                    CreateTreeItems();
                }).DisposeWith(d);

                profileEditorService.ProfileElement.WhereNotNull().Subscribe(SelectCurrentProfileElement).DisposeWith(d);
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
            set => this.RaiseAndSetIfChanged(ref _selectedChild, value);
        }

        private void SelectCurrentProfileElement(RenderProfileElement element)
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