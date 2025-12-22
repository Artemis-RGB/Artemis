using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using FluentAvalonia.UI.Controls;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Entries.Details;

public partial class EntryImagesDialogViewModel : ContentDialogViewModelBase
{
    [Notify] private EntryImageViewModel _currentImage;
    private readonly IInputService _inputService;

    public EntryImagesDialogViewModel(ObservableCollection<EntryImageViewModel> images, EntryImageViewModel startImage, IInputService inputService)
    {
        _currentImage = startImage;
        _inputService = inputService;

        Images = images;
        Previous = ReactiveCommand.Create(() => CurrentImage = Images[(Images.IndexOf(CurrentImage) - 1 + Images.Count) % Images.Count]);
        Next = ReactiveCommand.Create(() => CurrentImage = Images[(Images.IndexOf(CurrentImage) + 1) % Images.Count]);

        this.WhenActivated(d =>
        {
            if (ContentDialog == null)
                return;

            _inputService.KeyboardKeyDown += InputServiceOnKeyboardKeyDown;
            ContentDialog.Closing += ContentDialogOnClosing;
            Disposable.Create(() =>
            {
                _inputService.KeyboardKeyDown -= InputServiceOnKeyboardKeyDown;
                ContentDialog.Closing -= ContentDialogOnClosing;
            }).DisposeWith(d);
        });
    }

    private void InputServiceOnKeyboardKeyDown(object? sender, ArtemisKeyboardKeyEventArgs e)
    {
        // Leveraging InputService to avoid issues with which control has focus
        if (e.Key == KeyboardKey.ArrowRight)
            Next.Execute().Subscribe();
        else if (e.Key == KeyboardKey.ArrowLeft)
            Previous.Execute().Subscribe();
        else if (e.Key == KeyboardKey.Escape)
            ContentDialog?.Hide(ContentDialogResult.None);
    }

    private void ContentDialogOnClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
    {
        args.Cancel = args.Result != ContentDialogResult.None;
    }

    public ObservableCollection<EntryImageViewModel> Images { get; }
    public ReactiveCommand<Unit, EntryImageViewModel> Previous { get; }
    public ReactiveCommand<Unit, EntryImageViewModel> Next { get; }
}