using PromptCompose.Framework;

namespace PromptCompose;

public class SingleChildSlotWidget<TWidget> : Widget where TWidget : SingleChildSlotWidget<TWidget>
{
    public Widget? Child { get; init; }

    public override Element NewElement() => new SingleChildSlotElement<TWidget>();
}

public class SingleChildSlotElement<TWidget> : Element<TWidget> where TWidget : SingleChildSlotWidget<TWidget>
{
    private Element? _childElement;
    
    /// <summary>
    /// Update the child element.
    /// </summary>
    protected override async Task OnUpdate(TWidget newWidget)
    {
        _childElement = await UpdateChild(_childElement, newWidget.Child);
    }

    /// <summary>
    /// Inflate and mount child element.
    /// </summary>
    protected override async Task OnMount()
    {
        if (Widget.Child != null)
            _childElement = await MountChild(Widget.Child);
    }

    /// <summary>
    /// Unmount the child element.
    /// </summary>
    protected override Task OnUnmount()
    {
        return _childElement != null ? _childElement.Unmount() : Task.CompletedTask;
    }

    /// <summary>
    /// Compile the child element.
    /// </summary>
    protected override Task OnCompile(ChatContext context)
    {
        return CompileChild(context, _childElement);
    }
}