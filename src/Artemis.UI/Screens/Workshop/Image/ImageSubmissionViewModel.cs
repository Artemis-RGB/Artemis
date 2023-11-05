using System.IO;
using System.Reactive.Disposables;
using System.Windows.Input;
using Artemis.UI.Shared;
using Artemis.WebClient.Workshop.Handlers.UploadHandlers;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace Artemis.UI.Screens.Workshop.Image;

public partial class ImageSubmissionViewModel : ValidatableViewModelBase
{
    [Notify(Setter.Private)] private Bitmap? _bitmap;
    [Notify(Setter.Private)] private string? _imageDimensions;
    [Notify(Setter.Private)] private long _fileSize;
    [Notify] private string? _name;
    [Notify] private string? _description;
    [Notify] private ICommand? _remove;

    public ImageSubmissionViewModel(ImageUploadRequest image)
    {
        this.WhenActivated(d =>
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                image.File.Seek(0, SeekOrigin.Begin);
                Bitmap = new Bitmap(image.File);
                FileSize = image.File.Length;
                ImageDimensions = Bitmap.Size.Width + "x" + Bitmap.Size.Height;
                Name = image.Name;
                Description = image.Description;

                Bitmap.DisposeWith(d);
            }, DispatcherPriority.Background);
        });
        
        this.ValidationRule(vm => vm.Name, input => !string.IsNullOrWhiteSpace(input), "Name is required");
        this.ValidationRule(vm => vm.Name, input => input?.Length <= 50, "Name can be a maximum of 50 characters");
        this.ValidationRule(vm => vm.Description, input => input?.Length <= 150, "Description can be a maximum of 150 characters");
    }
}