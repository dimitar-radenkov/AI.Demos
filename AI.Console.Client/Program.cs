using AI.Agents.Analysis;
using AI.Agents.CodeGeneration;
using AI.Agents.QualityAssurance;
using AI.Client.Settings;

// Initialize agents
var analyst = new QueryAnalystAgent(AgentSettingsProvider.CreateQueryAnalystSettings());
var developer = new DeveloperAgent(AgentSettingsProvider.CreateDeveloperSettings());
var reviewer = new ReviewerAgent(AgentSettingsProvider.CreateReviewerSettings());

var question = "what is the result of 20+20+20*10";
Console.WriteLine($"Question: {question}\n");

// Analyst
var requirementsResult = await analyst.ExecuteAsync(question);
if (!requirementsResult.IsSuccess)
{
    Console.WriteLine($"Error: {requirementsResult.ErrorMessage}");
    return;
}

var requirements = requirementsResult.Data!;
Console.WriteLine($"Task: {requirements.Task}\n");

// Developer
var codeResult = await developer.ExecuteAsync(requirements);
if (!codeResult.IsSuccess)
{
    Console.WriteLine($"Error: {codeResult.ErrorMessage}");
    return;
}

var codeArtifact = codeResult.Data!;
Console.WriteLine($"Generated Code: {codeArtifact.Code}\n");

// Reviewer
var reviewResult = await reviewer.ExecuteAsync(codeArtifact);
if (!reviewResult.IsSuccess)
{
    Console.WriteLine($"Error: {reviewResult.ErrorMessage}");
    return;
}

var review = reviewResult.Data!;
Console.WriteLine($"Review Status: {(review.IsApproved ? "✓ APPROVED" : "✗ REJECTED")}");
Console.WriteLine($"Comments: {review.Comments}");
