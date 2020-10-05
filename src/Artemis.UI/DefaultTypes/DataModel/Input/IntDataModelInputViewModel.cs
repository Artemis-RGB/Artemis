using System.Text.RegularExpressions;
using System.Windows.Input;
using Artemis.Core.DataModelExpansions;
using Artemis.UI.Shared;

namespace Artemis.UI.DefaultTypes.DataModel.Input
{
    public class IntDataModelInputViewModel : DataModelInputViewModel<int>
    {
        public IntDataModelInputViewModel(DataModelPropertyAttribute targetDescription, int initialValue) : base(targetDescription, initialValue)
        {
        }

        public void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}