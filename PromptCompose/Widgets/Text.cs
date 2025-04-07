using System.Diagnostics;
using OpenAI.Chat;

namespace PromptCompose.Widgets;

[DebuggerDisplay("Text: {Content}")]
public class Text(string text) : PrimitiveWidget<Text>
{
    public string Content { get; } = text;

    public Text(params IEnumerable<string> texts) : this(string.Concat(texts))
    {}

    protected override IEnumerable<ChatMessageContentPart> OnCompile() =>
        [ChatMessageContentPart.CreateTextPart(Content)];

    protected override bool IsChanged(Text newWidget) => Content != newWidget.Content;
    
    /// <summary>
    /// Create a sentence with the given text. A sentence is separated with other sentences by a space.
    /// </summary>
    /// <param name="text">Text of the sentence.</param>
    /// <returns>Sentence ends with a blank space.</returns>
    public static Text Sentence(string text) => new(text + " ");
    
    public static Text Line(string text) => new(text + "\n");
    
    public static Text Paragraph(string text) => new(text + "\n\n");
}