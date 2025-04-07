using System.ClientModel;
using System.Diagnostics;
using Azure.AI.OpenAI;
using PromptCompose.Utilities;
using PromptCompose.Widgets;
using Activity = PromptCompose.Framework.Activity;

var client = new AzureOpenAIClient(
    new Uri(Environment.GetEnvironmentVariable("AZURE_ENDPOINT")!),
    new ApiKeyCredential(Environment.GetEnvironmentVariable("AZURE_KEY")!),
    new AzureOpenAIClientOptions(
        AzureOpenAIClientOptions.ServiceVersion.V2024_12_01_Preview)
);

var timestamp = Stopwatch.GetTimestamp();

var activity = new Activity
{
    ChatClient = client.GetChatClient("gpt-4o-mini"),
    RootWidget = new Sequence(
        Message.System(
            new Text("You are a very helpful assistant."),
            new Text("Answer questions with no more than 30 words.")
        ),
        Message.User(
            new Predicate()
            {
                Prompt = "Are you human?",
                Child = (result) => result? 
                    new Text("How could you be a human?") :
                    new Text("What makes you different from a human?")
            },
            new Selection<int>()
            {
                Prompt = "What is your favorite color?",
                Child  = (widget, color) =>
                {
                    return new Text($"Why you prefer this color {widget.Choices[color]}") {
                    };
                },
                Choices = [
                    ("Red", 1),
                    ("Green", 2),
                    ("Blue", 3),
                ]
            }
        )
    )
};

var response = await activity.Respond();

Console.WriteLine($"Time Consumption: {Stopwatch.GetElapsedTime(timestamp).TotalMilliseconds} ms");
Console.WriteLine("============================================================");
Console.WriteLine(response.GetContentText());
Console.WriteLine("============================================================");