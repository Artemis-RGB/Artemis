using System.IO;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Windows.Input;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Image;

public partial class ImageSubmissionViewModel : ActivatableViewModelBase
{
    [Notify(Setter.Private)] private Bitmap? _bitmap;
    [Notify(Setter.Private)] private string? _fileName;
    [Notify(Setter.Private)] private string? _imageDimensions;
    [Notify(Setter.Private)] private long _fileSize;
    [Notify] private ICommand? _remove;

    public ImageSubmissionViewModel(Stream imageStream)
    {
        this.WhenActivated(d =>
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                imageStream.Seek(0, SeekOrigin.Begin);
                Bitmap = new Bitmap(imageStream);
                FileSize = imageStream.Length;
                ImageDimensions = Bitmap.Size.Width + "x" + Bitmap.Size.Height;

                if (imageStream is FileStream fileStream)
                    FileName = Path.GetFileName(fileStream.Name);
                else
                    FileName = "Unnamed image";

                Bitmap.DisposeWith(d);
            }, DispatcherPriority.Background);
        });
    }
}