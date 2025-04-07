using System.Diagnostics;
using System.Net.Mime;
using JetBrains.Annotations;
using OpenAI.Chat;
using OpenCvSharp;
using PromptCompose.Framework;
using PromptCompose.Widgets;

namespace PromptCompose.Robotics;

/// <summary>
/// This widget can retrieve an image from a camera or a video file.
/// Change the key of this widget if you want to play the video from the beginning,
/// otherwise the video will be played continuously.
/// </summary>
public abstract class VideoCaptureImage : StatefulWidget<VideoCaptureImage.VideoCaptureImageState>
{
    /// <summary>
    /// Target size for the image.
    /// If this is not null, the image will be resized to the specified size
    /// according to the fit mode specified in <see cref="Fit"/>
    /// </summary>
    public Size? Size { get; init; }

    /// <summary>
    /// Specify how the image should be resized to the specified <see cref="Size"/>.
    /// Default is <see cref="BoxFit.Shrink"/>
    /// </summary>
    public BoxFit Fit { get; init; } = BoxFit.Shrink;
    
    /// <summary>
    /// The quality of the image when it is encoded to a JPEG buffer.
    /// Value range is from 0 to 100, default is 95, the higher, the better.
    /// </summary>
    [ValueRange(0, 100)]
    public int Quality { get; init; } = 95;
    
    public ChatImageDetailLevel DetailLevel { get; init; } = ChatImageDetailLevel.Auto;
    
    protected abstract bool IsCaptureChanged(VideoCaptureImage newWidget);

    protected abstract VideoCapture NewCapture();

    public override VideoCaptureImageState NewState() => new();
    
    public class VideoCaptureImageState : State<VideoCaptureImage>
    {
        private VideoCapture? _capture = null;

        private Mat _image = new();
        
        protected override Task OnMount()
        {
            _capture = Widget.NewCapture();
            
            return Task.CompletedTask;
        }

        protected override Task OnUnmount()
        {
            _capture?.Release();
            _capture?.Dispose();
            _capture = null;

            return Task.CompletedTask;
        }

        protected override Task OnUpdate(VideoCaptureImage newWidget)
        {
            if (!Widget.IsCaptureChanged(newWidget))
                return Task.CompletedTask;
            _capture?.Release();
            _capture?.Dispose();
            _capture = newWidget.NewCapture();

            return Task.CompletedTask;
        }
        
        protected override ValueTask<Widget?> OnBuild(BuildContext context)
        {
            if (_capture == null)
                throw new Exception("This state has not been bound to a widget.");
            
            while (!_capture.Read(_image))
            {
                Console.Write("Cannot read image from the video capture, retrying...");
            }
            
            if (Widget.Size != null)
                _image = _image.Fit(Widget.Size.Value, Widget.Fit);
            
            Cv2.ImEncode(".jpg", _image, out var buffer,
                new ImageEncodingParam(ImwriteFlags.JpegQuality, Widget.Quality)
            );
            
            return new BytesImage
            {
                Bytes = buffer, 
                MediaType = MediaTypeNames.Image.Jpeg,
                DetailLevel = Widget.DetailLevel
            };
        }
    }
}

[DebuggerDisplay("Video Capture: Camera {CameraIndex}")]
public class CameraImage(int cameraIndex) : VideoCaptureImage
{
    public int CameraIndex => cameraIndex;

    protected override VideoCapture NewCapture() => new(cameraIndex);

    protected override bool IsCaptureChanged(VideoCaptureImage newWidget)
        => newWidget is not CameraImage cameraVideoImage ||
           cameraVideoImage.CameraIndex != CameraIndex;
}

[DebuggerDisplay("Video Capture: Path '{VideoPath}'")]
public class VideoImage(string videoPath) : VideoCaptureImage
{
    public string VideoPath => videoPath;

    protected override VideoCapture NewCapture() => new(videoPath);

    protected override bool IsCaptureChanged(VideoCaptureImage newWidget)
        => newWidget is not VideoImage fileVideoImage ||
           fileVideoImage.VideoPath != VideoPath;
}