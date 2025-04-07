using OpenAI.Chat;

namespace PromptCompose.Utilities;

public static class ChatCompletionExtensions
{
    public static string GetContentText(this ChatCompletion completion)
        => completion.Content.First().Text;
}