namespace AI.Agents;

public interface IAgent<TOutput>
{
    Task<TOutput> ExecuteAsync(string input, CancellationToken cancellationToken = default);
}
