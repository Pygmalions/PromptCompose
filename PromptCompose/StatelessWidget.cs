using PromptCompose.Framework;

namespace PromptCompose;

/// <summary>
/// The content of a stateless widget never changes.
/// </summary>
public abstract class StatelessWidget : Widget
{
    public override Element NewElement() => new StatelessElement();
    
    /// <summary>
    /// Override this method to describe the content of this widget.
    /// </summary>
    /// <param name="context">Context of this widget during the build.</param>
    /// <returns>Widget that describes the content of this widget.</returns>
    protected virtual ValueTask<Widget?> OnBuild(BuildContext context) => ValueTask.FromResult<Widget?>(null);
    
    private class StatelessElement : Element<StatelessWidget>
    {
        private Element? _childElement;
        
        protected override async Task OnMount()
        {
            _childElement = await UpdateChild(_childElement, await Widget.OnBuild(this));
        }

        protected override Task OnUnmount()
        {
            _childElement?.Unmount();

            return Task.CompletedTask;
        }

        protected override async Task OnUpdate(StatelessWidget newWidget)
        {
            _childElement = await UpdateChild(_childElement, await newWidget.OnBuild(this));
        }

        protected override Task OnCompile(ChatContext context)
        {
            return CompileChild(context, _childElement);
        }
    }
}