using AI.Agents.Analysis;
using AI.Client.Settings;

// Question to analyze
var question = "what is the result of 20+20+20*10";

// Create settings and initialize agent
var settings = AgentSettingsProvider.CreateQueryAnalystSettings();
var analyst = new QueryAnalystAgent(settings);
var result = await analyst.ExecuteAsync(question);

if (result.IsSuccess)
{
    var requirements = result.Data!;
    Console.WriteLine($"✓ Analysis completed! \n\n {requirements}");
}
else
{
    Console.WriteLine($"✗ Analysis failed: {result.ErrorMessage}");
}

