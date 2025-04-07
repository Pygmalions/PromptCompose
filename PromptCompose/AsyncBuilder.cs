using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using PromptCompose.Framework;

namespace PromptCompose;

[DebuggerDisplay("AsyncBuilder")]
public sealed class AsyncBuilder() : AsyncWidget
{
    public required Func<BuildContext, ChatContext, ValueTask<Widget?>> Builder { get; init; }
    
    [SetsRequiredMembers]
    public AsyncBuilder(Func<BuildContext, ChatContext, ValueTask<Widget?>> builder) : this()
    {
        Builder = builder;
    }

    protected override ValueTask<Widget?> OnBuild(BuildContext buildContext, ChatContext chatContext)
        => Builder(buildContext, chatContext);
}