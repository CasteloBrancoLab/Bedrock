namespace Bedrock.BuildingBlocks.Persistence.Abstractions.Connections.Interfaces;

public interface IConnection : IDisposable
{
    // Constants
    public const string CONNECTION_ALREADY_OPEN_MESSAGE_CODE = "IConnection.ConnectionAlreadyOpen";
    public const string CONNECTION_ALREADY_OPEN_MESSAGE_DESCRIPTION = "Connection already open";

    public const string CONNECTION_ALREADY_CLOSED_MESSAGE_CODE = "IConnection.ConnectionAlreadyClosed";
    public const string CONNECTION_ALREADY_CLOSED_MESSAGE_DESCRIPTION = "Connection already closed";

    // Methods
    /// <summary>
    /// Indicates whether the underlying connection is open and ready for use.
    /// </summary>
    public bool IsOpen();

    /// <summary>
    /// Attempts to open the connection if it is not already open.
    /// </summary>
    public Task<bool> TryOpenConnectionAsync(ExecutionContext executionContext, CancellationToken cancellationToken);

    /// <summary>
    /// Attempts to close the connection if it is currently open.
    /// </summary>
    public Task<bool> TryCloseConnectionAsync(ExecutionContext executionContext, CancellationToken cancellationToken);
}
