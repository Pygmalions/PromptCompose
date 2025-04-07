using System.ClientModel;
using System.Diagnostics;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using PromptCompose.Robotics;
using PromptCompose.Utilities;
using PromptCompose.Widgets;
using Spectre.Console;
using Size = OpenCvSharp.Size;
using Text = PromptCompose.Widgets.Text;
using Activity = PromptCompose.Framework.Activity;

var client = new AzureOpenAIClient(
    new Uri(Environment.GetEnvironmentVariable("AZURE_ENDPOINT")!),
    new ApiKeyCredential(Environment.GetEnvironmentVariable("AZURE_KEY")!),
    new AzureOpenAIClientOptions(AzureOpenAIClientOptions.ServiceVersion.V2024_12_01_Preview)
);

Console.WriteLine("============================================================");
Console.WriteLine("Starts.");

ImageDisplay.Start();

Console.CancelKeyPress += (_, _) => ImageDisplay.Stop();

while (true)
{
    var timestamp = Stopwatch.GetTimestamp();
    var activity = new Activity
    {
        ChatClient = client.GetChatClient("gpt-4o"),
        RootWidget = new Sequence(
            Message.System(
                new Text("You are a helpful assistant.")),
            Message.User(
                new Text("Hello, how are you?"),
                new CameraImage(0) {
                    Size = new Size(256, 256), 
                    Fit = BoxFit.Contain,
                    DetailLevel = ChatImageDetailLevel.Low,
                }
            )
        )
    };
    var response = await activity.Respond(new ChatCompletionOptions()
    {
        ResponseModalities = ChatResponseModalities.Text | ChatResponseModalities.Audio,
        AudioOptions = new ChatAudioOptions(
            ChatOutputAudioVoice.Alloy, ChatOutputAudioFormat.Wav)
    });
    Console.WriteLine("============================================================");
    Console.WriteLine(response.GetContentText());

    AnsiConsole.Write(
        new Spectre.Console.Text($"Time Consumption: {Stopwatch.GetElapsedTime(timestamp).TotalMilliseconds} ms",
            Color.Aqua));
    AnsiConsole.WriteLine();
}