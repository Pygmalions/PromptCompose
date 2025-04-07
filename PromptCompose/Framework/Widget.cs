namespace PromptCompose.Framework;

public abstract class Widget
{
    /// <summary>
    /// The key to identify widgets.
    /// </summary>
    public object? Key { get; set; }

    /// <summary>
    /// Create a corresponding element for this widget.
    /// </summary>
    /// <returns>A new instance of a corresponding element.</returns>
    public abstract Element NewElement();
    
    /// <summary>
    /// Element of a widget can only be updated with another <b>similar</b> widget that: <br/>
    /// - Is the instance of the exactly same type of the previous widget. <br/>
    /// - Has a key equals to the key of the previous widget. <br/>
    /// </summary>
    /// <returns>True if the element of the old widget can be updated with the new widget.</returns>
    /// <seealso cref="Element.Update"/>
    public static bool AreSimilar(Widget oldWidget, Widget newWidget)
    {
        return oldWidget.GetType() == newWidget.GetType() &&
               (oldWidget.Key?.Equals(newWidget.Key) ?? newWidget.Key == null);
    }
    
    /// <summary>
    /// Wraps a widget into a <see cref="ValueTask{TResult}"/>.
    /// Allows conversion from a widget to a <see cref="ValueTask{TResult}"/> to reduce the complexity of the code.
    /// </summary>
    public static implicit operator ValueTask<Widget?>(Widget? widget) => ValueTask.FromResult(widget);
}