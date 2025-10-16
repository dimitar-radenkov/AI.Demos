using AI.Agents.Analysis;
using AI.Agents.CodeGeneration;
using AI.Agents.Presentation;
using AI.Agents.QualityAssurance;
using AI.Client.Settings;
using AI.Console.Client.Settings;
using AI.Services.CodeExecution;
using AI.Services.CodeExecution.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// Initialize agents with IOptions
var analyst = new QueryAnalystAgent(Options.Create(AgentSettingsProvider.CreateQueryAnalystSettings()));
var developer = new DeveloperAgent(Options.Create(AgentSettingsProvider.CreateDeveloperSettings()));
var reviewer = new ReviewerAgent(Options.Create(AgentSettingsProvider.CreateReviewerSettings()));
var presenter = new PresenterAgent(Options.Create(AgentSettingsProvider.CreatePresenterSettings()));

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

var scriptRunner = new RoslynScriptRunner(
    settings: Options.Create(ScriptRunnerSettingsProvider.Create()),
    logger: loggerFactory.CreateLogger<RoslynScriptRunner>());

var executionResult = await scriptRunner.ExecuteAsync(codeArtifact.Code);
if (!executionResult.IsSuccess)
{
    Console.WriteLine($"Error: {executionResult.ErrorMessage}");
    return;
}

//Presenter
var presenterInput = new PresentationInput
{
    UserRequest = question,
    ExecutionResult = new ExecutionDto { ExecutionTime = TimeSpan.FromMilliseconds(100), ReturnValue = executionResult!.Data!.ReturnValue }
};
var presentationResult = await presenter.ExecuteAsync(presenterInput);
if (!presentationResult.IsSuccess)
{
    Console.WriteLine($"Error: {presentationResult.ErrorMessage}");
    return;
}

var presentation = presentationResult.Data!;
Console.WriteLine($"\nPresentation:\nSummary: {presentation.Summary}\Result: {presentation.FormattedResult}\n");
