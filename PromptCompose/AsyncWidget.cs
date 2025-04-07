using PromptCompose.Framework;

namespace PromptCompose;

public abstract class AsyncWidget : Widget
{
    protected abstract ValueTask<Widget?> OnBuild(BuildContext buildContext, ChatContext chatContext);
    
    public override Element NewElement() => new AsyncWidgetElement();
    
    private class AsyncWidgetElement : Element<AsyncWidget>
    {
        private Element? _childElement;

        protected override Task OnUnmount()
        {
            _childElement?.Unmount();

            return Task.CompletedTask;
        }
        
        protected override async Task OnCompile(ChatContext context)
        {
            _childElement = await UpdateChild(_childElement, await Widget.OnBuild(this, context));
            
            await CompileChild(context, _childElement);
        }
    }
}