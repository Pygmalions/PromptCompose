namespace PromptCompose.Robotics;

public enum BoxFit
{
    /// <summary>
    /// Scale down the content to fit within the box while maintaining the aspect ratio.
    /// The content may be smaller than the box.
    /// </summary>
    Shrink,
    /// <summary>
    /// Align the content in the center of the box without scaling.
    /// Content exceeding the box will be cropped. 
    /// </summary>
    Center,
    /// <summary>
    /// Scale the content to fill the box, distorting the aspect ratio if necessary.
    /// </summary>
    Fill,
    /// <summary>
    /// Align the content within the box without scaling.
    /// Content exceeding the box will be cropped. 
    /// </summary>
    Crop,
    /// <summary>
    /// Scale the content to fit within the box while maintaining the aspect ratio.
    /// Content would be put in the center of the box.
    /// </summary>
    Contain,
    /// <summary>
    /// Scale the content to cover the entire box while maintaining the aspect ratio.
    /// Content would be put in the center of the box; the part that exceeds the box will be cropped.
    /// </summary>
    Cover,
    /// <summary>
    /// Scale the content to fit the width of the box, potentially overflowing vertically.
    /// </summary>
    FitWidth,
    /// <summary>
    /// Scale the content to fit the width of the box, potentially overflowing horizontally.
    /// </summary>
    FitHeight,
}