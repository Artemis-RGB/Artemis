using System.Windows;
using System.Windows.Media;
using Artemis.Modules.Abstract;
using Artemis.Profiles.Layers.Abstract;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;
using Artemis.Properties;
using Artemis.Utilities;
using Artemis.ViewModels.Profiles;

namespace Artemis.Profiles.Layers.Types.Folder
{
    public class FolderType : ILayerType
    {
        public string Name => "Folder";
        public bool ShowInEdtor => false;
        // FolderType pretents to be a keyboard so it's children get drawn
        public DrawType DrawType => DrawType.Keyboard;

        public ImageSource DrawThumbnail(LayerModel layer)
        {
            var thumbnailRect = new Rect(0, 0, 18, 18);
            var visual = new DrawingVisual();
            using (var c = visual.RenderOpen())
                c.DrawImage(ImageUtilities.BitmapToBitmapImage(Resources.folder), thumbnailRect);

            var image = new DrawingImage(visual.Drawing);
            return image;
        }

        public void Draw(LayerModel layerModel, DrawingContext c)
        {
        }

        public void Update(LayerModel layerModel, ModuleDataModel dataModel, bool isPreview = false)
        {
        }

        public void SetupProperties(LayerModel layerModel)
        {
            if (layerModel.Properties is SimplePropertiesModel)
                return;

            layerModel.Properties = new SimplePropertiesModel(layerModel.Properties);
        }

        public LayerPropertiesViewModel SetupViewModel(LayerEditorViewModel layerEditorViewModel,
            LayerPropertiesViewModel layerPropertiesViewModel)
        {
            if (layerPropertiesViewModel is FolderPropertiesViewModel)
                return layerPropertiesViewModel;
            return new FolderPropertiesViewModel(layerEditorViewModel);
        }
    }
}