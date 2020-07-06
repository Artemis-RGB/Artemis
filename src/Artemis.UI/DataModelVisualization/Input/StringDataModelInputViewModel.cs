using System;
using System.Collections.Generic;
using System.Text;
using Artemis.Core.Plugins.Abstract.DataModels.Attributes;
using Artemis.UI.Shared.DataModelVisualization;

namespace Artemis.UI.DataModelVisualization.Input
{
    public class StringDataModelInputViewModel : DataModelInputViewModel<string>
    {
        public StringDataModelInputViewModel(DataModelPropertyAttribute description, string initialValue) : base(description, initialValue)
        {
        }
    }
}