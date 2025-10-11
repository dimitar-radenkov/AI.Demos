using AI.Agents.CodeExecution.Models;
using AI.Shared.Settings.Agents;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;

namespace AI.Agents.CodeExecution;

public sealed class CodeExecutionAgent : ICodeExecutionAgent, IDisposable
{
    private readonly SemaphoreSlim semaphore;
    private readonly ScriptOptions scriptOptions;
    private readonly TimeSpan maxExecutionTime;
    private readonly bool enableCaching;
    private readonly int maxCacheSize;
    private readonly ConcurrentDictionary<string, Script<object>> scriptCache;
    private readonly ILogger<CodeExecutionAgent> logger;

    public CodeExecutionAgent(
        IOptions<CodeExecutionSettings> settings,
        ILogger<CodeExecutionAgent> logger)
    {
        var config = settings.Value;
        this.logger = logger;
        this.enableCaching = config.EnableScriptCaching;
        this.maxCacheSize = config.MaxCacheSize;
        this.semaphore = new SemaphoreSlim(config.MaxConcurrentExecutions);
        this.maxExecutionTime = TimeSpan.FromSeconds(config.MaxExecutionTimeSeconds);
        this.scriptCache = new ConcurrentDictionary<string, Script<object>>();
        this.scriptOptions = BuildScriptOptions(config);

        this.logger.LogInformation(
            "CodeExecutionAgent initialized (timeout={Timeout}s, concurrency={Concurrency})",
            config.MaxExecutionTimeSeconds,
            config.MaxConcurrentExecutions);
    }
    
    private static ScriptOptions BuildScriptOptions(CodeExecutionSettings config)
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
    
    public async Task<ExecutionResult> ExecuteCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
            return ExecutionResult.Failure("Code cannot be empty");

        var stopwatch = Stopwatch.StartNew();
        await this.semaphore.WaitAsync(cancellationToken);

        try
        {
            this.logger.LogDebug("Executing code: {Code}", code);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(this.maxExecutionTime);

            // Get or create script
            Script<object>? script = null;
            if (this.enableCaching)
            {
                script = this.scriptCache.GetOrAdd(code, _ => 
                    CSharpScript.Create<object>(code, this.scriptOptions));
                
                // Manage cache size
                if (this.scriptCache.Count > this.maxCacheSize)
                {
                    var keysToRemove = this.scriptCache.Keys.Take(this.maxCacheSize / 2).ToList();
                    foreach (var key in keysToRemove)
                        this.scriptCache.TryRemove(key, out _);
                }
            }
            else
            {
                script = CSharpScript.Create<object>(code, this.scriptOptions);
            }

            var scriptResult = await script.RunAsync(cancellationToken: cts.Token);
            stopwatch.Stop();

            this.logger.LogInformation("Code executed successfully in {Duration}ms", 
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
            this.logger.LogWarning("Execution timed out");
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
    
    public async Task<ValidationResult> ValidateCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return ValidationResult.Failure("Code cannot be empty");

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
        catch(Exception ex)
        {
            return ValidationResult.Failure($"Validation error: {ex.Message}");
        }
    }

    public void Dispose()
    {
        this.semaphore?.Dispose();
    }
}