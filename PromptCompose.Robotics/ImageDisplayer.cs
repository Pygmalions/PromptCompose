using System.Threading.Channels;
using OpenCvSharp;

namespace PromptCompose.Robotics;

public static class ImageDisplay
{
    private static (Task Thread, Channel<(string Name, Mat Image)> Channel)? _worker;

    public static bool IsRunning => _worker != null;

    public static void Start()
    {
        if (_worker != null)
            return;
        
        var channel = Channel.CreateUnbounded<(string Name, Mat Image)>();
        var thread = Task.Factory.StartNew(() =>
        {
            while (!channel.Reader.Completion.IsCompleted)
            {
                while (channel.Reader.TryRead(out var item))
                    Cv2.ImShow(item.Name, item.Image);
                Cv2.WaitKey(1);
            }
        }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        
        _worker = (thread, channel);
    }

    public static void Stop()
    {
        if (_worker == null)
            return;

        _worker.Value.Channel.Writer.Complete();
        _worker = null;
    }

    public static void Show(string name, Mat image)
    {
        if (_worker == null)
            throw new Exception("The image display thread is not running.");
        _worker.Value.Channel.Writer.TryWrite((name, image));
    }
}