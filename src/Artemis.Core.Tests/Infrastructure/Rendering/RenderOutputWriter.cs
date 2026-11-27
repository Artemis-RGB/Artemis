using System.Text;
using System.Text.Json;
using SkiaSharp;

namespace Artemis.Core.Tests.Infrastructure.Rendering;

/// <summary>
///     Writes the files of a single save to its output folder, attaches them to the test result and, once disposed, lists
///     them in the test output as clickable links.
/// </summary>
internal sealed class RenderOutputWriter : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new() {WriteIndented = true};

    private readonly string _name;
    private readonly List<string> _files = [];

    public RenderOutputWriter(string directory, string name)
    {
        Directory = directory;
        _name = name;
    }

    /// <summary>
    ///     Gets the folder the files are written to
    /// </summary>
    public string Directory { get; }

    public void WritePng(string fileName, SKBitmap bitmap)
    {
        // Favour speed over size, the default compression takes about half again as long
        using SKPixmap pixmap = bitmap.PeekPixels();
        using SKData data = pixmap.Encode(new SKPngEncoderOptions(SKPngEncoderFilterFlags.NoFilters, 1));
        Write(fileName, data.ToArray(), "image/png");
    }

    public void WriteJson(string fileName, object value)
    {
        Write(fileName, JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions), "application/json");
    }

    public void Dispose()
    {
        ITestOutputHelper? output = TestContext.Current.TestOutputHelper;
        if (output == null)
            return;

        // File URIs are clickable in most test output windows, they are long so each gets a line of its own
        StringBuilder builder = new();
        builder.AppendLine($"Render output: {_name}");
        builder.AppendLine($"  folder: {new Uri(Directory).AbsoluteUri}");
        foreach (string file in _files)
            builder.AppendLine($"  {Path.GetFileName(file)}: {new Uri(file).AbsoluteUri}");
        output.WriteLine(builder.ToString());
    }

    private void Write(string fileName, byte[] data, string mediaType)
    {
        string path = Path.Combine(Directory, fileName);
        File.WriteAllBytes(path, data);
        _files.Add(path);

        // Prefix with the output name, a test may save under multiple names
        TestContext.Current.AddAttachment($"{Path.GetFileName(Directory)}/{fileName}", data, mediaType);
    }
}
