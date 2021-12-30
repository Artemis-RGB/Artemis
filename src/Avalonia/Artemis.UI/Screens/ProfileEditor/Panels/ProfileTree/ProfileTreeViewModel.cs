using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Services;
using Artemis.UI.Shared;
using HistoricalReactiveCommand;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree
{
    public class ProfileTreeViewModel : ActivatableViewModelBase
    {
        private readonly IProfileEditorVmFactory _profileEditorVmFactory;
        private TreeItemViewModel? _selectedTreeItem;

        public ProfileTreeViewModel(IProfileEditorService profileEditorService, IProfileEditorVmFactory profileEditorVmFactory)
        {
            _profileEditorVmFactory = profileEditorVmFactory;

            this.WhenActivated(d =>
            {
                ProfileConfiguration profileConfiguration = null!;
                profileEditorService.CurrentProfileConfiguration.WhereNotNull().Subscribe(p => profileConfiguration = p).DisposeWith(d);
                profileEditorService.CurrentProfileConfiguration.WhereNotNull().Subscribe(Repopulate).DisposeWith(d);
                profileEditorService.CurrentProfileElement.WhereNotNull().Subscribe(SelectCurrentProfileElement).DisposeWith(d);

                Folder rootFolder = profileConfiguration.Profile!.GetRootFolder();
                AddLayer = ReactiveCommandEx.CreateWithHistory("AddLayerAtRoot",
                    // ReSharper disable once ObjectCreationAsStatement
                    () => new Layer(rootFolder, "New layer", 0),
                    () => rootFolder.RemoveChild(rootFolder.Children[0]),
                    profileConfiguration.Profile.EntityId.ToString()
                );

                AddFolder = ReactiveCommandEx.CreateWithHistory("AddFolderAtRoot",
                    // ReSharper disable once ObjectCreationAsStatement
                    () => new Folder(rootFolder, "New folder", 0),
                    () => rootFolder.RemoveChild(rootFolder.Children[0]),
                    profileConfiguration.Profile.EntityId.ToString()
                );
            });
            this.WhenAnyValue(vm => vm.SelectedTreeItem).Subscribe(model => profileEditorService.ChangeCurrentProfileElement(model?.ProfileElement));
        }

        public ReactiveCommandWithHistory<Unit, Unit>? AddLayer { get; set; }
        public ReactiveCommandWithHistory<Unit, Unit>? AddFolder { get; set; }

        public ObservableCollection<TreeItemViewModel> TreeItems { get; } = new();

        public TreeItemViewModel? SelectedTreeItem
        {
            get => _selectedTreeItem;
            set => this.RaiseAndSetIfChanged(ref _selectedTreeItem, value);
        }

        private void Repopulate(ProfileConfiguration profileConfiguration)
        {
            if (TreeItems.Any())
                TreeItems.Clear();

            if (profileConfiguration.Profile == null)
                return;

            foreach (ProfileElement profileElement in profileConfiguration.Profile.GetRootFolder().Children)
            {
                if (profileElement is Folder folder)
                    TreeItems.Add(_profileEditorVmFactory.FolderTreeItemViewModel(null, folder));
                else if (profileElement is Layer layer)
                    TreeItems.Add(_profileEditorVmFactory.LayerTreeItemViewModel(null, layer));
            }
        }

        private void SelectCurrentProfileElement(RenderProfileElement element)
        {
            if (SelectedTreeItem?.ProfileElement == element)
                return;

            // Find the tree item belonging to the selected element
            List<TreeItemViewModel> treeItems = GetAllTreeItems(TreeItems);
            TreeItemViewModel? selected = treeItems.FirstOrDefault(e => e.ProfileElement == element);

            // Walk up the tree, expanding parents
            TreeItemViewModel? currentParent = selected?.Parent;
            while (currentParent != null)
            {
                currentParent.IsExpanded = true;
                currentParent = currentParent.Parent;
            }

            SelectedTreeItem = selected;
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