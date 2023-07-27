using System;
using Artemis.UI.Shared;
using Artemis.WebClient.Workshop;
using Material.Icons;

namespace Artemis.UI.Screens.Workshop.Categories;

public class CategoryViewModel : ViewModelBase
{
    private bool _isSelected;

    public CategoryViewModel(IGetCategories_Categories category)
    {
        Id = category.Id;
        Name = category.Name;
        if (Enum.TryParse(typeof(MaterialIconKind), category.Icon, out object? icon))
            Icon = icon as MaterialIconKind? ?? MaterialIconKind.QuestionMarkCircle;
    }

    public int Id { get; }
    public string Name { get; }
    public MaterialIconKind Icon { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set => RaiseAndSetIfChanged(ref _isSelected, value);
    }
}