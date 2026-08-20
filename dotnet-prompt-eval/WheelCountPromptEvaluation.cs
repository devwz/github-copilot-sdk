#:package GitHub.Copilot.SDK@1.0.9

using GitHub.Copilot;

// Default: uses logged-in user credentials
await using var client = new CopilotClient();

// ---------------------------------------------------------------
//          Prompt evaluation
// ---------------------------------------------------------------

await using var session = await client.CreateSessionAsync(new SessionConfig
{
    Model = "gpt-5.6-luna",
    OnPermissionRequest = PermissionHandler.ApproveAll
});

var evals = new[]
{
    new { statement = "The vehicle is a bicycle.", expected_answer = 2 },
    new { statement = "The vehicle is a motorcycle.", expected_answer = 2 },
    new { statement = "The vehicle is a car.", expected_answer = 4 },
    new { statement = "The vehicle is a truck.", expected_answer = 6 },
    new { statement = "The vehicle is a bus.", expected_answer = 4 },
    new { statement = "The vehicle is a unicycle.", expected_answer = 1 },
    new { statement = "The vehicle is a tricycle.", expected_answer = 3 },
    new { statement = "The vehicle is a scooter.", expected_answer = 2 },
    new { statement = "The vehicle is a car with two extra wheels attached.", expected_answer = 6 },
    new { statement = "The vehicle is a bicycle that lost one wheel and then had two new wheels added.", expected_answer = 3 },
    new { statement = "The vehicle is a truck that lost two wheels and then got one replacement wheel.", expected_answer = 5 },
    new { statement = "The vehicle is a two-trailer road train with eighteen wheels.", expected_answer = 18 }
};

static string BuiltInPrompt(string vehicleStatement) => $"""
    You will be provided a statement about a vehicle and your job is to determine how many wheels that vehicle has.
    Here is the vehicle statement.
    <vehicle_statement>{vehicleStatement}</vehicle_statement>
    How many wheels does the vehicle have? Please respond with a single integer number only.
""";

var correctCount = 0;

foreach (var eval in evals)
{
    var prompt = BuiltInPrompt(eval.statement);

    var response = await session.SendAndWaitAsync(new MessageOptions 
    {
        Prompt = prompt
    });

    var answer = response?.Data.Content.Trim();

    if (!int.TryParse(answer, out int predicted))
    {
        Console.WriteLine($"Predicted answer is not a valid integer: {answer}");
        continue;
    }

    if (predicted == eval.expected_answer)
    {
        correctCount++;
    }

    Console.WriteLine($"Statement: {eval.statement}");
    Console.WriteLine($"Expected answer: {eval.expected_answer}");
    Console.WriteLine($"Predicted answer: {predicted}");
    Console.WriteLine();
}

var score = (double)correctCount / evals.Length * 100;
Console.WriteLine($"Score: {score:F2}%");