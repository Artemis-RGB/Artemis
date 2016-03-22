using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Artemis.Models.Interfaces;
using Artemis.Properties;
using Newtonsoft.Json;
using Color = System.Drawing.Color;
using Pen = System.Drawing.Pen;

namespace Artemis.Models.Profiles
{
    public class LayerModel
    {
        public LayerModel(string name, LayerType layerType)
        {
            Name = name;
            LayerType = layerType;
            LayerUserProperties = new LayerPropertiesModel();
            LayerCalculatedProperties = new LayerPropertiesModel();

            Children = new List<LayerModel>();
            LayerConditions = new List<LayerConditionModel>();
            LayerProperties = new List<LayerDynamicPropertiesModel>();
        }

        public string Name { get; set; }
        public LayerType LayerType { get; set; }
        public LayerPropertiesModel LayerUserProperties { get; set; }

        [JsonIgnore]
        public LayerPropertiesModel LayerCalculatedProperties { get; }

        public List<LayerModel> Children { get; set; }
        public List<LayerConditionModel> LayerConditions { get; set; }
        public List<LayerDynamicPropertiesModel> LayerProperties { get; set; }
        public ImageSource LayerImage => GetPreviewImage();

        private BitmapImage GetPreviewImage()
        {
            var bitmap = new Bitmap(18, 18);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                if (LayerType == LayerType.Ellipse)
                {
                    g.FillEllipse(new SolidBrush(LayerUserProperties.Colors.FirstOrDefault()), 0, 0, 18, 18);
                    g.DrawEllipse(new Pen(Color.Black, 1), 0, 0, 17, 17);
                }
                else if (LayerType == LayerType.Rectangle)
                {
                    g.FillRectangle(new SolidBrush(LayerUserProperties.Colors.FirstOrDefault()), 0, 0, 18, 18);
                    g.DrawRectangle(new Pen(Color.Black, 1), 0, 0, 17, 17);
                }
                else
                {
                    bitmap = Resources.folder;
                }
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

        public bool ConditionsMet<T>(IGameDataModel dataModel)
        {
            return LayerConditions.All(cm => cm.ConditionMet<T>(dataModel));
        }

        public void Draw<T>(IGameDataModel dataModel, Graphics g)
        {
            if (!ConditionsMet<T>(dataModel))
                return;

            Update<T>(dataModel);
            switch (LayerType)
            {
                case LayerType.Folder:
                    DrawChildren<T>(dataModel, g);
                    break;
                case LayerType.Rectangle:
                    DrawRectangle(g);
                    break;
                case LayerType.Ellipse:
                    DrawEllipse(g);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Update<T>(IGameDataModel dataModel)
        {
            foreach (var dynamicProperty in LayerProperties)
                dynamicProperty.ApplyProperty<T>(dataModel, LayerUserProperties, LayerCalculatedProperties);
        }

        private void DrawChildren<T>(IGameDataModel dataModel, Graphics g)
        {
            foreach (var layerModel in Children)
                layerModel.Draw<T>(dataModel, g);
        }

        private void DrawRectangle(Graphics g)
        {
        }

        private void DrawEllipse(Graphics g)
        {
        }
    }

    public enum LayerType
    {
        Folder,
        Rectangle,
        Ellipse
    }
}