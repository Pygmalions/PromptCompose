using OpenCvSharp;

namespace PromptCompose.Robotics;

public static class ImageUtility
{
    private static Mat FitNone(this Mat image, Size size)
    {
        var cropRegion = new Rect(0, 0,
            Math.Min(size.Width, image.Width),
            Math.Min(size.Width, image.Height));
        
        image = new Mat(image, cropRegion);
        
        var paddingRight = size.Width - image.Width;
        var paddingBottom = size.Height - image.Height;
        if (paddingRight > 0 || paddingBottom > 0)
            image = image.CopyMakeBorder(0, paddingBottom, 0, paddingRight,
                BorderTypes.Constant);
        return image;
    }
    
    private static Mat FitCenter(this Mat image, Size size, bool padding = true)
    {
        // Crop the image to fit within the box.
        var cropRegion = new Rect(0, 0, image.Width, image.Height);
        
        if (cropRegion.Width > size.Width)
        {
            cropRegion.X = (cropRegion.Width - size.Width) / 2;
            cropRegion.Width = size.Width;
        }

        if (cropRegion.Height > size.Height)
        {
            cropRegion.Y = (cropRegion.Height - size.Height) / 2;
            cropRegion.Height = size.Height;
        }

        image = new Mat(image, cropRegion);

        if (!padding)
            return image;
        
        // Add padding to fill the remaining space of the box.
        var differenceHeight = size.Height - image.Height;
        var differenceWidth = size.Width - image.Width;

        if (differenceHeight == 0 && differenceWidth == 0)
            return image;
        
        var paddingTop = (int)(differenceHeight / 2.0);
        var paddingLeft = (int)(differenceWidth / 2.0);
        
        return image.CopyMakeBorder(paddingTop, differenceHeight - paddingTop,
            paddingLeft, differenceWidth - paddingLeft, BorderTypes.Constant);
    }

    private static Mat FitWidth(this Mat image, Size size)
    {
        return image.Resize(new Size(size.Width, (double)image.Height / image.Width * size.Width));
    }

    private static Mat FitHeight(this Mat image, Size size)
    {
        return image.Resize(new Size((double)image.Width / image.Height * size.Height, size.Height));
    }

    private static Mat FitContain(this Mat image, Size size, bool padding = true)
    {
        if (image.Width <= size.Width && image.Height <= size.Height)
            return FitCenter(image, size, padding);
                
        var scaleByWidth = (double)size.Width / image.Width;
        var scaleByHeight  = (double)size.Height / image.Height;
        // Use the smaller scale to scale the image.
        image = scaleByWidth <= scaleByHeight ? 
            FitWidth(image, size) : FitHeight(image, size);

        return !padding ? image :
            // Add padding to fill the remaining space of the box.
            FitCenter(image, size, padding);
    }

    private static Mat FitCover(this Mat image, Size size)
    {
        if (image.Width >= size.Width && image.Height >= size.Height)
            return FitCenter(image, size);
                
        var scaleByWidth = (double)size.Width / image.Width;
        var scaleByHeight  = (double)size.Height / image.Height;
        // Use the bigger scale to scale the image.
        image = scaleByWidth >= scaleByHeight ? 
            FitWidth(image, size) : FitHeight(image, size);

        // Crop the image to fit within the box.
        return FitCenter(image, size, false);
    }

    public static Mat Fit(this Mat image, Size size, BoxFit fit)
    {
        switch (fit)
        {
            case BoxFit.Fill:
                return image.Resize(size);
            case BoxFit.Shrink:
                return FitContain(image, size, false);
            case BoxFit.Crop:
                return FitNone(image, size);
            case BoxFit.Center:
                return FitCenter(image, size);
            case BoxFit.FitWidth:
                return FitWidth(image, size);
            case BoxFit.FitHeight:
                return FitHeight(image, size);
            case BoxFit.Contain:
                return FitContain(image, size);
            case BoxFit.Cover:
                return FitCover(image, size);
            default:
                throw new ArgumentOutOfRangeException(nameof(fit), fit, $"Unsupported fit mode: '{fit}'.");
        }
    }
}