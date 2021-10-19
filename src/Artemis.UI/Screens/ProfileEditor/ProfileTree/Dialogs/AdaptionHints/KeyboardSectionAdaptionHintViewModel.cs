using Artemis.Core;
using Artemis.UI.Shared;
using Stylet;
using EnumUtilities = Artemis.UI.Shared.EnumUtilities;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree.Dialogs.AdaptionHints
{
    public class KeyboardSectionAdaptionHintViewModel : AdaptionHintViewModel
    {
        /// <inheritdoc />
        public KeyboardSectionAdaptionHintViewModel(KeyboardSectionAdaptionHint adaptionHint) : base(adaptionHint)
        {
            KeyboardSectionAdaptionHint = adaptionHint;
            Sections = new BindableCollection<ValueDescription>(EnumUtilities.GetAllValuesAndDescriptions(typeof(KeyboardSection)));
        }

        public KeyboardSectionAdaptionHint KeyboardSectionAdaptionHint { get; }
        public BindableCollection<ValueDescription> Sections { get; }
    }
}