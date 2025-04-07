using System.Diagnostics.CodeAnalysis;

namespace PromptCompose.Utilities;

public static class LinkedListExtensions
{
    public static bool TryPopFirstNode<TValue>(this LinkedList<TValue> list, 
        [NotNullWhen(true)] out LinkedListNode<TValue>? node)
    {
        node = list.First;
        if (node == null)
            return false;
        list.RemoveFirst();
        return true;
    }
    
    public static bool TryPopLastNode<TValue>(this LinkedList<TValue> list, 
        [NotNullWhen(true)] out LinkedListNode<TValue>? node)
    {
        node = list.Last;
        if (node == null)
            return false;
        list.RemoveLast();
        return true;
    }

    public static bool TryPopFirst<TValue>(this LinkedList<TValue> list, out TValue? value)
    {
        if (list.TryPopFirstNode(out var node))
        {
            value = node.Value;
            return true;
        }
        value = default;
        return false;
    }
    
    public static bool TryPopLast<TValue>(this LinkedList<TValue> list, out TValue? value)
    {
        if (list.TryPopLastNode(out var node))
        {
            value = node.Value;
            return true;
        }
        value = default;
        return false;
    }
    
    public static IEnumerable<LinkedListNode<TValue>> EnumerateNodes<TValue>(this LinkedList<TValue> list)
    {
        for (var node = list.First; node != null; node = node.Next)
        {
            yield return node;
        }
    }
}