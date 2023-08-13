using System;
using System.Collections.Generic;
using System.IO;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop;
using DryIoc;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard;

public class SubmissionWizardState
{
    private readonly IContainer _container;
    private readonly IWindowService _windowService;
    private readonly SubmissionWizardViewModel _wizardViewModel;

    public SubmissionWizardState(SubmissionWizardViewModel wizardViewModel, IContainer container, IWindowService windowService)
    {
        _wizardViewModel = wizardViewModel;
        _container = container;
        _windowService = windowService;
    }

    public EntryType EntryType { get; set; }

    public string Name { get; set; } = string.Empty;
    public Stream? Icon { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public List<int> Categories { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public List<Stream> Images { get; set; } = new();

    public object? EntrySource { get; set; }

    public void ChangeScreen<TSubmissionViewModel>() where TSubmissionViewModel : SubmissionViewModel
    {
        try
        {
            _wizardViewModel.Screen = _container.Resolve<TSubmissionViewModel>();
        }
        catch (Exception e)
        {
            _windowService.ShowExceptionDialog("Wizard screen failed to activate", e);
        }
    }
}