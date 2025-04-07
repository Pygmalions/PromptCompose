using Microsoft.Extensions.Logging;
using OpenAI.Chat;

namespace PromptCompose.Framework;

public abstract class BuildContext
{
    /// <summary>
    /// Whether this element has been mounted into an element tree.
    /// An element can only be mounted once.
    /// </summary>
    public bool IsMounted { get; protected set; }

    public abstract ILogger<Activity>? Logger { get; }
    
    /// <summary>
    /// Search the ancestor widget of the specific type.
    /// </summary>
    /// <typeparam name="TWidget">Widget type to search.</typeparam>
    /// <returns>Ancestor widget of the specified type.</returns>
    public abstract TWidget? SearchAncestor<TWidget>() where TWidget : Widget;

    /// <summary>
    /// Start a new chat inheriting the context of the current chat.
    /// </summary>
    /// <param name="continuation">Widget to mount under this widget in the new chat context.</param>
    /// <param name="options">Chat options.</param>
    /// <param name="quick">Try to get the response as quickly as possible, even compromising the response quality.</param>
    /// <returns>Responses from the new chat.</returns>
    public abstract Task<ChatCompletion> Respond(
        Widget? continuation = null, ChatCompletionOptions? options = null, bool quick = false);
}