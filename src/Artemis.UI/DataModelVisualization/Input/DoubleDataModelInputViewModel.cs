using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Artemis.Core.DataModelExpansions;
using Artemis.UI.Shared;

namespace Artemis.UI.DataModelVisualization.Input
{
    public class DoubleDataModelInputViewModel : DataModelInputViewModel<double>
    {
        public DoubleDataModelInputViewModel(DataModelPropertyAttribute description, double initialValue) : base(description, initialValue)
        {
        }

        public void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var seperator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            var regex = new Regex("^[" + seperator + "][0-9]+$|^[0-9]*[" + seperator + "]{0,1}[0-9]*$");
            e.Handled = !regex.IsMatch(e.Text);
        }
    }
}