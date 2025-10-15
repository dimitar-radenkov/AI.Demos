namespace AI.Core.Infrastructure;

public abstract record OperationResult<TSelf> where TSelf : OperationResult<TSelf>, new()
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }

    public static TSelf Success() => new() { IsSuccess = true };
    public static TSelf Failure(string errorMessage) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage
    };
}

public abstract record OperationResult<TData, TSelf> : OperationResult<TSelf>
    where TSelf : OperationResult<TData, TSelf>, new()
{
    public TData? Data { get; init; }

    public static TSelf Success(TData data) => new()
    {
        IsSuccess = true,
        Data = data
    };
    public new static TSelf Failure(string errorMessage) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage
    };
}
