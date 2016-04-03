using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Artemis.Models.Profiles;
using Artemis.Utilities;
using Artemis.ViewModels.LayerEditor;
using Caliburn.Micro;
using Color = System.Windows.Media.Color;

namespace Artemis.ViewModels
{
    public class LayerEditorViewModel<T> : Screen
    {
        private readonly BackgroundWorker _previewWorker;
        private LayerModel _layer;
        private LayerPropertiesModel _proposedProperties;

        public LayerEditorViewModel(LayerModel layer)
        {
            Layer = layer;

            DataModelProps = new BindableCollection<GeneralHelpers.PropertyCollection>();
            ProposedColors = new BindableCollection<Color>();
            ProposedProperties = new LayerPropertiesModel();
            ProposedColors.CollectionChanged += UpdateColors;
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

        public BindableCollection<Color> ProposedColors { get; set; }

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
                var bitmap = new Bitmap(ProposedProperties.Width*4, ProposedProperties.Height*4);

                using (var g = Graphics.FromImage(bitmap))
                {
                    _layer.DrawPreview(g);
                }

                using (var memory = new MemoryStream())
                {
                    bitmap.Save(memory, ImageFormat.Png);
                    memory.Position = 0;

                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();

                    return bitmapImage;
                }
            }
        }

        private void UpdateColors(object sender, NotifyCollectionChangedEventArgs e)
        {
            ProposedProperties.Colors = ProposedColors.ToList();
        }

        private void PreviewWorkerOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            while (!_previewWorker.CancellationPending)
            {
                NotifyOfPropertyChange(() => LayerImage);
                Thread.Sleep(1000/25);
            }
        }

        private void PreSelect()
        {
            GeneralHelpers.CopyProperties(ProposedProperties, Layer.LayerUserProperties);
            ProposedColors.Clear();
            ProposedColors.AddRange(ProposedProperties.Colors);
        }

        public void AddCondition()
        {
            var condition = new LayerConditionModel();
            Layer.LayerConditions.Add(condition);
            LayerConditionVms.Add(new LayerConditionViewModel<T>(this, condition, DataModelProps));
        }

        public void AddColor()
        {
            ProposedColors.Add(ColorHelpers.ToMediaColor(ColorHelpers.GetRandomRainbowColor()));
        }

        public void DeleteColor(Color c)
        {
            ProposedColors.Remove(c);
        }

        public void Apply()
        {
            GeneralHelpers.CopyProperties(Layer.LayerUserProperties, ProposedProperties);
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