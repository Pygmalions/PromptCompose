using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;

namespace PromptCompose.Framework;

/// <summary>
/// Element is the composing part of an element tree,
/// it manages the life cycle of a corresponding widget,
/// and implements the compilation of the widget.
/// </summary>
public abstract class Element : BuildContext
{
    /// <summary>
    /// Chat context of this element.
    /// </summary>
    public Activity Activity { get; private set; } = null!;
    
    /// <summary>
    /// Parent element of this element.
    /// </summary>
    public Element? Parent { get; private set; }

    /// <summary>
    /// Corresponding widget of this element.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Throw if this element has not been mounted.
    /// </exception>
    [field: MaybeNull]
    public Widget Widget
    {
        get => field ?? throw new InvalidOperationException("This element has not been mounted.");
        private set;
    }

    public override TWidget? SearchAncestor<TWidget>() where TWidget : class
    {
        if (Parent == null)
            return null;
        if (Parent.Widget is TWidget widget)
            return widget;
        return Parent.SearchAncestor<TWidget>();
    }
    
    /// <summary>
    /// Try to update this element with a new widget that can update this element.
    /// </summary>
    /// <param name="newWidget">New widget that can update this element.</param>
    /// <seealso cref="Widget.AreSimilar"/>
    public async Task Update(Widget newWidget)
    {
        if (!IsMounted)
            throw new InvalidOperationException("This element has not been mounted.");
        if (!Widget.AreSimilar(Widget, newWidget))
            throw new InvalidOperationException("Cannot update this element with the new widget.");

        await OnUpdate(newWidget);

        // Update the corresponding widget with the new widget.
        Widget = newWidget;
    }

    /// <summary>
    /// Mount this element to an element tree.
    /// An element can only be mounted to an element tree once.
    /// </summary>
    /// <param name="widget">The corresponding widget.</param>
    /// <param name="parent">Parent element of this element.</param>
    /// <param name="activity">The activity that this element belongs to.</param>
    /// <exception cref="InvalidOperationException">Throw if this element has already been mounted.</exception>
    public async Task Mount(Widget widget, Element? parent, Activity activity)
    {
        if (IsMounted)
            throw new InvalidOperationException("This element has already been mounted.");
        
        Activity = activity;
        Widget = widget;
        Parent = parent;

        // At this time point, Compile(...) method can be called.
        await OnMount();
        
        IsMounted = true;
    }

    /// <summary>
    /// Unmount this element from the element tree.
    /// Once unmounted, this element will not be mounted to any element tree again.
    /// </summary>
    /// <exception cref="InvalidOperationException">Throw if this element has not been mounted.</exception>
    public async Task Unmount()
    {
        if (!IsMounted)
            throw new InvalidOperationException("This element has not been mounted.");
        
        await OnUnmount();

        Widget = null!;
        Parent = null;
        
        IsMounted = false;
    }

    public async Task Compile(ChatContext context)
    {
        if (!IsMounted)
            throw new InvalidOperationException("This element has not been mounted.");

        if (IsFrozen)
            return;
        
        await OnCompile(context);
    }

    /// <summary>
    /// Create a new element for the widget and mount it to this element.
    /// </summary>
    /// <param name="widget">Child widget to mount to this widget.</param>
    /// <returns>Mounted element for the specific widget.</returns>
    protected async Task<Element> MountChild(Widget widget)
    {
        var element = widget.NewElement();
        await element.Mount(widget, this, Activity);
        return element;
    }

    /// <summary>
    /// Whether the compilation for this widget has been paused.
    /// Being true meas the compilation for this widget has been (usually temporarily) paused.
    /// </summary>
    public bool IsFrozen { get; private set; } = false;

    public override ILogger<Activity>? Logger => Activity.Logger;

    public override async Task<ChatCompletion> Respond(
        Widget? continuation = null, ChatCompletionOptions? options = null, bool quick = false)
    {
        IsFrozen = true; // Freeze the element to prevent endless-loop caused by further compilation.
        var response = await Activity.InternalRespond(this, continuation, options);
        IsFrozen = false;
        return response;
    }

    /// <summary>
    /// Update the child element with a new widget.
    /// If the child element can be updated with the new widget, update it;
    /// otherwise, inflate a new element for the new widget.
    /// </summary>
    [MustUseReturnValue("Must manually replace the old element with the new element.")]
    protected async Task<Element?> UpdateChild(Element? oldChildElement, Widget? newChildWidget)
    {
        if (newChildWidget == null)
        {
            if (oldChildElement != null)
                await oldChildElement.Unmount();
            return null;
        }

        // When there is no old child element:
        if (oldChildElement == null)
            return await MountChild(newChildWidget);

        // When old child element can be updated with the new widget:
        if (Widget.AreSimilar(oldChildElement.Widget, newChildWidget))
        {
            await oldChildElement.Update(newChildWidget);
            return oldChildElement;
        }

        // When old child element cannot be updated with new widget:
        await oldChildElement.Unmount();
        return await MountChild(newChildWidget);
    }

    /// <summary>
    /// Update the child elements with new widgets.
    /// Elements will be updated with similar widgets if possible;
    /// otherwise, they will be replaced with new elements created from the new widgets.
    /// </summary>
    [MustUseReturnValue("Must manually replace the old elements with the new elements.")]
    protected async Task<IList<Element>> UpdateChildren(
        IList<Element> oldChildrenElements, IEnumerable<Widget> newChildrenWidgets)
    {
        var newChildrenElements = new List<Element>();

        foreach (var childWidget in newChildrenWidgets)
        {
            var childElement =
                oldChildrenElements.FirstOrDefault(element => Widget.AreSimilar(element.Widget, childWidget));
            if (childElement != null)
                await childElement.Update(childWidget);
            else
                childElement = await MountChild(childWidget);
            newChildrenElements.Add(childElement);
        }

        return newChildrenElements;
    }

    protected async Task CompileChild(ChatContext context, Element? childElement)
    {
        if (childElement != null)
            await childElement.Compile(context);
    }

    /// <summary>
    /// Invoked when this element is mounted to an element tree.
    /// <see cref="Parent"/> and <see cref="Widget"/> have been set.
    /// </summary>
    protected virtual Task OnMount() => Task.CompletedTask;

    /// <summary>
    /// Invoked when this element is unmounted from an element tree.
    /// <see cref="Parent"/> and <see cref="Widget"/> have not been reset to null.
    /// </summary>
    protected virtual Task OnUnmount() => Task.CompletedTask;

    /// <summary>
    /// Invoked when this element is updated with a new widget.
    /// <see cref="Widget"/> has not been updated to the new widget.
    /// </summary>
    /// <param name="newWidget">New widget to update to.</param>
    protected virtual Task OnUpdate(Widget newWidget) => Task.CompletedTask;

    /// <summary>
    /// Invoked when this element is compiled.
    /// </summary>
    protected virtual Task OnCompile(ChatContext context) => Task.CompletedTask;
}

public abstract class Element<TWidget> : Element where TWidget : Widget
{
    /// <inheritdoc cref="Element.Widget"/>
    [DebuggerHidden]
    public new TWidget Widget => (TWidget)base.Widget;

    /// <inheritdoc cref="Element.OnUpdate"/>
    protected virtual Task OnUpdate(TWidget newWidget) => Task.CompletedTask;

    protected sealed override Task OnUpdate(Widget newWidget)
    {
        return OnUpdate((TWidget)newWidget);
    }
}