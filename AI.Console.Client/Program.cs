using AI.Agents.Presentation;
using AI.Console.Client.Extensions;
using AI.Console.Client.Factories;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

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
var input = "What is the result of 10+10+10+10+10";
var run = await InProcessExecution.RunAsync(workflow, input);

Console.ReadLine();