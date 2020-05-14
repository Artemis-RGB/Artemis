using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Artemis.Core.Models.Profile;
using Artemis.UI.Exceptions;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Abstract;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree.PropertyInput;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Tree.PropertyInput;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Tree
{
    public class TreePropertyViewModel<T> : TreePropertyViewModel
    {
        public TreePropertyViewModel(LayerPropertyBaseViewModel layerPropertyBaseViewModel) : base(layerPropertyBaseViewModel)
        {
            LayerPropertyViewModel = (LayerPropertyViewModel<T>) layerPropertyBaseViewModel;
            PropertyInputViewModel = CreatePropertyInputViewModel();
        }

        public LayerPropertyViewModel<T> LayerPropertyViewModel { get; }
        public PropertyInputViewModel<T> PropertyInputViewModel { get; set; }

        public override void Dispose()
        {
            PropertyInputViewModel.Dispose();
        }

        private PropertyInputViewModel<T> CreatePropertyInputViewModel()
        {
            if (!IsPropertySupported(typeof(T)))
                throw new ArtemisUIException($"Failed to create a property input view model, type {typeof(T).Name} is not supported.");

            return (PropertyInputViewModel<T>) Activator.CreateInstance(SupportedTypes[typeof(T)], this);
        }
    }

    public abstract class TreePropertyViewModel : IDisposable
    {
        public static ReadOnlyDictionary<Type, Type> SupportedTypes = new ReadOnlyDictionary<Type, Type>(new Dictionary<Type, Type>
        {
            {typeof(LayerBrushReference), typeof(BrushPropertyInputViewModel)},
            {typeof(LayerBrushReference), typeof(BrushPropertyInputViewModel)},
            {typeof(LayerBrushReference), typeof(BrushPropertyInputViewModel)},
            {typeof(LayerBrushReference), typeof(BrushPropertyInputViewModel)}
        });

        public static void RegisterPropertyInputViewModel()
        {

        }

        public static bool IsPropertySupported(Type type)
        {
            return SupportedTypes.ContainsKey(type);
        }

        protected TreePropertyViewModel(LayerPropertyBaseViewModel layerPropertyBaseViewModel)
        {
            LayerPropertyBaseViewModel = layerPropertyBaseViewModel;
        }

        public LayerPropertyBaseViewModel LayerPropertyBaseViewModel { get; }
        public abstract void Dispose();
    }
}