using Artemis.Core;
using Artemis.UI.Shared;
using RGB.NET.Core;
using Stylet;
using EnumUtilities = Artemis.UI.Shared.EnumUtilities;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree.Dialogs.AdaptionHints
{
    public class DeviceAdaptionHintViewModel : AdaptionHintViewModel
    {
        private bool _takeAllDevices;

        /// <inheritdoc />
        public DeviceAdaptionHintViewModel(DeviceAdaptionHint adaptionHint) : base(adaptionHint)
        {
            DeviceAdaptionHint = adaptionHint;
            DeviceTypes = new BindableCollection<ValueDescription>(EnumUtilities.GetAllValuesAndDescriptions(typeof(RGBDeviceType)));
        }

        public DeviceAdaptionHint DeviceAdaptionHint { get; }
        public BindableCollection<ValueDescription> DeviceTypes { get; }

        public bool TakeAllDevices
        {
            get => _takeAllDevices;
            set => SetAndNotify(ref _takeAllDevices, value);
        }

        #region Overrides of Screen

        /// <inheritdoc />
        protected override void OnInitialActivate()
        {
            TakeAllDevices = !DeviceAdaptionHint.LimitAmount;
            base.OnInitialActivate();
        }


        /// <inheritdoc />
        protected override void OnClose()
        {
            DeviceAdaptionHint.LimitAmount = !TakeAllDevices;
            base.OnClose();
        }

        #endregion
    }
}