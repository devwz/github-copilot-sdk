#:package GitHub.Copilot.SDK@1.0.9

using GitHub.Copilot;

// Default: uses logged-in user credentials
await using var client = new CopilotClient();

// ---------------------------------------------------------------
//          Interactive Assistant
// ---------------------------------------------------------------

await using var session = await client.CreateSessionAsync(new SessionConfig
{
    Model = "gpt-5.6-luna",
    OnPermissionRequest = PermissionHandler.ApproveAll
});

while (true)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write("You: ");
    Console.ResetColor();

    var userInput = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(userInput) || userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    Console.ForegroundColor = ConsoleColor.Red;
    Console.Write("Assistant: ");
    Console.ResetColor();
    
    var response = await session.SendAndWaitAsync(new MessageOptions { Prompt = userInput });
    Console.WriteLine(response?.Data.Content);
}