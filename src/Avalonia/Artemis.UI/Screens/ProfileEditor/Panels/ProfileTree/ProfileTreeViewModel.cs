using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.Commands;
using Artemis.UI.Services.ProfileEditor;
using Artemis.UI.Shared;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree
{
    public class ProfileTreeViewModel : ActivatableViewModelBase
    {
        private readonly IProfileEditorService _profileEditorService;
        private readonly IProfileEditorVmFactory _profileEditorVmFactory;
        private ReactiveCommand<Unit, Unit>? _addFolder;
        private ReactiveCommand<Unit, Unit>? _addLayer;
        private TreeItemViewModel? _selectedTreeItem;

        public ProfileTreeViewModel(IProfileEditorService profileEditorService, IProfileEditorVmFactory profileEditorVmFactory)
        {
            _profileEditorService = profileEditorService;
            _profileEditorVmFactory = profileEditorVmFactory;

            this.WhenActivated(d =>
            {
                profileEditorService.ProfileConfiguration.WhereNotNull().Subscribe(configuration =>
                {
                    Folder rootFolder = configuration.Profile!.GetRootFolder();
                    CreateTreeItems(rootFolder);
                    Observable.FromEventPattern<ProfileElementEventArgs>(x => rootFolder.ChildAdded += x, x => rootFolder.ChildAdded -= x).Subscribe(c => AddTreeItemIfMissing(c.EventArgs.ProfileElement));
                    Observable.FromEventPattern<ProfileElementEventArgs>(x => rootFolder.ChildRemoved += x, x => rootFolder.ChildRemoved -= x).Subscribe(c => RemoveTreeItemsIfFound(c.EventArgs.ProfileElement));

                    AddLayer = ReactiveCommand.Create(() => AddLayerToRoot(configuration.Profile), profileEditorService.ProfileConfiguration.Select(p => p != null));
                    AddFolder = ReactiveCommand.Create(() => AddFolderToRoot(configuration.Profile), profileEditorService.ProfileConfiguration.Select(p => p != null));
                }).DisposeWith(d);

                profileEditorService.ProfileElement.WhereNotNull().Subscribe(SelectCurrentProfileElement).DisposeWith(d);
            });
            this.WhenAnyValue(vm => vm.SelectedTreeItem).Subscribe(model => profileEditorService.ChangeCurrentProfileElement(model?.ProfileElement));
        }

        public ReactiveCommand<Unit, Unit>? AddLayer
        {
            get => _addLayer;
            set => this.RaiseAndSetIfChanged(ref _addLayer, value);
        }

        public ReactiveCommand<Unit, Unit>? AddFolder
        {
            get => _addFolder;
            set => this.RaiseAndSetIfChanged(ref _addFolder, value);
        }

        public ObservableCollection<TreeItemViewModel> TreeItems { get; } = new();

        public TreeItemViewModel? SelectedTreeItem
        {
            get => _selectedTreeItem;
            set => this.RaiseAndSetIfChanged(ref _selectedTreeItem, value);
        }

        private void RemoveTreeItemsIfFound(ProfileElement profileElement)
        {
            List<TreeItemViewModel> toRemove = TreeItems.Where(t => t.ProfileElement == profileElement).ToList();
            foreach (TreeItemViewModel treeItemViewModel in toRemove)
                TreeItems.Remove(treeItemViewModel);
        }

        private void AddTreeItemIfMissing(ProfileElement profileElement)
        {
            if (TreeItems.Any(t => t.ProfileElement == profileElement))
                return;

            if (profileElement is Folder folder)
                TreeItems.Insert(folder.Parent.Children.IndexOf(folder), _profileEditorVmFactory.FolderTreeItemViewModel(null, folder));
            else if (profileElement is Layer layer)
                TreeItems.Insert(layer.Parent.Children.IndexOf(layer), _profileEditorVmFactory.LayerTreeItemViewModel(null, layer));
        }

        private void AddFolderToRoot(Profile profile)
        {
            Folder rootFolder = profile.GetRootFolder();
            Folder folder = new(rootFolder, "New folder");
            _profileEditorService.ExecuteCommand(new AddProfileElement(folder, rootFolder, 0));
        }

        private void AddLayerToRoot(Profile profile)
        {
            Folder rootFolder = profile.GetRootFolder();
            Layer layer = new(rootFolder, "New layer");
            _profileEditorService.ExecuteCommand(new AddProfileElement(layer, rootFolder, 0));
        }

        private void CreateTreeItems(Folder rootFolder)
        {
            if (TreeItems.Any())
                TreeItems.Clear();

            foreach (ProfileElement profileElement in rootFolder.Children)
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