namespace RagQnA.Contracts.Interfaces;

public interface ICompletionClient
{
    Task<string> CompleteAsync(string systemPrompt, string userPrompt);
}
