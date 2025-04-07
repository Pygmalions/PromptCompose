using PromptCompose.Framework;

namespace PromptCompose.Widgets;

public class Builder : StatelessWidget
{
    public required Func<BuildContext, Widget?> Child { get; init; }

    protected override ValueTask<Widget?> OnBuild(BuildContext context) => Child(context);
}