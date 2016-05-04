using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Media;
using Artemis.KeyboardProviders;
using Artemis.Models.Profiles;
using Artemis.Services;
using Artemis.Utilities;
using Caliburn.Micro;
using Screen = Caliburn.Micro.Screen;

namespace Artemis.ViewModels.LayerEditor
{
    public class LayerEditorViewModel<T> : Screen
    {
        private readonly KeyboardProvider _activeKeyboard;
        private readonly BackgroundWorker _previewWorker;
        private readonly bool _wasEnabled;
        private LayerModel _layer;
        private LayerModel _proposedLayer;
        private LayerPropertiesModel _proposedProperties;
        private LayerType _layerType;
        private MetroDialogService _dialogService;

        public LayerEditorViewModel(KeyboardProvider activeKeyboard, LayerModel layer)
        {
            _activeKeyboard = activeKeyboard;
            _wasEnabled = layer.Enabled;

            _dialogService = new MetroDialogService(this);
            Layer = layer;
            ProposedLayer = new LayerModel();
            GeneralHelpers.CopyProperties(ProposedLayer, Layer);
            Layer.Enabled = false;
            DataModelProps = new BindableCollection<GeneralHelpers.PropertyCollection>();
            ProposedProperties = new LayerPropertiesModel();
            DataModelProps.AddRange(GeneralHelpers.GenerateTypeMap<T>());
            LayerConditionVms =
                new BindableCollection<LayerConditionViewModel<T>>(
                    layer.LayerConditions.Select(c => new LayerConditionViewModel<T>(this, c, DataModelProps)));
            HeightProperties = new LayerDynamicPropertiesViewModel("Height", DataModelProps, layer);
            WidthProperties = new LayerDynamicPropertiesViewModel("Width", DataModelProps, layer);
            OpacityProperties = new LayerDynamicPropertiesViewModel("Opacity", DataModelProps, layer);

            _previewWorker = new BackgroundWorker {WorkerSupportsCancellation = true};
            _previewWorker.DoWork += PreviewWorkerOnDoWork;
            _previewWorker.RunWorkerAsync();

            PropertyChanged += GridDisplayHandler;
            PreSelect();
        }
        
        public LayerDynamicPropertiesViewModel OpacityProperties { get; set; }

        public LayerDynamicPropertiesViewModel WidthProperties { get; set; }

        public LayerDynamicPropertiesViewModel HeightProperties { get; set; }

        public BindableCollection<GeneralHelpers.PropertyCollection> DataModelProps { get; set; }

        public BindableCollection<string> LayerTypes => new BindableCollection<string>();

        public BindableCollection<LayerConditionViewModel<T>> LayerConditionVms { get; set; }

        public LayerModel Layer
        {
            get { return _layer; }
            set
            {
                if (Equals(value, _layer)) return;
                _layer = value;
                NotifyOfPropertyChange(() => Layer);
            }
        }

        public LayerModel ProposedLayer
        {
            get { return _proposedLayer; }
            set
            {
                if (Equals(value, _proposedLayer)) return;
                _proposedLayer = value;
                NotifyOfPropertyChange(() => ProposedLayer);
            }
        }

        public LayerPropertiesModel ProposedProperties
        {
            get { return _proposedProperties; }
            set
            {
                if (Equals(value, _proposedProperties)) return;
                _proposedProperties = value;
                NotifyOfPropertyChange(() => ProposedProperties);
            }
        }

        public bool KeyboardGridIsVisible => Layer.LayerType == LayerType.Keyboard;
        public bool GifGridIsVisible => Layer.LayerType == LayerType.KeyboardGif;

        public ImageSource LayerImage
        {
            get
            {
                var keyboardRect = _activeKeyboard.KeyboardRectangle(4);

                var visual = new DrawingVisual();
                using (var drawingContext = visual.RenderOpen())
                {
                    // Setup the DrawingVisual's size
                    drawingContext.PushClip(new RectangleGeometry(keyboardRect));
                    drawingContext.DrawRectangle(new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)), null, keyboardRect);

                    // Draw the layer
                    _layer.DrawPreview(drawingContext);

                    // Remove the clip
                    drawingContext.Pop();
                }
                var image = new DrawingImage(visual.Drawing);

                return image;
            }
        }

        private void PreviewWorkerOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            while (!_previewWorker.CancellationPending)
            {
                NotifyOfPropertyChange(() => LayerImage);
                Thread.Sleep(1000/25);
            }
        }

        public void PreSelect()
        {
            GeneralHelpers.CopyProperties(ProposedProperties, Layer.UserProps);
            LayerType = Layer.LayerType;
        }

        public LayerType LayerType
        {
            get { return _layerType; }
            set
            {
                if (value == _layerType) return;
                _layerType = value;
                NotifyOfPropertyChange(() => LayerType);
            }
        }

        private void GridDisplayHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "LayerType")
                return;

            Layer.LayerType = LayerType;
            NotifyOfPropertyChange(() => KeyboardGridIsVisible);
            NotifyOfPropertyChange(() => GifGridIsVisible);
        }

        public void AddCondition()
        {
            var condition = new LayerConditionModel();
            Layer.LayerConditions.Add(condition);
            LayerConditionVms.Add(new LayerConditionViewModel<T>(this, condition, DataModelProps));
        }

        public void Apply()
        {
            GeneralHelpers.CopyProperties(Layer.UserProps, ProposedProperties);
            HeightProperties.Apply();
            WidthProperties.Apply();
            OpacityProperties.Apply();

            if (!File.Exists(Layer.GifFile) && Layer.LayerType == LayerType.KeyboardGif)
                _dialogService.ShowErrorMessageBox("Couldn't find or access the provided GIF file.");
        }

        public void DeleteCondition(LayerConditionViewModel<T> layerConditionViewModel,
            LayerConditionModel layerConditionModel)
        {
            LayerConditionVms.Remove(layerConditionViewModel);
            Layer.LayerConditions.Remove(layerConditionModel);
        }

        public void BrowseGif()
        {
            var dialog = new OpenFileDialog {Filter = "Animated image file (*.gif)|*.gif"};
            var result = dialog.ShowDialog();
            if (result == DialogResult.OK)
                Layer.GifFile = dialog.FileName;
        }

        public override void CanClose(Action<bool> callback)
        {
            _previewWorker.CancelAsync();
            _layer.Enabled = _wasEnabled;
            base.CanClose(callback);
        }
    }
}