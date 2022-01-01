using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.Interfaces;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree
{
    public abstract class TreeItemViewModel : ActivatableViewModelBase
    {
        private readonly IWindowService _windowService;
        private bool _isExpanded;

        protected TreeItemViewModel(TreeItemViewModel? parent, RenderProfileElement profileElement, IWindowService windowService)
        {
            _windowService = windowService;
            Parent = parent;
            ProfileElement = profileElement;

            // AddLayerAtIndex = ReactiveCommandWithHistory.CreateWithHistory<int, Layer>(
            //     (targetIndex, _) => new Layer(ProfileElement, "New folder"),
            //     (targetIndex, _) =>
            //     {
            //         Layer toRemove = (Layer) ProfileElement.Children.ElementAt(targetIndex);
            //         ProfileElement.RemoveChild(toRemove);
            //         return toRemove;
            //     },
            //     historyId: ProfileElement.Profile.EntityId.ToString()
            // );

            // AddFolderAtIndex = ReactiveCommandWithHistory.CreateWithHistory<int, Folder>(
            //     (targetIndex, _) => new Folder(ProfileElement, "New folder"),
            //     (targetIndex, _) =>
            //     {
            //         Folder toRemove = (Folder) ProfileElement.Children.ElementAt(targetIndex);
            //         ProfileElement.RemoveChild(toRemove);
            //         return toRemove;
            //     },
            //     historyId: ProfileElement.Profile.EntityId.ToString()
            // );
        }

        public ReactiveCommand<int, Unit> AddLayerAtIndex { get; set; }
        public ReactiveCommand<int, Unit> AddFolderAtIndex { get; set; }

        public RenderProfileElement ProfileElement { get; }
        public TreeItemViewModel? Parent { get; set; }
        public ObservableCollection<TreeItemViewModel> Children { get; } = new();

        public bool IsExpanded
        {
            get => _isExpanded;
            set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
        }

        public async Task ShowBrokenStateExceptions()
        {
            List<IBreakableModel> broken = ProfileElement.GetBrokenHierarchy().Where(b => b.BrokenStateException != null).ToList();

            foreach (IBreakableModel current in broken)
            {
                _windowService.ShowExceptionDialog($"{current.BrokenDisplayName} - {current.BrokenState}", current.BrokenStateException!);
                if (broken.Last() != current)
                {
                    if (!await _windowService.ShowConfirmContentDialog("Broken state", "Do you want to view the next exception?"))
                        return;
                }
            }
        }
    }
}