using System.Text.Json;
using OpenAI.Chat;
using PromptCompose.Framework;
using PromptCompose.Utilities;

namespace PromptCompose.Widgets;

public class Predicate : AsyncWidget
{
    public required string Prompt { get; init; }
    
    public required Func<bool, Widget> Child { get; init; }
    
    protected override async ValueTask<Widget?> OnBuild(BuildContext buildContext, ChatContext chatContext)
    {
        var question = Message.User(
            new Text($"Answer this question with true or false: {Prompt}\n")
        );
        var response = await buildContext.Respond(question, new ChatCompletionOptions()
        {
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat("Choices", new BinaryData(
                """
                {
                    "type": "object",
                    "properties": {
                        "result": {
                            "type": "boolean",
                            "description": "The true/false result of the answer."
                        }
                    },
                    "required": ["result"]
                }
                """))
        });
        
        var result = JsonDocument.Parse(response.GetContentText())
            .RootElement.GetProperty("result").GetBoolean();

        return Child(result);
    }
}