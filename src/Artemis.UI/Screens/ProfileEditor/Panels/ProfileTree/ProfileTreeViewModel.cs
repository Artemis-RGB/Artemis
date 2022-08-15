using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.ProfileEditor;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree;

public class ProfileTreeViewModel : TreeItemViewModel
{
    private readonly IProfileEditorService _profileEditorService;
    private TreeItemViewModel? _selectedChild;
    private ObservableAsPropertyHelper<bool>? _focusNone;
    private ObservableAsPropertyHelper<bool>? _focusFolder;
    private ObservableAsPropertyHelper<bool>? _focusSelection;

    public ProfileTreeViewModel(IWindowService windowService, IProfileEditorService profileEditorService, IProfileEditorVmFactory profileEditorVmFactory)
        : base(null, null, windowService, profileEditorService, profileEditorVmFactory)
    {
        _profileEditorService = profileEditorService;
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

            _focusNone = profileEditorService.FocusMode.Select(f => f == ProfileEditorFocusMode.None).ToProperty(this, vm => vm.FocusNone).DisposeWith(d);
            _focusFolder = profileEditorService.FocusMode.Select(f => f == ProfileEditorFocusMode.Folder).ToProperty(this, vm => vm.FocusFolder).DisposeWith(d);
            _focusSelection = profileEditorService.FocusMode.Select(f => f == ProfileEditorFocusMode.Selection).ToProperty(this, vm => vm.FocusSelection).DisposeWith(d);
        });

        this.WhenAnyValue(vm => vm.SelectedChild).Subscribe(model =>
        {
            if (model?.ProfileElement is RenderProfileElement renderProfileElement)
                profileEditorService.ChangeCurrentProfileElement(renderProfileElement);
        });
    }

    public bool FocusNone => _focusNone?.Value ?? false;
    public bool FocusFolder => _focusFolder?.Value ?? false;
    public bool FocusSelection => _focusSelection?.Value ?? false;

    public TreeItemViewModel? SelectedChild
    {
        get => _selectedChild;
        set => RaiseAndSetIfChanged(ref _selectedChild, value);
    }

    public override bool SupportsChildren => true;

    public void ClearSelection()
    {
        _profileEditorService.ChangeCurrentProfileElement(null);
    }

    public void RenameSelected()
    {
        SelectedChild?.Rename.Execute().Subscribe();
    }

    public void DeleteSelected()
    {
        SelectedChild?.Delete.Execute().Subscribe();
    }

    public void DuplicateSelected()
    {
        SelectedChild?.Duplicate.Execute().Subscribe();
    }

    public void CopySelected()
    {
        SelectedChild?.Copy.Execute().Subscribe();
    }

    public void PasteSelected()
    {
        SelectedChild?.Paste.Execute().Subscribe();
    }

    public void UpdateCanPaste()
    {
        throw new NotImplementedException();
    }

    protected override Task ExecuteDuplicate()
    {
        throw new NotSupportedException();
    }

    protected override Task ExecuteCopy()
    {
        throw new NotSupportedException();
    }

    protected override Task ExecutePaste()
    {
        throw new NotSupportedException();
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