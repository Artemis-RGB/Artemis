using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Artemis.UI.Shared;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Extensions;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using StrawberryShake;

namespace Artemis.UI.Screens.Workshop.Categories;

public class CategoriesViewModel : ActivatableViewModelBase
{
    private ObservableAsPropertyHelper<IReadOnlyList<EntryFilterInput>?>? _categoryFilters;

    public CategoriesViewModel(IWorkshopClient client)
    {
        client.GetCategories
            .Watch(ExecutionStrategy.CacheFirst)
            .SelectOperationResult(c => c.Categories)
            .ToObservableChangeSet(c => c.Id)
            .Transform(c => new CategoryViewModel(c))
            .Bind(out ReadOnlyObservableCollection<CategoryViewModel> categoryViewModels)
            .Subscribe();

        Categories = categoryViewModels;

        this.WhenActivated(d =>
        {
            _categoryFilters = Categories.ToObservableChangeSet()
                .AutoRefresh(c => c.IsSelected)
                .Filter(e => e.IsSelected)
                .Select(_ => CreateFilter())
                .ToProperty(this, vm => vm.CategoryFilters)
                .DisposeWith(d);
        });
    }

    public ReadOnlyObservableCollection<CategoryViewModel> Categories { get; }
    public IReadOnlyList<EntryFilterInput>? CategoryFilters => _categoryFilters?.Value;

    private IReadOnlyList<EntryFilterInput>? CreateFilter()
    {
        List<long?> categories = Categories.Where(c => c.IsSelected).Select(c => (long?) c.Id).ToList();
        if (!categories.Any())
            return null;

        List<EntryFilterInput> categoryFilters = new();
        foreach (long? category in categories)
        {
            categoryFilters.Add(new EntryFilterInput
            {
                Categories = new ListFilterInputTypeOfCategoryFilterInput {Some = new CategoryFilterInput {Id = new LongOperationFilterInput {Eq = category}}}
            });
        }

        return categoryFilters;
    }
}