using System;
using System.Collections.Generic;
using Artemis.Core.Models.Profile.Conditions.Abstract;
using Artemis.UI.Shared.DataModelVisualization.Shared;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.DisplayConditions.Abstract
{
    public abstract class DisplayConditionViewModel : PropertyChangedBase, IDisposable
    {
        protected DisplayConditionViewModel(DisplayConditionPart model, DisplayConditionViewModel parent)
        {
            Model = model;
            Parent = parent;
            Children = new BindableCollection<DisplayConditionViewModel>();
        }

        public DisplayConditionPart Model { get; }
        public DisplayConditionViewModel Parent { get; set; }
        public BindableCollection<DisplayConditionViewModel> Children { get; }

        public void Dispose()
        {
            foreach (var child in Children)
                child.Dispose();

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public abstract void Update();

        public virtual DataModelPropertiesViewModel GetDataModelOverride()
        {
            return Parent?.GetDataModelOverride();
        }

        public virtual void Delete()
        {
            Model.Parent.RemoveChild(Model);
            Parent.Update();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }
    }
}