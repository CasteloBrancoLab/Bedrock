using System.Diagnostics.Tracing;

namespace Bedrock.BuildingBlocks.Testing.Benchmarks;

/// <summary>
/// Listens to the <c>System.Net.Sockets</c> EventSource to capture cumulative
/// network bytes sent and received by the current process.
/// <para>
/// Uses <see cref="EventListener"/> to subscribe to the <c>bytes-sent</c> and
/// <c>bytes-received</c> <see cref="PollingCounter"/>s, which report the total
/// accumulated bytes via the <c>Mean</c> payload field.
/// These counters are updated by the .NET runtime for all TCP socket operations
/// (including Npgsql, HttpClient, etc.).
/// </para>
/// <para>
/// Thread-safe: values are read/written using <see cref="Interlocked"/> operations.
/// </para>
/// </summary>
internal sealed class NetworkEventListener : EventListener
{
    private const string SocketsEventSourceName = "System.Net.Sockets";
    private const string BytesSentCounter = "bytes-sent";
    private const string BytesReceivedCounter = "bytes-received";

    private long _bytesSent;
    private long _bytesReceived;

    /// <summary>
    /// Gets the cumulative total of bytes sent by the process since startup.
    /// </summary>
    public long BytesSent => Interlocked.Read(ref _bytesSent);

    /// <summary>
    /// Gets the cumulative total of bytes received by the process since startup.
    /// </summary>
    public long BytesReceived => Interlocked.Read(ref _bytesReceived);

    /// <inheritdoc />
    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        if (eventSource.Name == SocketsEventSourceName)
        {
            EnableEvents(eventSource, EventLevel.LogAlways, EventKeywords.All,
                new Dictionary<string, string?> { ["EventCounterIntervalSec"] = "1" });
        }
    }

    /// <inheritdoc />
    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        if (eventData.EventName != "EventCounters" || eventData.Payload is null || eventData.Payload.Count == 0)
            return;

        if (eventData.Payload[0] is not IDictionary<string, object> payload)
            return;

        if (!payload.TryGetValue("Name", out var nameObj) || nameObj is not string name)
            return;

        // PollingCounter reports the cumulative total in "Mean" field.
        // IncrementingEventCounter/IncrementingPollingCounter use "Increment".
        // Try both to handle either counter type.
        double value;
        if (payload.TryGetValue("Mean", out var meanObj))
            value = Convert.ToDouble(meanObj);
        else if (payload.TryGetValue("Increment", out var incrementObj))
            value = Convert.ToDouble(incrementObj);
        else
            return;

        var longValue = (long)value;

        if (name == BytesSentCounter)
            Interlocked.Exchange(ref _bytesSent, longValue);
        else if (name == BytesReceivedCounter)
            Interlocked.Exchange(ref _bytesReceived, longValue);
    }
}
