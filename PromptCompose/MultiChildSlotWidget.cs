using PromptCompose.Framework;

namespace PromptCompose;

public class MultiChildSlotWidget<TWidget> : Widget where TWidget : MultiChildSlotWidget<TWidget>
{
    public IEnumerable<Widget> Children { get; init; } = [];

    public override Element NewElement() => new MultiChildSlotElement<TWidget>();
}

public class MultiChildSlotElement<TWidget> : Element<TWidget> where TWidget : MultiChildSlotWidget<TWidget>
{
    private IList<Element> _childElements = [];

    /// <summary>
    /// Update the child elements.
    /// </summary>
    protected override async Task OnUpdate(TWidget newWidget)
    {
        _childElements = await UpdateChildren(_childElements, newWidget.Children);
    }

    /// <summary>
    /// Inflate and mount child elements.
    /// </summary>
    protected override async Task OnMount()
    {
        var childElements = new List<Element>();
        foreach (var childWidget in Widget.Children)
        {
            childElements.Add(await MountChild(childWidget));
        }

        _childElements = childElements;
    }

    /// <summary>
    /// Unmount the child elements.
    /// </summary>
    protected override async Task OnUnmount()
    {
        foreach (var childElement in _childElements)
        {
            await childElement.Unmount();
        }
    }

    /// <summary>
    /// Compile the child elements.
    /// </summary>
    protected override async Task OnCompile(ChatContext context)
    {
        foreach (var childElement in _childElements)
            await CompileChild(context, childElement);
    }
}