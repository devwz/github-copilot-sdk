// sdk: https://github.com/github/copilot-sdk
// references: https://docs.github.com/en/copilot/how-tos/copilot-sdk

#:package GitHub.Copilot.SDK@1.0.9

using GitHub.Copilot;

// Default: uses logged-in user credentials
await using var client = new CopilotClient();

// ---------------------------------------------------------------
//          Authentication
// ---------------------------------------------------------------

// OAuth - https://docs.github.com/en/copilot/how-tos/copilot-sdk/auth/authenticate
// await using var client = new CopilotClient(new CopilotClientOptions
// {
//     GitHubToken = "[userAccessToken]",
//     UseLoggedInUser = false,
// });

// BYOK (bring your own key) - https://docs.github.com/en/copilot/how-tos/copilot-sdk/auth/byok
// await using var session = await client.CreateSessionAsync(new SessionConfig
// {
//     Model = "gpt-5.2-codex",  // Your deployment name
//     Provider = new ProviderConfig
//     {
//         Type = "openai",
//         BaseUrl = "https://your-resource.openai.azure.com/openai/v1/",
//         WireApi = "responses",  // Use "completions" for older models
//         ApiKey = Environment.GetEnvironmentVariable("FOUNDRY_API_KEY"),
//     },
// });

// ---------------------------------------------------------------
//          Getting Started
// ---------------------------------------------------------------

// Getting started with the Copilot SDK
// https://docs.github.com/en/copilot/how-tos/copilot-sdk/getting-started

await using var session = await client.CreateSessionAsync(new SessionConfig
{
    Model = "gpt-5.6-luna", // the model must be available to the authenticated Copilot account
    OnPermissionRequest = PermissionHandler.ApproveAll // demo-only, use a restricted handler in production
});

var response = await session.SendAndWaitAsync(new MessageOptions 
{
    Prompt = "Write a function that adds two numbers in Python."
});

Console.WriteLine(response?.Data.Content);
