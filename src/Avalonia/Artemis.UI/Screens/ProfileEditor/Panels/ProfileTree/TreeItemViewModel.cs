using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core;
using Artemis.UI.Shared;
using HistoricalReactiveCommand;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree
{
    public abstract class TreeItemViewModel : ActivatableViewModelBase
    {
        private bool _isExpanded;

        protected TreeItemViewModel(TreeItemViewModel? parent, RenderProfileElement profileElement)
        {
            Parent = parent;
            ProfileElement = profileElement;

            AddLayerAtIndex = ReactiveCommandEx.CreateWithHistory<int, Layer>("AddLayerAtIndex",
                (targetIndex, _) => new Layer(ProfileElement, "New folder", targetIndex),
                (targetIndex, _) =>
                {
                    Layer toRemove = (Layer) ProfileElement.Children.ElementAt(targetIndex);
                    ProfileElement.RemoveChild(toRemove);
                    return toRemove;
                },
                historyId: ProfileElement.Profile.EntityId.ToString()
            );

            AddFolderAtIndex = ReactiveCommandEx.CreateWithHistory<int, Folder>("AddFolderAtIndex",
                (targetIndex, _) => new Folder(ProfileElement, "New folder", targetIndex),
                (targetIndex, _) =>
                {
                    Folder toRemove = (Folder) ProfileElement.Children.ElementAt(targetIndex);
                    ProfileElement.RemoveChild(toRemove);
                    return toRemove;
                },
                historyId: ProfileElement.Profile.EntityId.ToString()
            );
        }

        public ReactiveCommandWithHistory<int, Layer> AddLayerAtIndex { get; set; }
        public ReactiveCommandWithHistory<int, Folder> AddFolderAtIndex { get; set; }

        public RenderProfileElement ProfileElement { get; }
        public TreeItemViewModel? Parent { get; set; }
        public ObservableCollection<TreeItemViewModel> Children { get; } = new();

        public bool IsExpanded
        {
            get => _isExpanded;
            set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
        }
    }
}