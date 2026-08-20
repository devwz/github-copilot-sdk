#:package GitHub.Copilot.SDK@1.0.9

using GitHub.Copilot;

// Default: uses logged-in user credentials
await using var client = new CopilotClient();

// ---------------------------------------------------------------
//          Event Subscription
// ---------------------------------------------------------------

await using var session = await client.CreateSessionAsync(new SessionConfig
{
    Model = "gpt-5.6-luna",
    OnPermissionRequest = PermissionHandler.ApproveAll,
    Streaming = true,
});

var unsubscribe = session.On<SessionEvent>(ev => Console.WriteLine($"Event: {ev.Type}"));

// Filter by event type using pattern matching
session.On<SessionEvent>(ev =>
{
    if (ev is AssistantMessageEvent messageEvent)
    {
        Console.WriteLine(messageEvent.Data.Content);
    }

    if (ev is AssistantMessageDeltaEvent deltaEvent)
    {
        Console.Write(deltaEvent.Data.DeltaContent);
    }

    if (ev is SessionIdleEvent)
    {
        Console.WriteLine();
    }
});

await session.SendAndWaitAsync(new MessageOptions { Prompt = "Tell me a short joke" });

unsubscribe.Dispose();