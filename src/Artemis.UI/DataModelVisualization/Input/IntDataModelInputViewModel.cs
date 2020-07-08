using System.Text.RegularExpressions;
using System.Windows.Input;
using Artemis.Core.Plugins.Abstract.DataModels.Attributes;
using Artemis.UI.Shared.DataModelVisualization;

namespace Artemis.UI.DataModelVisualization.Input
{
    public class IntDataModelInputViewModel : DataModelInputViewModel<int>
    {
        public IntDataModelInputViewModel(DataModelPropertyAttribute description, int initialValue) : base(description, initialValue)
        {
        }

        public void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}