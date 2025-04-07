using System.Diagnostics;
using PromptCompose.Framework;

namespace PromptCompose.Widgets;

/// <summary>
/// Child widget of a visibility widget will only be compiled when this widget is visible;
/// however, the child widget will still be mounted in the widget tree regardless of visibility.
/// In conclusion, 'visibility' is only for the LLM and decides whether the LLM can see the child widget.
/// </summary>
/// <param name="visible">Visible of the child widget.</param>
[DebuggerDisplay("Visible: {Visible}")]
public class Visibility(bool visible) : SingleChildSlotWidget<Visibility>
{
    public bool Visible { get; init; } = visible;

    public override Element NewElement() => new VisibilityElement();

    private class VisibilityElement : SingleChildSlotElement<Visibility>
    {
        protected override Task OnCompile(ChatContext context)
        {
            return Widget.Visible ? base.OnCompile(context) : Task.CompletedTask;
        }
    }
}