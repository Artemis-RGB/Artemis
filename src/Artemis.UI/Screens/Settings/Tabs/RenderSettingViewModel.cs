namespace Artemis.UI.Screens.Settings;

public class RenderSettingViewModel
{
    public RenderSettingViewModel(string display, double value)
    {
        Display = display;
        Value = value;
    }

    public string Display { get; }
    public double Value { get; }
}