using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Shared;
using Artemis.WebClient.Workshop;
using ReactiveUI;
using Serilog;
using StrawberryShake;

namespace Artemis.UI.Screens.Workshop.Categories;

public class CategoriesViewModel : ActivatableViewModelBase
{
    private readonly IWorkshopClient _client;
    private readonly ILogger _logger;
    private IReadOnlyList<CategoryViewModel> _categories;

    public CategoriesViewModel(ILogger logger, IWorkshopClient client)
    {
        _logger = logger;
        _client = client;

        this.WhenActivated(d => ReactiveCommand.CreateFromTask(GetCategories).Execute().Subscribe().DisposeWith(d));
    }

    public IReadOnlyList<CategoryViewModel> Categories
    {
        get => _categories;
        set => RaiseAndSetIfChanged(ref _categories, value);
    }

    private async Task GetCategories(CancellationToken cancellationToken)
    {
        try
        {
            IOperationResult<IGetCategoriesResult> result = await _client.GetCategories.ExecuteAsync(cancellationToken);
            if (result.IsErrorResult())
                _logger.Warning("Failed to retrieve categories {Error}", result.Errors);

            Categories = result.Data?.Categories.Select(c => new CategoryViewModel(c)).ToList() ?? new List<CategoryViewModel>();
        }
        catch (Exception e)
        {
            _logger.Warning(e, "Failed to retrieve categories");
        }
    }
}