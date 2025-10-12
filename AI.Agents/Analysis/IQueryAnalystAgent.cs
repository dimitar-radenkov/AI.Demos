namespace AI.Agents.Analysis;

public interface IQueryAnalystAgent
{
    Task<RequirementsResult> AnalyzeRequest(
        string userRequest,
        CancellationToken cancellationToken = default);
}