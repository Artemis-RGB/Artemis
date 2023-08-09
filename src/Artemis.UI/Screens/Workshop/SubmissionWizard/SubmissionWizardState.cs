using System.Collections.Generic;
using System.IO;
using Artemis.WebClient.Workshop;
using DryIoc;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard;

public class SubmissionWizardState
{
    private readonly SubmissionWizardViewModel _wizardViewModel;
    private readonly IContainer _container;

    public SubmissionWizardState(SubmissionWizardViewModel wizardViewModel, IContainer container)
    {
        _wizardViewModel = wizardViewModel;
        _container = container;
    }

    public EntryType EntryType { get; set; }

    public string Name { get; set; } = string.Empty;
    public Stream? Icon { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public List<int> Categories { get; set; } = new();
    public List<int> Tags { get; set; } = new();
    public List<Stream> Images { get; set; } = new();

    public void ChangeScreen<TSubmissionViewModel>() where TSubmissionViewModel : SubmissionViewModel
    {
        _wizardViewModel.Screen = _container.Resolve<TSubmissionViewModel>();
    }
}