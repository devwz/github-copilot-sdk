#:package GitHub.Copilot.SDK@1.0.9

using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using GitHub.Copilot;
using Microsoft.Extensions.AI;

// Default: uses logged-in user credentials
await using var client = new CopilotClient();

// ---------------------------------------------------------------
//          Tools
// ---------------------------------------------------------------

// Define a custom tool that the assistant can call during a conversation

var getWeather = CopilotTool.DefineTool(
    ([Description("The city name")] string city) =>
    {
        // In a real app, you'd call a weather API here
        var conditions = new[] { "sunny", "cloudy", "rainy", "partly cloudy" };
        var temp = Random.Shared.Next(50, 80);
        var condition = conditions[Random.Shared.Next(conditions.Length)];
        return JsonSerializer.Serialize(
            new WeatherReport(city, $"{temp}°F", condition),
            WeatherReportJsonContext.Default.WeatherReport);
    },
    factoryOptions: new AIFunctionFactoryOptions
    {
        Name = "get_weather",
        Description = "Get the current weather for a given city."
    }
);

await using var session = await client.CreateSessionAsync(new SessionConfig
{
    Model = "gpt-5.6-luna",
    OnPermissionRequest = PermissionHandler.ApproveAll,
    Streaming = true,
    Tools = [getWeather],
});

session.On<SessionEvent>(ev =>
{
    if (ev is AssistantMessageDeltaEvent deltaEvent)
    {
        Console.Write(deltaEvent.Data.DeltaContent);
    }

    if (ev is SessionIdleEvent)
    {
        Console.WriteLine();
    }
});

await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = "What's the weather like in Seattle and Tokyo?"
});

// ---------------------------------------------------------------
//         Type Declarations
// ---------------------------------------------------------------

record WeatherReport(string City, string Temperature, string Condition);

[JsonSerializable(typeof(WeatherReport))]
partial class WeatherReportJsonContext : JsonSerializerContext { }