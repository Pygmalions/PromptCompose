using System.Diagnostics;
using PromptCompose.Framework;

namespace PromptCompose;

public abstract class State
{
    /// <summary>
    /// Widget that owns this state.
    /// </summary>
    public Widget Widget { get; private set; } = null!;
    
    public bool IsMounted { get; private set; }
    
    /// <summary>
    /// Override this method to describe the content of this widget.
    /// </summary>
    /// <param name="context">Context of this widget during the build.</param>
    /// <returns>Widget that describes the content of this widget.</returns>
    protected virtual ValueTask<Widget?> OnBuild(BuildContext context) => ValueTask.FromResult<Widget?>(null);

    /// <summary>
    /// Invoked when this state is bound to a widget.
    /// <see cref="Widget"/> has been updated to the new widget.
    /// </summary>
    protected virtual Task OnMount() => Task.CompletedTask;
    
    /// <summary>
    /// Invoked when this state is bound to a widget.
    /// <see cref="Widget"/> hasn't been reset to null.
    /// </summary>
    protected virtual Task OnUnmount() => Task.CompletedTask;
    
    /// <summary>
    /// Invoked when this state is updated with a new widget.
    /// <see cref="Widget"/> hasn't been updated to the new widget.
    /// </summary>
    /// <param name="newWidget">New widget for this state to bind to.</param>
    protected virtual Task OnUpdate(Widget newWidget) => Task.CompletedTask;
    
    public async Task Mount(Widget widget)
    {
        if (IsMounted)
            throw new InvalidOperationException("This state has already been mounted.");
        
        Widget = widget;
        await OnMount();

        IsMounted = true;
    }

    public async Task Unmount()
    {
        if (!IsMounted)
            throw new InvalidOperationException("This element has not been mounted.");
        
        await OnUnmount();
        Widget = null!;

        IsMounted = false;
    }

    public async Task Update(Widget newWidget)
    {
        if (!Widget.AreSimilar(Widget, newWidget))
            throw new InvalidOperationException("Cannot update this state with the new widget.");
        await OnUpdate(newWidget);
        Widget = newWidget;
    }

    public ValueTask<Widget?> Build(BuildContext context) => OnBuild(context);
}

public abstract class State<TWidget> : State where TWidget : Widget
{
    /// <summary>
    /// Widget that owns this state.
    /// </summary>
    [DebuggerHidden]
    public new TWidget Widget => (TWidget)base.Widget;

    protected sealed override Task OnUpdate(Widget newWidget)
        => OnUpdate((TWidget)newWidget);
    
    /// <inheritdoc cref="State.OnUpdate"/>
    protected virtual Task OnUpdate(TWidget newWidget) => Task.CompletedTask;
}

/// <summary>
/// Stateful widgets have a corresponding state.
/// Same to other widgets, stateful widgets also replaced in the build process,
/// however their states are preserved and passed to the new instances of the corresponding stateful widgets.
/// </summary>
public abstract class StatefulWidget<TState> : Widget where TState : State
{
    public abstract TState NewState();

    public override Element NewElement() => new StatefulElement();
    
    private class StatefulElement : Element<StatefulWidget<TState>>
    {
        private TState _state = null!;

        private Element? _childElement;
        
        protected override async Task OnMount()
        {
            _state = Widget.NewState();
            await _state.Mount(Widget);
            var childWidget = await _state.Build(this);
            if (childWidget != null)
                _childElement = await MountChild(childWidget);
        }

        protected override async Task OnUpdate(StatefulWidget<TState> newWidget)
        {
            await _state.Update(newWidget);
            
            _childElement = await UpdateChild(_childElement, newWidget);
        }

        protected override async Task OnUnmount()
        {
            await _state.Unmount();
            if (_childElement != null)
                await _childElement.Unmount();
        }

        protected override Task OnCompile(ChatContext context)
        {
            return CompileChild(context, _childElement);
        }
    }
}