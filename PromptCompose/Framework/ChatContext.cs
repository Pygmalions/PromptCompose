using OpenAI.Chat;

namespace PromptCompose.Framework;

public class ChatContext(Activity activity)
{
    /// <summary>
    /// The activity that this context is associated with.
    /// </summary>
    public Activity Activity { get; } = activity;

    /// <summary>
    /// Messages that have been packed in the chat history.
    /// </summary>
    public LinkedList<ChatMessage> Messages { get; } = [];

    /// <summary>
    /// Contents that have not been packed into messages yet.
    /// </summary>
    public LinkedList<ChatMessageContentPart> Contents { get; } = [];
    
    /// <summary>
    /// Pack the specific number of contents into a message.
    /// </summary>
    /// <param name="role">Role of this message.</param>
    /// <param name="number">Numbers of contents to pack from the top of the stack.</param>
    internal void PackMessage(ChatMessageRole role, int? number = null)
    {
        var parts = Contents.ToArray();
        Contents.Clear();

        Messages.AddLast(role switch
        {
            ChatMessageRole.System => ChatMessage.CreateSystemMessage(parts),
            ChatMessageRole.User => ChatMessage.CreateUserMessage(parts),
            ChatMessageRole.Assistant => ChatMessage.CreateAssistantMessage(parts),
            ChatMessageRole.Developer => ChatMessage.CreateDeveloperMessage(parts),
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, $"Unsupported message role: {role}.")
        });
    }
}