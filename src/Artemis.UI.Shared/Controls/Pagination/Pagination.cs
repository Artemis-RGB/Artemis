using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using ReactiveUI;

namespace Artemis.UI.Shared.Pagination;

/// <summary>
/// Represents a pagination control that can be used to switch between pages.
/// </summary>
[TemplatePart("PART_PreviousButton", typeof(Button))]
[TemplatePart("PART_NextButton", typeof(Button))]
[TemplatePart("PART_PagesView", typeof(StackPanel))]
public partial class Pagination : TemplatedControl
{
    private Button? _previousButton;
    private Button? _nextButton;
    private StackPanel? _pagesView;

    /// <inheritdoc />
    public Pagination()
    {
        PropertyChanged += OnPropertyChanged;
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_previousButton != null)
            _previousButton.Click -= PreviousButtonOnClick;
        if (_nextButton != null)
            _nextButton.Click -= NextButtonOnClick;

        _previousButton = e.NameScope.Find<Button>("PART_PreviousButton");
        _nextButton = e.NameScope.Find<Button>("PART_NextButton");
        _pagesView = e.NameScope.Find<StackPanel>("PART_PagesView");

        if (_previousButton != null)
            _previousButton.Click += PreviousButtonOnClick;
        if (_nextButton != null)
            _nextButton.Click += NextButtonOnClick;

        Update();
    }

    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ValueProperty || e.Property == MaximumProperty)
            Update();
    }

    private void NextButtonOnClick(object? sender, RoutedEventArgs e)
    {
        if (Value < Maximum)
            Value++;
    }

    private void PreviousButtonOnClick(object? sender, RoutedEventArgs e)
    {
        if (Value > 1)
            Value--;
    }

    private void Update()
    {
        if (_pagesView == null)
            return;

        List<int> pages = GetPages(Value, Maximum);

        // Remove extra children
        while (_pagesView.Children.Count > pages.Count)
        {
            _pagesView.Children.RemoveAt(_pagesView.Children.Count - 1);
        }

        if (_pagesView.Children.Count > pages.Count)
            _pagesView.Children.RemoveRange(0, _pagesView.Children.Count - pages.Count);

        // Add/modify children
        for (int i = 0; i < pages.Count; i++)
        {
            int page = pages[i];

            // -1 indicates an ellipsis (...)
            if (page == -1)
            {
                if (_pagesView.Children.ElementAtOrDefault(i) is not PaginationEllipsis)
                {
                    if (_pagesView.Children.Count - 1 >= i)
                        _pagesView.Children[i] = new PaginationEllipsis();
                    else
                        _pagesView.Children.Add(new PaginationEllipsis());
                }
            }
            // Anything else indicates a regular page
            else
            {
                if (_pagesView.Children.ElementAtOrDefault(i) is PaginationPage paginationPage)
                {
                    paginationPage.Page = page;
                    paginationPage.Command = ReactiveCommand.Create(() => Value = page);
                    continue;
                }

                paginationPage = new PaginationPage {Page = page, Command = ReactiveCommand.Create(() => Value = page)};
                if (_pagesView.Children.Count - 1 >= i)
                    _pagesView.Children[i] = paginationPage;
                else
                    _pagesView.Children.Add(paginationPage);
            }
        }

        foreach (Control child in _pagesView.Children)
        {
            if (child is PaginationPage paginationPage)
                ((IPseudoClasses) paginationPage.Classes).Set(":selected", paginationPage.Page == Value);
        }
    }

    private void PaginationPageOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is PaginationPage paginationPage)
            Value = paginationPage.Page;
    }

    private static List<int> GetPages(int currentPage, int pageCount)
    {
        // Determine the delta based on how close to the edge the current page is
        int delta;
        if (pageCount <= 7)
            delta = 7;
        else
            delta = currentPage > 4 && currentPage < pageCount - 3 ? 2 : 4;

        int start = currentPage - delta / 2;
        int end = currentPage + delta / 2;

        if (start - 1 == 1 || end + 1 == pageCount)
        {
            start += 1;
            end += 1;
        }

        // Determine start and end numbers based on how close to the edge the current page is
        start = currentPage > delta ? Math.Min(start, pageCount - delta) : 1;
        end = currentPage > delta ? Math.Min(end, pageCount) : Math.Min(pageCount, delta + 1);

        // Start with the pages neighbouring the current page
        List<int> paginationItems = Enumerable.Range(start, end - start + 1).ToList();

        // If not starting at the first page, add the first page and an ellipsis (-1)
        if (paginationItems.First() != 1)
        {
            paginationItems.Insert(0, 1);
            paginationItems.Insert(1, -1);
        }

        // If not ending at the last page, add an ellipsis (-1) and the last page
        if (paginationItems.Last() < pageCount)
        {
            paginationItems.Add(-1);
            paginationItems.Add(pageCount);
        }

        return paginationItems;
    }
}