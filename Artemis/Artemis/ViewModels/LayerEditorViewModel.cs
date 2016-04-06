using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using Artemis.DAL;
using Artemis.KeyboardProviders;
using Artemis.Models.Profiles;
using Artemis.Utilities;
using Artemis.ViewModels.LayerEditor;
using Caliburn.Micro;

namespace Artemis.ViewModels
{
    public class LayerEditorViewModel<T> : Screen
    {
        private readonly KeyboardProvider _activeKeyboard;
        private readonly BackgroundWorker _previewWorker;
        private readonly ProfileModel _profile;
        private LayerModel _layer;
        private LayerPropertiesModel _proposedProperties;

        public LayerEditorViewModel(KeyboardProvider activeKeyboard, ProfileModel profile, LayerModel layer)
        {
            _activeKeyboard = activeKeyboard;
            _profile = profile;
            Layer = layer;

            DataModelProps = new BindableCollection<GeneralHelpers.PropertyCollection>();
            ProposedProperties = new LayerPropertiesModel();
            DataModelProps.AddRange(GeneralHelpers.GenerateTypeMap<T>());

            LayerConditionVms =
                new BindableCollection<LayerConditionViewModel<T>>(
                    layer.LayerConditions.Select(c => new LayerConditionViewModel<T>(this, c, DataModelProps)));

            _previewWorker = new BackgroundWorker();
            _previewWorker.WorkerSupportsCancellation = true;
            _previewWorker.DoWork += PreviewWorkerOnDoWork;
            _previewWorker.RunWorkerAsync();

            PreSelect();
        }

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

        public ImageSource LayerImage
        {
            get
            {
                // For the preview, put the proposed properties into the calculated properties
                _layer.LayerCalculatedProperties = ProposedProperties;
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
            GeneralHelpers.CopyProperties(ProposedProperties, Layer.LayerUserProperties);
        }

        public void AddCondition()
        {
            var condition = new LayerConditionModel();
            Layer.LayerConditions.Add(condition);
            LayerConditionVms.Add(new LayerConditionViewModel<T>(this, condition, DataModelProps));
        }

        public void Apply()
        {
            GeneralHelpers.CopyProperties(Layer.LayerUserProperties, ProposedProperties);
            ProfileProvider.AddOrUpdate(_profile);
        }

        public void DeleteCondition(LayerConditionViewModel<T> layerConditionViewModel,
            LayerConditionModel layerConditionModel)
        {
            LayerConditionVms.Remove(layerConditionViewModel);
            Layer.LayerConditions.Remove(layerConditionModel);
        }

        public override void CanClose(Action<bool> callback)
        {
            _previewWorker.CancelAsync();
            base.CanClose(callback);
        }
    }
}