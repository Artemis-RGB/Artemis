using Artemis.Core;
using Artemis.UI.Shared;
using Stylet;
using EnumUtilities = Artemis.UI.Shared.EnumUtilities;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree.Dialogs.AdaptionHints
{
    public class CategoryAdaptionHintViewModel : AdaptionHintViewModel
    {
        private bool _takeAllDevices;

        /// <inheritdoc />
        public CategoryAdaptionHintViewModel(CategoryAdaptionHint adaptionHint) : base(adaptionHint)
        {
            CategoryAdaptionHint = adaptionHint;
            Categories = new BindableCollection<ValueDescription>(EnumUtilities.GetAllValuesAndDescriptions(typeof(DeviceCategory)));
        }

        public CategoryAdaptionHint CategoryAdaptionHint { get; }
        public BindableCollection<ValueDescription> Categories { get; }

        public bool TakeAllDevices
        {
            get => _takeAllDevices;
            set => SetAndNotify(ref _takeAllDevices, value);
        }

        #region Overrides of Screen

        /// <inheritdoc />
        protected override void OnInitialActivate()
        {
            TakeAllDevices = !CategoryAdaptionHint.LimitAmount;
            base.OnInitialActivate();
        }


        /// <inheritdoc />
        protected override void OnClose()
        {
            CategoryAdaptionHint.LimitAmount = !TakeAllDevices;
            base.OnClose();
        }

        #endregion
    }
}