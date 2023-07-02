using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.DryIoc.Factories;
using Artemis.UI.Extensions;
using Artemis.UI.Screens.ProfileEditor.ProfileTree.ContentDialogs;
using Artemis.UI.Screens.ProfileEditor.ProfileTree.Dialogs;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.ProfileEditor.Commands;
using Avalonia;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree;

public abstract class TreeItemViewModel : ActivatableViewModelBase
{
    private readonly IProfileEditorVmFactory _profileEditorVmFactory;
    private readonly IWindowService _windowService;
    protected readonly IProfileEditorService ProfileEditorService;
    private bool _canPaste;
    private RenderProfileElement? _currentProfileElement;
    private bool _isExpanded;
    private bool _isFlyoutOpen;
    private ObservableAsPropertyHelper<bool>? _isFocused;
    private ProfileElement? _profileElement;
    private TimeSpan _time;

    protected TreeItemViewModel(TreeItemViewModel? parent,
        ProfileElement? profileElement,
        IWindowService windowService,
        IProfileEditorService profileEditorService,
        IProfileEditorVmFactory profileEditorVmFactory)
    {
        ProfileEditorService = profileEditorService;
        _windowService = windowService;
        _profileEditorVmFactory = profileEditorVmFactory;

        Parent = parent;
        ProfileElement = profileElement;

        AddLayer = ReactiveCommand.Create(ExecuteAddLayer);
        AddFolder = ReactiveCommand.Create(ExecuteAddFolder);
        OpenAdaptionHints = ReactiveCommand.CreateFromTask(ExecuteOpenAdaptionHints, this.WhenAnyValue(vm => vm.ProfileElement).Select(p => p is Layer));
        Rename = ReactiveCommand.CreateFromTask(ExecuteRename);
        Delete = ReactiveCommand.Create(ExecuteDelete);
        Duplicate = ReactiveCommand.CreateFromTask(ExecuteDuplicate);
        Copy = ReactiveCommand.CreateFromTask(ExecuteCopy);
        Paste = ReactiveCommand.CreateFromTask(ExecutePaste, this.WhenAnyValue(vm => vm.CanPaste));
        AbsorbCommand = ReactiveCommand.Create(() => true);

        this.WhenActivated(d =>
        {
            _isFocused = ProfileEditorService.FocusMode
                .CombineLatest(ProfileEditorService.ProfileElement)
                .Select(tuple => GetIsFocused(tuple.First, tuple.Second))
                .ToProperty(this, vm => vm.IsFocused)
                .DisposeWith(d);

            ProfileEditorService.Time.Subscribe(t => _time = t).DisposeWith(d);
            ProfileEditorService.ProfileElement.Subscribe(element => _currentProfileElement = element).DisposeWith(d);
            SubscribeToProfileElement(d);
            CreateTreeItems();
        });

        this.WhenAnyValue(vm => vm.IsFlyoutOpen).ObserveOn(AvaloniaScheduler.Instance).Subscribe(UpdateCanPaste);
    }

    public ReactiveCommand<Unit, bool> AbsorbCommand { get; }

    public bool IsFocused => _isFocused?.Value ?? false;

    public ProfileElement? ProfileElement
    {
        get => _profileElement;
        set => RaiseAndSetIfChanged(ref _profileElement, value);
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set => RaiseAndSetIfChanged(ref _isExpanded, value);
    }

    public bool IsFlyoutOpen
    {
        get => _isFlyoutOpen;
        set => RaiseAndSetIfChanged(ref _isFlyoutOpen, value);
    }

    public bool CanPaste
    {
        get => _canPaste;
        set => RaiseAndSetIfChanged(ref _canPaste, value);
    }

    public TreeItemViewModel? Parent { get; set; }
    public ObservableCollection<TreeItemViewModel> Children { get; } = new();

    public ReactiveCommand<Unit, Unit> AddLayer { get; }
    public ReactiveCommand<Unit, Unit> AddFolder { get; }
    public ReactiveCommand<Unit, Unit> OpenAdaptionHints { get; }
    public ReactiveCommand<Unit, Unit> Rename { get; }
    public ReactiveCommand<Unit, Unit> Duplicate { get; }
    public ReactiveCommand<Unit, Unit> Copy { get; }
    public ReactiveCommand<Unit, Unit> Paste { get; }
    public ReactiveCommand<Unit, Unit> Delete { get; }
    public abstract bool SupportsChildren { get; }
    
    public async Task ShowBrokenStateExceptions()
    {
        if (ProfileElement == null)
            return;

        List<IBreakableModel> broken = ProfileElement.GetBrokenHierarchy().Where(b => b.BrokenStateException != null).ToList();

        foreach (IBreakableModel current in broken)
        {
            _windowService.ShowExceptionDialog($"{current.BrokenDisplayName} - {current.BrokenState}", current.BrokenStateException!);
            if (broken.Last() == current)
                continue;
            if (!await _windowService.ShowConfirmContentDialog("Broken state", "Do you want to view the next exception?"))
                return;
        }
    }
    
    public void InsertElement(TreeItemViewModel elementViewModel, int targetIndex)
    {
        if (elementViewModel.Parent == this && Children.IndexOf(elementViewModel) == targetIndex)
            return;

        if (ProfileElement != null && elementViewModel.ProfileElement != null)
            ProfileEditorService.ExecuteCommand(new MoveProfileElement(ProfileElement, elementViewModel.ProfileElement, targetIndex));
    }

    protected abstract Task ExecuteDuplicate();
    protected abstract Task ExecuteCopy();
    protected abstract Task ExecutePaste();

    protected void SubscribeToProfileElement(CompositeDisposable d)
    {
        if (ProfileElement == null)
            return;

        Observable.FromEventPattern<ProfileElementEventArgs>(x => ProfileElement.ChildAdded += x, x => ProfileElement.ChildAdded -= x)
            .Subscribe(c => AddTreeItemIfMissing(c.EventArgs.ProfileElement))
            .DisposeWith(d);
        Observable.FromEventPattern<ProfileElementEventArgs>(x => ProfileElement.ChildRemoved += x, x => ProfileElement.ChildRemoved -= x)
            .Subscribe(c => RemoveTreeItemsIfFound(c.Sender, c.EventArgs.ProfileElement))
            .DisposeWith(d);
        ProfileElement.WhenAnyValue(e => e.Suspended).Subscribe(_ => ProfileEditorService.ChangeTime(_time)).DisposeWith(d);
    }

    protected void RemoveTreeItemsIfFound(object? sender, ProfileElement profileElement)
    {
        List<TreeItemViewModel> toRemove = Children.Where(t => t.ProfileElement == profileElement).ToList();
        foreach (TreeItemViewModel treeItemViewModel in toRemove)
            Children.Remove(treeItemViewModel);

        if (_currentProfileElement != profileElement)
            return;

        // Find a good candidate for a new selection, preferring the next sibling, falling back to the previous sibling and finally the parent
        ProfileElement? parent = sender as ProfileElement;
        ProfileElement? newSelection = null;
        if (parent != null)
        {
            newSelection = parent.Children.FirstOrDefault(c => c.Order == profileElement.Order) ?? parent.Children.FirstOrDefault(c => c.Order == profileElement.Order - 1);
            if (newSelection == null && parent is Folder {IsRootFolder: false})
                newSelection = parent;
        }

        ProfileEditorService.ChangeCurrentProfileElement(newSelection as RenderProfileElement);
    }

    protected void AddTreeItemIfMissing(ProfileElement profileElement)
    {
        if (Children.Any(t => t.ProfileElement == profileElement))
            return;

        if (profileElement is Folder folder)
            Children.Insert(folder.Parent.Children.IndexOf(folder), _profileEditorVmFactory.FolderTreeItemViewModel(this, folder));
        else if (profileElement is Layer layer)
            Children.Insert(layer.Parent.Children.IndexOf(layer), _profileEditorVmFactory.LayerTreeItemViewModel(this, layer));

        // Select the newly added element
        if (profileElement is RenderProfileElement renderProfileElement)
            ProfileEditorService.ChangeCurrentProfileElement(renderProfileElement);
    }

    protected void CreateTreeItems()
    {
        if (Children.Any())
            Children.Clear();

        if (ProfileElement == null)
            return;

        foreach (ProfileElement profileElement in ProfileElement.Children)
        {
            if (profileElement is Folder folder)
                Children.Add(_profileEditorVmFactory.FolderTreeItemViewModel(this, folder));
            else if (profileElement is Layer layer)
                Children.Add(_profileEditorVmFactory.LayerTreeItemViewModel(this, layer));
        }
    }

    private void ExecuteDelete()
    {
        if (ProfileElement is RenderProfileElement renderProfileElement)
            ProfileEditorService.ExecuteCommand(new RemoveProfileElement(renderProfileElement));
    }

    private async Task ExecuteRename()
    {
        if (ProfileElement == null)
            return;

        await _windowService.CreateContentDialog()
            .WithTitle(ProfileElement is Folder ? "Rename folder" : "Rename layer")
            .WithViewModel(out ProfileElementRenameViewModel vm, ProfileElement)
            .HavingPrimaryButton(b => b.WithText("Confirm").WithCommand(vm.Confirm))
            .WithCloseButtonText("Cancel")
            .WithDefaultButton(ContentDialogButton.Primary)
            .ShowAsync();
    }

    private void ExecuteAddFolder()
    {
        if (ProfileElement != null)
            ProfileEditorService.CreateAndAddFolder(ProfileElement);
    }

    private void ExecuteAddLayer()
    {
        if (ProfileElement != null)
            ProfileEditorService.CreateAndAddLayer(ProfileElement);
    }

    private async Task ExecuteOpenAdaptionHints()
    {
        if (ProfileElement is not Layer layer)
            return;

        await _windowService.ShowDialogAsync<LayerHintsDialogViewModel, bool>(layer);
        await ProfileEditorService.SaveProfileAsync();
    }

    private async void UpdateCanPaste(bool isFlyoutOpen)
    {
        string[] formats = await Shared.UI.Clipboard.GetFormatsAsync();
        //diogotr7: This can be null on Linux sometimes. I'm not sure why.
        if (formats == null!)
        {
            CanPaste = false;
            return;
        }

        CanPaste = formats.Contains(ProfileElementExtensions.ClipboardDataFormat);
    }

    private bool GetIsFocused(ProfileEditorFocusMode focusMode, RenderProfileElement? currentProfileElement)
    {
        if (focusMode == ProfileEditorFocusMode.None || currentProfileElement == null)
            return false;
        if (focusMode == ProfileEditorFocusMode.Selection)
            return currentProfileElement == ProfileElement;
        if (focusMode == ProfileEditorFocusMode.Folder && currentProfileElement?.Parent != null)
            // Any direct parent or direct siblings cause focus
            return currentProfileElement.Parent == ProfileElement?.Parent || currentProfileElement.Parent.GetAllRenderElements().Any(e => e == ProfileElement);

        return false;
    }
}