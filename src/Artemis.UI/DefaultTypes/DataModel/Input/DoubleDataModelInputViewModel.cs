using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Artemis.Core.DataModelExpansions;
using Artemis.UI.Shared;

namespace Artemis.UI.DefaultTypes.DataModel.Input
{
    public class DoubleDataModelInputViewModel : DataModelInputViewModel<double>
    {
        public DoubleDataModelInputViewModel(DataModelPropertyAttribute targetDescription, double initialValue) : base(targetDescription, initialValue)
        {
        }

        public void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            if (e.Text != "-")
            {
                string seperator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                Regex regex = new("^[" + seperator + "][0-9]+$|^[0-9]*[" + seperator + "]{0,1}[0-9]*$");
                e.Handled = !regex.IsMatch(e.Text);
            }
        }
    }
}