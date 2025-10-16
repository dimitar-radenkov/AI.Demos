using AI.Core.Settings;
using AI.Services.CodeExecution.Models;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Reflection;

namespace AI.Services.CodeExecution;

/// <summary>
/// Roslyn-based C# script runner that compiles and executes dynamic code.
/// Provides concurrency control and execution timeout enforcement.
/// </summary>
public sealed class RoslynScriptRunner : IScriptRunner, IDisposable
{
    private readonly SemaphoreSlim semaphore;
    private readonly ScriptOptions scriptOptions;
    private readonly TimeSpan maxExecutionTime;
    private readonly ILogger<RoslynScriptRunner> logger;

    public RoslynScriptRunner(
        IOptions<ScriptRunnerSettings> settings,
        ILogger<RoslynScriptRunner> logger)
    {
        var config = settings.Value;
        this.logger = logger;
        this.semaphore = new SemaphoreSlim(config.MaxConcurrentExecutions);
        this.maxExecutionTime = TimeSpan.FromSeconds(config.MaxExecutionTimeSeconds);
        this.scriptOptions = BuildScriptOptions(config);

        this.logger.LogInformation(
            "RoslynScriptRunner initialized (timeout={Timeout}s, concurrency={Concurrency})",
            config.MaxExecutionTimeSeconds,
            config.MaxConcurrentExecutions);
    }

    private static ScriptOptions BuildScriptOptions(ScriptRunnerSettings config)
    {
        var options = ScriptOptions.Default;

        // Add allowed assemblies
        var assemblies = new List<Assembly>();
        foreach (var assemblyName in config.AllowedAssemblies)
        {
            try
            {
                assemblies.Add(Assembly.Load(assemblyName));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to load assembly '{assemblyName}': {ex.Message}", ex);
            }
        }

        options = options.AddReferences(assemblies);
        options = options.WithImports(config.AllowedNamespaces);

        return options;
    }

    public async Task<ExecutionResult> ExecuteAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return ExecutionResult.Failure("Code cannot be empty");
        }

        var stopwatch = Stopwatch.StartNew();
        await this.semaphore.WaitAsync(cancellationToken);

        try
        {
            this.logger.LogDebug("Executing code: {Code}", code);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(this.maxExecutionTime);

            // Create and execute script
            var script = CSharpScript.Create<object>(code, this.scriptOptions);
            var scriptResult = await script.RunAsync(cancellationToken: cts.Token);

            stopwatch.Stop();

            this.logger.LogInformation(
                "Code executed successfully in {Duration}ms",
                stopwatch.ElapsedMilliseconds);

            var result = new ExecutionDto
            {
                ReturnValue = scriptResult.ReturnValue,
                ExecutionTime = stopwatch.Elapsed
            };

            return ExecutionResult.Success(result);
        }
        catch (CompilationErrorException ex)
        {
            stopwatch.Stop();
            var errors = string.Join("\n", ex.Diagnostics);
            this.logger.LogWarning("Compilation error: {Errors}", errors);
            return ExecutionResult.Failure($"Compilation failed:\n{errors}");
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            this.logger.LogWarning("Execution timed out after {Timeout}s", this.maxExecutionTime.TotalSeconds);
            return ExecutionResult.Failure($"Execution timed out after {this.maxExecutionTime.TotalSeconds} seconds");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            this.logger.LogError(ex, "Runtime error during code execution");
            return ExecutionResult.Failure($"Runtime error: {ex.Message}");
        }
        finally
        {
            this.semaphore.Release();
        }
    }

    public async Task<ValidationResult> ValidateAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return ValidationResult.Failure("Code cannot be empty");
        }

        try
        {
            var script = CSharpScript.Create<object>(code, this.scriptOptions);
            var compilation = script.GetCompilation();
            var diagnostics = compilation.GetDiagnostics();

            var errors = diagnostics
                .Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
                .Select(d => d.ToString());

            return errors.Any()
                ? ValidationResult.Failure(string.Join("\n", errors))
                : ValidationResult.Success();
        }
        catch (Exception ex)
        {
            return ValidationResult.Failure($"Validation error: {ex.Message}");
        }
    }

    public void Dispose()
    {
        this.semaphore?.Dispose();
    }
}
