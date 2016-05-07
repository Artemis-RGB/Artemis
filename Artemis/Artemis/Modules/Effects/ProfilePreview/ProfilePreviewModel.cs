using System.Drawing;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Models.Interfaces;
using Artemis.Models.Profiles;

namespace Artemis.Modules.Effects.ProfilePreview
{
    public class ProfilePreviewModel : EffectModel
    {
        private readonly ProfilePreviewDataModel _previewDataModel;

        public ProfilePreviewModel(MainManager mainManager, KeyboardManager keyboardManager)
            : base(mainManager, keyboardManager)
        {
            Name = "Profile Preview";
            _previewDataModel = new ProfilePreviewDataModel();
        }

        public ProfileModel SelectedProfile { get; set; }

        public override void Dispose()
        {
            Initialized = false;
            SelectedProfile = null;
        }

        public override void Enable()
        {
            Initialized = true;
        }

        public override void Update()
        {
            if (SelectedProfile == null)
                return;

            foreach (var layerModel in SelectedProfile.Layers)
                layerModel.Update<ProfilePreviewDataModel>(_previewDataModel, true);
        }

        public override Bitmap GenerateBitmap()
        {
            var bitmap = KeyboardManager.ActiveKeyboard.KeyboardBitmap(4);

            if (SelectedProfile == null)
                return bitmap;

            var keyboardRect = KeyboardManager.ActiveKeyboard.KeyboardRectangle(4);
            var image = SelectedProfile.GenerateBitmap<ProfilePreviewDataModel>(keyboardRect, _previewDataModel, true);

            // Draw on top of everything else
            using (var g = Graphics.FromImage(bitmap))
                g.DrawImage(image, 0, 0);

            return bitmap;
        }
    }

    public class ProfilePreviewDataModel : IGameDataModel
    {
    }
}