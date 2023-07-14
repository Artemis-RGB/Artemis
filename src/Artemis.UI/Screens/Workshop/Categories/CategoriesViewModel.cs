using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Shared;
using Artemis.WebClient.Workshop;
using DynamicData;
using ReactiveUI;
using Serilog;
using StrawberryShake;

namespace Artemis.UI.Screens.Workshop.Categories;

public class CategoriesViewModel : ActivatableViewModelBase
{
    private readonly IWorkshopClient _client;
    private readonly ILogger _logger;
    public readonly SourceList<CategoryViewModel> _categories;

    public CategoriesViewModel(ILogger logger, IWorkshopClient client)
    {
        _logger = logger;
        _client = client;
        _categories = new SourceList<CategoryViewModel>();
        _categories.Connect().Bind(out ReadOnlyObservableCollection<CategoryViewModel> categoryViewModels).Subscribe();
        
        Categories = categoryViewModels;
        this.WhenActivated(d => ReactiveCommand.CreateFromTask(GetCategories).Execute().Subscribe().DisposeWith(d));
    }

    public ReadOnlyObservableCollection<CategoryViewModel> Categories { get; }


    private async Task GetCategories(CancellationToken cancellationToken)
    {
        try
        {
            IOperationResult<IGetCategoriesResult> result = await _client.GetCategories.ExecuteAsync(cancellationToken);
            if (result.IsErrorResult())
                _logger.Warning("Failed to retrieve categories {Error}", result.Errors);

            _categories.Edit(l =>
            {
                l.Clear();
                if (result.Data?.Categories != null)
                    l.AddRange(result.Data.Categories.Select(c => new CategoryViewModel(c)));
            });
        }
        catch (Exception e)
        {
            _logger.Warning(e, "Failed to retrieve categories");
        }
    }
}