using System.Text.Json;
using OpenAI.Chat;
using PromptCompose.Framework;
using PromptCompose.Utilities;

namespace PromptCompose.Widgets;

public class MultipleSelection<TValue> : AsyncWidget
{
    public required string Prompt { get; init; }

    public required IList<(string Description, TValue Value)> Choices { get; init; }

    public required Func<Widget, IEnumerable<TValue>, Widget?> Child { get; init; }

    protected override async ValueTask<Widget?> OnBuild(BuildContext buildContext, ChatContext chatContext)
    {
        var question = Message.User(
            new Text("Here you have multiple choices. Select one or more" +
                     $"of them according to the instruction: {Prompt} \n"),
            new Sequence(Choices.Select((pair, index) =>
                new Text($"Choice {index}: {pair.Description}"))));
        var choices = await buildContext.Respond(question, new ChatCompletionOptions()
        {
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat("Choices", new BinaryData(
                """
                {
                    "type": "object",
                    "properties": {
                        "choices": {
                            "type": "array",
                            "items": {
                                "type": "integer"
                            },
                            "description": "The indices of the selected choices."
                        }
                    },
                    "required": ["choice"]
                }
                """))
        });

        var selectedChoices = JsonDocument
            .Parse(choices.GetContentText()).RootElement.GetProperty("choices")
            .EnumerateArray()
            .Select(element => element.GetInt32()).Select(index => Choices[index].Value)
            .ToArray();

        return Child(this, selectedChoices);
    }
}