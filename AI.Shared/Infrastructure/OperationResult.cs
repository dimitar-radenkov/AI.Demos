namespace AI.Shared.Infrastructure;

public abstract record OperationResult<TSeft> where TSeft : OperationResult<TSeft>, new()
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }

    public static TSeft Success() => new() { IsSuccess = true };
    public static TSeft Failure(string errorMessage) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage
    };
}

public abstract record OperationResult<TData, TSeft> : OperationResult<TSeft>
    where TSeft : OperationResult<TData, TSeft>, new()
{
    public TData? Data { get; init; }

    public static TSeft Success(TData data) => new()
    {
        IsSuccess = true,
        Data = data
    };
    public new static TSeft Failure(string errorMessage) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage
    };
}
