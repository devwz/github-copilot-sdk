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
    new { complaint = "I was charged twice for the same subscription this month.", expected_answer = new[] { "Billing Issue" } },
    new { complaint = "I can't log into my account because my password reset link expired.", expected_answer = new[] { "Account Access" } },
    new { complaint = "The checkout page throws an error when I try to pay with a card.", expected_answer = new[] { "Product Bug", "Billing Issue" } },
    new { complaint = "Can you add support for annual invoices?", expected_answer = new[] { "Feature Request" } },
    new { complaint = "My order was marked delivered, but nothing arrived.", expected_answer = new[] { "Shipping Delay" } },
    new { complaint = "The app freezes whenever I open the reports tab.", expected_answer = new[] { "Product Bug" } },
    new { complaint = "I forgot which email I used for the account and cannot sign in.", expected_answer = new[] { "Account Access" } },
    new { complaint = "Please add dark mode to the mobile app.", expected_answer = new[] { "Feature Request" } },
    new { complaint = "My refund hasn't been processed after three weeks.", expected_answer = new[] { "Billing Issue" } },
    new { complaint = "The package is stuck at the courier hub and hasn't moved in days.", expected_answer = new[] { "Shipping Delay" } },
    new { complaint = "The app crashes and I can't complete payment for my order.", expected_answer = new[] { "Product Bug", "Billing Issue" } },
    new { complaint = "My account was locked after too many failed login attempts.", expected_answer = new[] { "Account Access" } },
    new { complaint = "I want a way to export my purchase history to CSV.", expected_answer = new[] { "Feature Request" } },
    new { complaint = "The promotional code field rejects every code I enter.", expected_answer = new[] { "Product Bug", "Billing Issue" } },
    new { complaint = "Can you make shipping faster for international orders?", expected_answer = new[] { "Feature Request", "Shipping Delay" } },
    new { complaint = "I never received the password reset email.", expected_answer = new[] { "Account Access" } },
    new { complaint = "The invoice shows the wrong tax amount.", expected_answer = new[] { "Billing Issue" } },
    new { complaint = "The order confirmation page keeps timing out after payment.", expected_answer = new[] { "Product Bug", "Billing Issue" } },
    new { complaint = "Could you let customers change their delivery address after placing an order?", expected_answer = new[] { "Feature Request" } },
    new { complaint = "The courier says the parcel is delayed because of weather.", expected_answer = new[] { "Shipping Delay" } },
};

// gpt-5-mini = Accuracy: 0,00%
// gpt-5.6-luna = Accuracy: 0,00%
static string BuiltInPrompt(string complaint) => $"""
    Classify the following support ticket into one or more of the following categories:
    - Billing Issue
    - Account Access
    - Product Bug
    - Feature Request
    - Shipping Delay

    Complaint: {complaint}

    Classification:
""";

// gpt-5-mini = Accuracy: 0,00%
// gpt-5.6-luna = Accuracy: 0,00%
static string ImprovedPrompt(string complaint) => $"""
    You are an AI assistant specializing in support ticket routing. Your task is to analyze a ticket and categorize it into one or more of the following categories:
    1. Billing Issue: Charges, refunds, invoices, payment failures, or promo code problems.
    2. Account Access: Login, password reset, account lockout, or email verification problems.
    3. Product Bug: The product or website is not behaving correctly, freezing, crashing, or timing out.
    4. Feature Request: A request for a new capability or improvement.
    5. Shipping Delay: Orders, deliveries, tracking, or courier delays.

    Important Guidelines:
    - A ticket may fall into multiple categories. If so, list all that apply but try to prioritize picking a single category when possible.

    Examples:
    1. Ticket: "I was charged twice for the same subscription."
       Classification: Billing Issue

    2. Ticket: "I can't log in because my reset link expired."
       Classification: Account Access

    3. Ticket: "The checkout page crashes when I try to pay with a card."
       Classification: Product Bug, Billing Issue

    4. Ticket: "Please add a dark mode option."
       Classification: Feature Request

    5. Ticket: "My package has been stuck at the courier hub for days."
       Classification: Shipping Delay

    6. Ticket: "The reports tab freezes every time I open it."
        Classification: Product Bug

    7. Ticket: "The app crashes and I can't finish payment."
        Classification: Product Bug, Billing Issue

    8. Ticket: "My account got locked after too many failed login attempts."
        Classification: Account Access

    9. Ticket: "Could you let me change the delivery address after placing an order?"
        Classification: Feature Request

    Now, please classify the following support ticket:

    <ticket>{complaint}</ticket>

    Only respond with the appropriate categories and nothing else.
    Classification:
""";

var correctCount = 0;

foreach (var eval in evals)
{
    var prompt = ImprovedPrompt(eval.complaint);

    var response = await session.SendAndWaitAsync(new MessageOptions 
    {
        Prompt = prompt
    });

    var answer = response?.Data.Content.Trim();

    if (string.IsNullOrWhiteSpace(answer))
    {
        Console.WriteLine($"Predicted answer is empty or null for complaint: {eval.complaint}");
        continue;
    }

    var predicted = answer.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries)
        .Select(s => s.Trim())
        .ToArray();

    if (predicted.Length == 0)
    {
        Console.WriteLine($"Predicted answer is empty after splitting: {answer}");
        continue;
    }

    if (predicted.SequenceEqual(eval.expected_answer))
    {
        correctCount++;
    }

    Console.WriteLine($"Complaint: {eval.complaint}");
    Console.WriteLine($"Expected answer: {string.Join(", ", eval.expected_answer)}");
    Console.WriteLine($"Predicted answer: {string.Join(", ", predicted)}");
    Console.WriteLine();
}

var accuracy = (double)correctCount / evals.Length * 100;
Console.WriteLine($"Accuracy: {accuracy:F2}%");
