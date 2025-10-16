using AI.Agents.Analysis;
using AI.Agents.CodeGeneration;
using AI.Agents.Pipeline.Executors;
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

//analyst -> developer
workflowBuilder.AddEdge(analystExecutor, developerExecutor);

//developer -> reviewer
workflowBuilder.AddEdge(developerExecutor, reviewerExecutor);

// reviewer -> script executor (only if approved)
workflowBuilder.AddEdge<CodeReview>(
    source: reviewerExecutor,
    target: scriptExecutor,
    condition: review => review!.IsApproved
);

// reviewer -> developer (if NOT approved)
workflowBuilder.AddEdge<CodeReview>(
    source: reviewerExecutor,
    target: developerExecutor,
    condition: review => !review!.IsApproved
);

// executor -> presenter (only if execution succeeded AND returnValue is not null)
workflowBuilder.AddEdge<ExecutionResult>(
    source: scriptExecutor,
    target: presenterExecutor,
    condition: result => result.IsSuccess && result.Data?.ReturnValue is not null
);

// executor -> developer (if execution failed OR returnValue is null) - LOOP BACK
workflowBuilder.AddEdge<ExecutionResult>(
    source: scriptExecutor,
    target: developerExecutor,
    condition: result => !result.IsSuccess || result.Data?.ReturnValue is null
);

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