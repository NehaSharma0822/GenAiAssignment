using System.Collections.Concurrent;
using RagAssignment.Api.Interfaces;
using RagAssignment.Api.Models;

namespace RagAssignment.Api.Services;

public class ConversationMemoryService : IConversationMemory
{
    private const int MaxTurns = 4;

    private readonly ConcurrentDictionary<
        string,
        List<ConversationTurn>> _conversations = new();

    public IReadOnlyList<ConversationTurn> GetHistory(
        string conversationId)
    {
        if (!_conversations.TryGetValue(
                conversationId,
                out var history))
        {
            return [];
        }

        lock (history)
        {
            return history.ToList();
        }
    }

    public void AddTurn(
        string conversationId,
        string question,
        string answer)
    {
        AddTurn(
            conversationId,
            question,
            answer,
            []);
    }

    public void AddTurn(
        string conversationId,
        string question,
        string answer,
        IReadOnlyList<string> documentIds)
    {
        var history = _conversations.GetOrAdd(
            conversationId,
            _ => []);

        lock (history)
        {
            history.Add(new ConversationTurn
            {
                Question = question,
                Answer = answer,
                DocumentIds = documentIds
                    .Distinct()
                    .ToList()
            });

            while (history.Count > MaxTurns)
            {
                history.RemoveAt(0);
            }
        }
    }
}