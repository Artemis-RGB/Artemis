using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using RGB.NET.Core;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree.Dialogs.AdaptionHints;

public partial class SingleLedAdaptionHintViewModel : AdaptionHintViewModelBase
{
    [Notify] private List<AdaptionLed> _adaptionLeds = new();
    [Notify] private AdaptionLed? _selectedLed;

    public SingleLedAdaptionHintViewModel(Layer layer, SingleLedAdaptionHint adaptionHint) : base(layer, adaptionHint)
    {
        SingleLedAdaptionHint = adaptionHint;

        this.WhenAnyValue(vm => vm.SelectedLed).WhereNotNull().Subscribe(l => SingleLedAdaptionHint.LedId = l.Value);
        Task.Run(() =>
        {
            AdaptionLeds = Enum.GetValues<LedId>().Select(l => new AdaptionLed(l)).ToList();
            SelectedLed = AdaptionLeds.FirstOrDefault(l => l.Value == adaptionHint.LedId);
        });
    }

    public SingleLedAdaptionHint SingleLedAdaptionHint { get; }
}

public class AdaptionLed
{
    public AdaptionLed(LedId led)
    {
        Value = led;
        Description = led.ToString();
    }

    public LedId Value { get; }
    public string Description { get; }
}