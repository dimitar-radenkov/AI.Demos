using AI.Agents.Analysis;
using AI.Agents.CodeGeneration;
using AI.Agents.Pipeline.Executors;
using AI.Agents.Pipeline.Models;
using AI.Agents.Presentation;
using AI.Agents.QualityAssurance;
using AI.Client.Settings;
using AI.Console.Client.Settings;
using AI.Services.CodeExecution;
using AI.Services.CodeExecution.Models;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

var loggerFactor = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

var scriptRunner = new RoslynScriptRunner(
    settings: Options.Create(ScriptRunnerSettingsProvider.Create()),
    logger: loggerFactor.CreateLogger<RoslynScriptRunner>());

var analystAgent = new QueryAnalystAgent(Options.Create(AgentSettingsProvider.CreateQueryAnalystSettings()));
var developerAgent = new DeveloperAgent(Options.Create(AgentSettingsProvider.CreateDeveloperSettings()));
var reviewerAgent = new ReviewerAgent(Options.Create(AgentSettingsProvider.CreateReviewerSettings()));
var presenterAgent = new PresenterAgent(Options.Create(AgentSettingsProvider.CreatePresenterSettings()));

var analystExecutor = new AnalystExecutor(analystAgent);
var developerExecutor = new DeveloperExecutor(developerAgent);
var reviewerExecutor = new ReviewerExecutor(reviewerAgent);
var scriptExecutor = new ScriptExecutionExecutor(scriptRunner);
var presenterExecutor = new PresenterExecutor(presenterAgent);

var workflowBuilder = new WorkflowBuilder(analystExecutor);

workflowBuilder.AddEdge(analystExecutor, developerExecutor);
workflowBuilder.AddEdge(developerExecutor, reviewerExecutor);

workflowBuilder.AddSwitch(reviewerExecutor, sb =>
{
    sb.AddCase<ReviewerDecision>(d => d.CodeReview.IsApproved, scriptExecutor );
    sb.WithDefault(developerExecutor);
});

workflowBuilder.AddSwitch(scriptExecutor, sb =>
{
    sb.AddCase<ExecutionResult>(r => r.IsSuccess, presenterExecutor);
    sb.WithDefault(developerExecutor);
});

var workflow = workflowBuilder.Build();

var run = await InProcessExecution.RunAsync(
    workflow,
    "what is the result of 20+20+20*10");

foreach (var evt in run.NewEvents)
{
    if (evt is ExecutorCompletedEvent completed)
    {
        Console.WriteLine($"✅ {completed.ExecutorId} completed");
        Console.WriteLine($"   Data: {completed.Data}");
    }
}

Console.WriteLine("test");