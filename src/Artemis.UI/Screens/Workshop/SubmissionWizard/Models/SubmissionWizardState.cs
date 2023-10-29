using System;
using System.Collections.Generic;
using System.IO;
using Artemis.UI.Screens.Workshop.SubmissionWizard.Steps.Layout;
using Artemis.UI.Screens.Workshop.SubmissionWizard.Steps.Profile;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop;
using DryIoc;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard.Models;

public class SubmissionWizardState : IDisposable
{
    private readonly IContainer _container;
    private readonly IWindowService _windowService;
    private readonly IWorkshopWizardViewModel _wizardViewModel;

    public SubmissionWizardState(IWorkshopWizardViewModel wizardViewModel, IContainer container, IWindowService windowService)
    {
        _wizardViewModel = wizardViewModel;
        _container = container;
        _windowService = windowService;
    }

    public EntryType EntryType { get; set; }
    public long? EntryId { get; set; }

    public string Name { get; set; } = string.Empty;
    public Stream? Icon { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public List<long> Categories { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public List<Stream> Images { get; set; } = new();

    public IEntrySource? EntrySource { get; set; }

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

    public void Close()
    {
        _wizardViewModel.ShouldClose = true;
    }

    public void StartForCurrentEntry()
    {
        if (EntryType == EntryType.Profile)
            ChangeScreen<ProfileSelectionStepViewModel>();
        else if (EntryType == EntryType.Layout)
            ChangeScreen<LayoutSelectionStepViewModel>();
        else
            throw new NotImplementedException();
    }

    public void Dispose()
    {
        Icon?.Dispose();
        foreach (Stream stream in Images)
            stream.Dispose();
    }
}