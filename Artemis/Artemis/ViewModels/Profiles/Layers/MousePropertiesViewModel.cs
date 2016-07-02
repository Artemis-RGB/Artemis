﻿using System.Collections.Generic;
using System.Windows.Media;
using Artemis.Models.Interfaces;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;
using Artemis.Utilities;
using Caliburn.Micro;

namespace Artemis.ViewModels.Profiles.Layers
{
    public class MousePropertiesViewModel : LayerPropertiesViewModel
    {
        private Brush _brush;
        private LayerPropertiesModel _proposedProperties;
        private ILayerAnimation _selectedLayerAnimation;

        public MousePropertiesViewModel(IEnumerable<ILayerAnimation> layerAnimations, IDataModel dataModel, LayerPropertiesModel properties)
            : base(dataModel)
        {
            ProposedProperties = GeneralHelpers.Clone(properties);
            Brush = ProposedProperties.Brush.CloneCurrentValue();

            LayerAnimations = new BindableCollection<ILayerAnimation>(layerAnimations);
        }

        public BindableCollection<ILayerAnimation> LayerAnimations { get; set; }

        public ILayerAnimation SelectedLayerAnimation
        {
            get { return _selectedLayerAnimation; }
            set
            {
                if (Equals(value, _selectedLayerAnimation)) return;
                _selectedLayerAnimation = value;
                NotifyOfPropertyChange(() => SelectedLayerAnimation);
            }
        }

        public Brush Brush
        {
            get { return _brush; }
            set
            {
                if (Equals(value, _brush)) return;
                _brush = value;
                NotifyOfPropertyChange(() => Brush);
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

        public override LayerPropertiesModel GetAppliedProperties()
        {
            var properties = GeneralHelpers.Clone(ProposedProperties);
            properties.Brush = Brush;
            return properties;
        }
    }
}