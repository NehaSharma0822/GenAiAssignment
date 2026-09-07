using RagAssignment.Api.Models;

namespace RagAssignment.Api.Interfaces;

public interface IConversationMemory
{
    IReadOnlyList<ConversationTurn> GetHistory(
        string conversationId);

    void AddTurn(
        string conversationId,
        string question,
        string answer);

    void AddTurn(
        string conversationId,
        string question,
        string answer,
        IReadOnlyList<string> documentIds);
}