using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using OpenAI.Chat;
using PromptCompose.Framework;

namespace PromptCompose.Widgets;

[DebuggerDisplay("Message: {Role}")]
public class Message() : MultiChildSlotWidget<Message>
{
    [SetsRequiredMembers]
    public Message(ChatMessageRole role, params IEnumerable<Widget> children) : this()
    {
        Role = role;
        Children = children;
    }

    public required ChatMessageRole Role { get; init; }

    public override Element NewElement() => new MessageElement();

    private class MessageElement : MultiChildSlotElement<Message>
    {
        protected override async Task OnCompile(ChatContext context)
        {
            await base.OnCompile(context);
            context.PackMessage(Widget.Role);
        }
    }
    
    public static Message User(params IEnumerable<Widget> children) => new(ChatMessageRole.User, children);
    
    public static Message Assistant(params IEnumerable<Widget> children) => new(ChatMessageRole.Assistant, children);
    
    public static Message System(params IEnumerable<Widget> children) => new(ChatMessageRole.System, children);
    
    public static Message Developer(params IEnumerable<Widget> children) => new(ChatMessageRole.Developer, children);
}