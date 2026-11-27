namespace Artemis.Core.Tests.Utilities;

public class StreamExtensionsTests
{
    [Fact]
    public async Task CopyToAsync_CopiesEverythingAndReportsProgress()
    {
        byte[] data = Enumerable.Range(0, 10).Select(i => (byte) i).ToArray();
        using MemoryStream source = new(data);
        using MemoryStream destination = new();
        RecordedProgress progress = new();

        await source.CopyToAsync(10, destination, 4, progress, TestContext.Current.CancellationToken);

        destination.ToArray().ShouldBe(data);
        // The final total is reported twice, once for the last block and once when done
        progress.Reports.ShouldBe([(4L, 10L), (8L, 10L), (10L, 10L), (10L, 10L)]);
    }

    [Fact]
    public async Task CopyToAsync_WithoutALengthReportsZeroAsTheLength()
    {
        using MemoryStream source = new(new byte[5]);
        using MemoryStream destination = new();
        RecordedProgress progress = new();

        await source.CopyToAsync(destination, progress, TestContext.Current.CancellationToken);

        progress.Reports.ShouldBe([(5L, 0L), (5L, 0L)]);
    }

    [Fact]
    public async Task CopyToAsync_UsesALargeBufferByDefault()
    {
        using MemoryStream source = new(new byte[100_000]);
        using MemoryStream destination = new();
        RecordedProgress progress = new();

        await source.CopyToAsync(100_000, destination, progress, TestContext.Current.CancellationToken);

        progress.Reports.First().ShouldBe((81920L, 100_000L));
        destination.Length.ShouldBe(100_000);
    }

    [Fact]
    public async Task CopyToAsync_OfAnEmptyStreamReportsOnce()
    {
        using MemoryStream source = new();
        using MemoryStream destination = new();
        RecordedProgress progress = new();

        await source.CopyToAsync(destination, progress, TestContext.Current.CancellationToken);

        progress.Reports.ShouldBe([(0L, 0L)]);
    }

    [Fact]
    public async Task CopyToAsync_ChecksTheStreams()
    {
        using MemoryStream readable = new(new byte[1]);
        using MemoryStream readOnly = new(new byte[1], false);
        using MemoryStream closed = new();
        await closed.DisposeAsync();
        RecordedProgress progress = new();

        (await Should.ThrowAsync<ArgumentNullException>(() => ((Stream) null!).CopyToAsync(1, readable, progress))).ParamName.ShouldBe("source");
        (await Should.ThrowAsync<ArgumentException>(() => closed.CopyToAsync(1, readable, progress))).ParamName.ShouldBe("source");
        (await Should.ThrowAsync<ArgumentNullException>(() => readable.CopyToAsync(1, null!, progress))).ParamName.ShouldBe("destination");
        (await Should.ThrowAsync<ArgumentException>(() => readable.CopyToAsync(1, readOnly, progress))).ParamName.ShouldBe("destination");
    }

    [Fact]
    public async Task CopyToAsync_StopsWhenCancelled()
    {
        using MemoryStream source = new(new byte[10]);
        using MemoryStream destination = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();

        await Should.ThrowAsync<OperationCanceledException>(() => source.CopyToAsync(10, destination, new RecordedProgress(), cancellation.Token));
        destination.Length.ShouldBe(0);
    }

    /// <summary>
    ///     Records progress as it is reported, unlike <see cref="Progress{T}" /> which reports on another thread
    /// </summary>
    private class RecordedProgress : IProgress<(long, long)>
    {
        public List<(long, long)> Reports { get; } = [];

        public void Report((long, long) value)
        {
            Reports.Add(value);
        }
    }
}
