using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using PromptCompose.Framework;

namespace PromptCompose.Widgets;

[DebuggerDisplay("Sequence")]
public class Sequence : MultiChildSlotWidget<Sequence>, IEnumerable<Widget>
{
    private readonly List<Widget> _children = [];

    private Sequence()
    {
        Children = _children;
    }

    [SetsRequiredMembers]
    public Sequence(params IEnumerable<Widget> widgets) : this()
    {
        _children.AddRange(widgets);
    }

    public IEnumerator<Widget> GetEnumerator() => _children.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(Widget widget) => _children.Add(widget);

    public bool Remove(Widget widget) => _children.Remove(widget);
}

public static class SequenceExtensions
{
    public static Sequence ToSequence(this IEnumerable<Widget> widgets) => new(widgets);
}