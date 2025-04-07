using OpenAI.Chat;
using PromptCompose.Framework;

namespace PromptCompose;

public abstract class PrimitiveWidget<TWidget> : Widget where TWidget : PrimitiveWidget<TWidget>
{
    public override Element NewElement() => new PrimitiveElement();

    protected abstract IEnumerable<ChatMessageContentPart> OnCompile();

    protected abstract bool IsChanged(TWidget newWidget);
    
    private class PrimitiveElement : Element
    {
        private IEnumerable<ChatMessageContentPart> _contents = [];

        protected override Task OnMount()
        {
            _contents = ((TWidget)Widget).OnCompile();

            return Task.CompletedTask;
        }
        
        protected override Task OnUpdate(Widget newWidget)
        {
            if (((TWidget)Widget).IsChanged((TWidget)newWidget))
                _contents = ((TWidget)newWidget).OnCompile();
            return Task.CompletedTask;
        }

        protected override Task OnCompile(ChatContext context)
        {
            foreach (var content in _contents)
            {
                context.Contents.AddLast(content);
            }

            return Task.CompletedTask;
        }
    }
}