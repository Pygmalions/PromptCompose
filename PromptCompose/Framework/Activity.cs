using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using OpenAI.Embeddings;

namespace PromptCompose.Framework;

public class Activity
{
    // Semaphore to ensure the element tree is not being modified by multiple thread at the same time.
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    // Root element of this activity.
    private Element? _root;

    public required Widget RootWidget { get; init; }

    public required ChatClient ChatClient { get; init; }
    
    public ILogger<Activity>? Logger { get; set; }

    /// <summary>
    /// Additional data of this context.
    /// </summary>
    public IDictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
    
    /// <summary>
    /// Whether this activity is currently running.
    /// </summary>
    public bool IsRunning => _semaphore.CurrentCount == 0;

    public async Task<ChatCompletion> Respond(
        ChatCompletionOptions? options = null,
        ChatClient? client = null)
    {
        await _semaphore.WaitAsync();
        
        await UpdateRootElement();
        
        var messages = await BuildChatMessages();
        
        var completion = 
            await (client ?? ChatClient).CompleteChatAsync(messages, options);
        
        _semaphore.Release();

        return completion;
    }

    public async IAsyncEnumerable<StreamingChatCompletionUpdate> RespondStreaming(
        ChatCompletionOptions? options = null, ChatClient? client = null)
    {
        await _semaphore.WaitAsync();

        await UpdateRootElement();
        
        var messages = await BuildChatMessages();
        
        await foreach (var update in (client ?? ChatClient).CompleteChatStreamingAsync(messages, options))
            yield return update;
        
        _semaphore.Release();
    }

    private async Task UpdateRootElement()
    {
        if (_root == null)
        {
            _root = RootWidget.NewElement();
            await _root.Mount(RootWidget, null, this);
        }
        else
        {
            await _root.Update(RootWidget);
        }
    }
    
    private async Task<IEnumerable<ChatMessage>> BuildChatMessages(
        Element? continuationParent = null,
        Widget? continuationWidget = null)
    {
        var context = new ChatContext(this);

        await _root!.Compile(context);

        if (continuationWidget != null)
        {
            var element = continuationWidget.NewElement();
            await element.Mount(continuationWidget, continuationParent, this);
            await element.Compile(context);
        }

        return context.Messages.Where(message => message.Content.Count > 0);
    }

    internal async Task<ChatCompletion> InternalRespond(
        Element? continuationParent = null,
        Widget? continuationWidget = null,
        ChatCompletionOptions? options = null)
    {
        var messages = await BuildChatMessages(continuationParent, continuationWidget);
        return (await ChatClient.CompleteChatAsync(messages, options)).Value;
    }

    internal async IAsyncEnumerable<StreamingChatCompletionUpdate> InternalRespondStreaming(
        Element? continuationParent = null,
        Widget? continuationWidget = null,
        ChatCompletionOptions? options = null)
    {
        var messages = await BuildChatMessages(continuationParent, continuationWidget);
        await foreach (var update in ChatClient.CompleteChatStreamingAsync(messages, options))
            yield return update;
    }
}