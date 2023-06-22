using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Screens.Workshop.CurrentUser;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using Artemis.WebClient.Workshop;
using Avalonia.Input;
using ReactiveUI;
using SkiaSharp;
using StrawberryShake;

namespace Artemis.UI.Screens.Workshop;

public class WorkshopViewModel : MainScreenViewModel
{
    private readonly IWorkshopClient _workshopClient;

    public WorkshopViewModel(IScreen hostScreen, IWorkshopClient workshopClient, CurrentUserViewModel currentUserViewModel) : base(hostScreen, "workshop")
    {
        CurrentUserViewModel = currentUserViewModel;
        _workshopClient = workshopClient;
        DisplayName = "Workshop";

        Task.Run(() => GetEntries());
    }

    public ObservableCollection<IGetEntries_Entries_Nodes> Test { get; set; } = new();
    public CurrentUserViewModel CurrentUserViewModel { get; set; }
    
    private async Task GetEntries()
    {

        try
        {
            IOperationResult<IGetEntriesResult> entries = await _workshopClient.GetEntries.ExecuteAsync();
            if (entries.Data?.Entries?.Nodes == null)
                return;
        
            foreach (IGetEntries_Entries_Nodes getEntriesEntriesNodes in entries.Data.Entries.Nodes)
            {
                Console.WriteLine(getEntriesEntriesNodes);
                Test.Add(getEntriesEntriesNodes);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}