using System.Text.Json;
using OpenAI.Chat;
using PromptCompose.Framework;
using PromptCompose.Utilities;

namespace PromptCompose.Widgets;

public class Selection<TValue> : AsyncWidget
{
    public required string Prompt { get; init; }

    public required IReadOnlyList<(string Description, TValue Value)> Choices { get; init; }

    public required Func<Selection<TValue>, TValue, Widget?> Child { get; init; }

    protected override async ValueTask<Widget?> OnBuild(BuildContext buildContext, ChatContext chatContext)
    {
        var question = Message.User(
            new Text("Here you have multiple choices. Select one " +
                     $"of them according to the instruction: {Prompt} \n"),
            Choices.Select((pair, index) =>
                new Text($"Choice {index}: {pair.Description}")).ToSequence()
            );
        var choices = await buildContext.Respond(question, new ChatCompletionOptions()
        {
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat("Choices", new BinaryData(
                """
                {
                    "type": "object",
                    "properties": {
                        "choice": {
                            "type": "integer",
                            "description": "The index of the selected choice."
                        }
                    },
                    "required": ["choice"]
                }
                """))
        });

        var selectedIndex = JsonDocument.Parse(choices.GetContentText())
            .RootElement.GetProperty("choice").GetInt32();

        return Child(this, Choices[selectedIndex].Value);
    }
}