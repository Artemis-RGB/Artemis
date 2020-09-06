using System.Text.RegularExpressions;
using System.Windows.Input;
using Artemis.Core.DataModelExpansions;
using Artemis.UI.Shared;

namespace Artemis.UI.DataModelVisualization.Input
{
    public class IntDataModelInputViewModel : DataModelInputViewModel<int>
    {
        public IntDataModelInputViewModel(DataModelPropertyAttribute targetDescription, int initialValue) : base(targetDescription, initialValue)
        {
        }

        public void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}