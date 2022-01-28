using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.Interfaces;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.ProfileEditor.Commands;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree
{
    public abstract class TreeItemViewModel : ActivatableViewModelBase
    {
        private readonly IProfileEditorVmFactory _profileEditorVmFactory;
        private readonly IWindowService _windowService;
        private readonly IProfileEditorService _profileEditorService;
        private bool _isExpanded;
        private ProfileElement? _profileElement;
        private RenderProfileElement? _currentProfileElement;
        private bool _renaming;
        private string? _renameValue;

        protected TreeItemViewModel(TreeItemViewModel? parent, ProfileElement? profileElement, IWindowService windowService, IProfileEditorService profileEditorService, IRgbService rgbService,
            IProfileEditorVmFactory profileEditorVmFactory)
        {
            _windowService = windowService;
            _profileEditorService = profileEditorService;
            _profileEditorVmFactory = profileEditorVmFactory;

            Parent = parent;
            ProfileElement = profileElement;

            AddLayer = ReactiveCommand.Create(() =>
            {
                if (ProfileElement is Layer targetLayer)
                {
                    Layer layer = new(targetLayer.Parent, "New layer");
                    layer.AddLeds(rgbService.EnabledDevices.SelectMany(d => d.Leds));
                    profileEditorService.ExecuteCommand(new AddProfileElement(layer, targetLayer.Parent, targetLayer.Order));
                }
                else if (ProfileElement != null)
                {
                    Layer layer = new(ProfileElement, "New layer");
                    layer.AddLeds(rgbService.EnabledDevices.SelectMany(d => d.Leds));
                    profileEditorService.ExecuteCommand(new AddProfileElement(layer, ProfileElement, 0));
                }
            });

            AddFolder = ReactiveCommand.Create(() =>
            {
                if (ProfileElement is Layer targetLayer)
                    profileEditorService.ExecuteCommand(new AddProfileElement(new Folder(targetLayer.Parent, "New folder"), targetLayer.Parent, targetLayer.Order));
                else if (ProfileElement != null)
                    profileEditorService.ExecuteCommand(new AddProfileElement(new Folder(ProfileElement, "New folder"), ProfileElement, 0));
            });

            Rename = ReactiveCommand.Create(() =>
            {
                Renaming = true;
                RenameValue = ProfileElement?.Name;
            });

            Duplicate = ReactiveCommand.Create(() => throw new NotImplementedException());
            Copy = ReactiveCommand.Create(() => throw new NotImplementedException());
            Paste = ReactiveCommand.Create(() => throw new NotImplementedException());

            Delete = ReactiveCommand.Create(() =>
            {
                if (ProfileElement is RenderProfileElement renderProfileElement)
                    profileEditorService.ExecuteCommand(new RemoveProfileElement(renderProfileElement));
            });

            this.WhenActivated(d =>
            {
                _profileEditorService.ProfileElement.Subscribe(element => _currentProfileElement = element).DisposeWith(d);
                SubscribeToProfileElement(d);
                CreateTreeItems();
            });
        }

        public ProfileElement? ProfileElement
        {
            get => _profileElement;
            set => this.RaiseAndSetIfChanged(ref _profileElement, value);
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
        }

        public bool Renaming
        {
            get => _renaming;
            set => this.RaiseAndSetIfChanged(ref _renaming, value);
        }

        public TreeItemViewModel? Parent { get; set; }
        public ObservableCollection<TreeItemViewModel> Children { get; } = new();

        public ReactiveCommand<Unit, Unit> AddLayer { get; }
        public ReactiveCommand<Unit, Unit> AddFolder { get; }
        public ReactiveCommand<Unit, Unit> Rename { get; }
        public ReactiveCommand<Unit, Unit> Duplicate { get; }
        public ReactiveCommand<Unit, Unit> Copy { get; }
        public ReactiveCommand<Unit, Unit> Paste { get; }
        public ReactiveCommand<Unit, Unit> Delete { get; }

        public string? RenameValue
        {
            get => _renameValue;
            set => this.RaiseAndSetIfChanged(ref _renameValue, value);
        }

        public async Task ShowBrokenStateExceptions()
        {
            if (ProfileElement == null)
                return;

            List<IBreakableModel> broken = ProfileElement.GetBrokenHierarchy().Where(b => b.BrokenStateException != null).ToList();

            foreach (IBreakableModel current in broken)
            {
                _windowService.ShowExceptionDialog($"{current.BrokenDisplayName} - {current.BrokenState}", current.BrokenStateException!);
                if (broken.Last() != current)
                    if (!await _windowService.ShowConfirmContentDialog("Broken state", "Do you want to view the next exception?"))
                        return;
            }
        }

        public void SubmitRename()
        {
            if (ProfileElement == null)
            {
                Renaming = false;
                return;
            }

            _profileEditorService.ExecuteCommand(new RenameProfileElement(ProfileElement, RenameValue));
            Renaming = false;
        }

        public void CancelRename()
        {
            Renaming = false;
        }

        protected void SubscribeToProfileElement(CompositeDisposable d)
        {
            if (ProfileElement == null)
                return;

            Observable.FromEventPattern<ProfileElementEventArgs>(x => ProfileElement.ChildAdded += x, x => ProfileElement.ChildAdded -= x)
                .Subscribe(c => AddTreeItemIfMissing(c.EventArgs.ProfileElement)).DisposeWith(d);
            Observable.FromEventPattern<ProfileElementEventArgs>(x => ProfileElement.ChildRemoved += x, x => ProfileElement.ChildRemoved -= x)
                .Subscribe(c => RemoveTreeItemsIfFound(c.EventArgs.ProfileElement)).DisposeWith(d);
        }

        protected void RemoveTreeItemsIfFound(ProfileElement profileElement)
        {
            List<TreeItemViewModel> toRemove = Children.Where(t => t.ProfileElement == profileElement).ToList();
            foreach (TreeItemViewModel treeItemViewModel in toRemove)
                Children.Remove(treeItemViewModel);

            // Deselect the current profile element if removed
            if (_currentProfileElement == profileElement)
                _profileEditorService.ChangeCurrentProfileElement(null);
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
                _profileEditorService.ChangeCurrentProfileElement(renderProfileElement);
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
    }
}