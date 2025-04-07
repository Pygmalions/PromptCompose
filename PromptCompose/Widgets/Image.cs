using System.Diagnostics;
using System.Net.Mime;
using JetBrains.Annotations;
using OpenAI.Chat;

namespace PromptCompose.Widgets;

public abstract class Image : PrimitiveWidget<Image>
{
    /// <summary>
    /// The media type of this image.
    /// Some common media types are defined in <see cref="MediaTypeNames.Image"/>.
    /// Currently, some LLM can still identify the image even if the media type is incorrect;
    /// such as a JPEG image with a PNG media type.
    /// </summary>
    /// <remarks>
    /// This property must be set to a valid MIME type, otherwise LLMs cannot identify the content.
    /// </remarks>
    [ValueProvider("System.Net.Mime.MediaTypeNames.Image")]
    public string MediaType { get; init; } = MediaTypeNames.Image.Jpeg;

    /// <summary>
    /// The detail level controls over how the model processes the image and generates its textual understanding.
    /// By default, the model will use the <see cref="ChatImageDetailLevel.Auto"/> setting,
    /// which looks at the image input size and decides if it should use the low or high setting.
    /// <br/><br/>
    /// <see cref="ChatImageDetailLevel.Low"/> enables the "low res" mode.
    /// The model receives a low-resolution 512px x 512px version of the image.
    /// It represents the image with a budget of 85 tokens.
    /// This allows the API to return faster responses and consume fewer input tokens for use cases
    /// that do not require high detail.
    /// <br/><br/>
    /// <see cref="ChatImageDetailLevel.High"/> enables "high resolution" mode, which first lets the model see the low-resolution image (using 85 tokens)
    /// and then creates detailed crops using 170 tokens for each 512px x 512px tile.
    /// </summary>
    public ChatImageDetailLevel DetailLevel { get; init; } = ChatImageDetailLevel.Auto;
}

[DebuggerDisplay("Image: {Uri}")]
public class UriImage : Image
{
    public required Uri Uri { get; init; }

    protected override bool IsChanged(Image newWidget)
        => newWidget is not UriImage uriImage || Uri != uriImage.Uri || DetailLevel != uriImage.DetailLevel;

    protected override IEnumerable<ChatMessageContentPart> OnCompile()
    {
        return [ChatMessageContentPart.CreateImagePart(Uri, DetailLevel)];
    }
}

[DebuggerDisplay("Image: [Bytes]")]
public class BytesImage : Image
{
    public required ReadOnlyMemory<byte> Bytes { get; init; }

    protected override bool IsChanged(Image newWidget)
        => newWidget is not BytesImage bytesImage || bytesImage.Bytes.Span != Bytes.Span;

    protected override IEnumerable<ChatMessageContentPart> OnCompile()
    {
        return [ChatMessageContentPart.CreateImagePart(BinaryData.FromBytes(Bytes), MediaType, DetailLevel)];
    }
}