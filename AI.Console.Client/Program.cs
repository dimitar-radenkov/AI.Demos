using AI.Console.Client.Extensions;
using AI.Console.Client.Factories;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddLogging();
builder.Services.AddAgents();
builder.Services.AddWorkflow();
builder.Services.AddScriptRunner();

var host = builder.Build();
using var scope = host.Services.CreateScope();
var workflowFactory = scope.ServiceProvider.GetRequiredService<IWorkflowFactory>();
var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

var workflow = workflowFactory.Create();
//var input = "what is the result of sqrt(64) ?";
//var input = "What is 15% of 2,450?";
var input = "What is the sum of all numbers from 1 to 10?";
var run = await InProcessExecution.RunAsync(workflow, input);

Console.ReadLine();