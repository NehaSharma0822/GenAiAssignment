using System.Net.Http.Json;
using System.Text.Json;

var evaluations = new[]
{
    new EvaluationCase
    {
        Question = "What is the Transformer architecture?",
        ExpectedAnswer =
            "The Transformer uses stacked self-attention and position-wise fully connected layers in both the encoder and decoder."
    },

    new EvaluationCase
    {
        Question = "How many layers does the Transformer encoder have?",
        ExpectedAnswer =
            "The Transformer encoder is composed of a stack of N = 6 identical layers."
    },

    new EvaluationCase
    {
        Question = "What are the two sub-layers in each encoder layer?",
        ExpectedAnswer =
            "Each encoder layer contains a multi-head self-attention mechanism and a position-wise fully connected feed-forward network."
    },

    new EvaluationCase
    {
        Question = "What is multi-head attention?",
        ExpectedAnswer =
            "Multi-head attention allows the model to jointly attend to information from different representation subspaces at different positions."
    },

    new EvaluationCase
    {
        Question = "What is the role of positional encoding?",
        ExpectedAnswer =
            "Positional encoding provides information about the position or order of tokens in the sequence."
    },

    new EvaluationCase
    {
        Question = "Why does the decoder use masking?",
        ExpectedAnswer =
            "The decoder uses causal masking to prevent the model from attending to future tokens while generating the output."
    },

    new EvaluationCase
    {
        Question = "What is BERT's masked language model objective?",
        ExpectedAnswer =
            "BERT randomly masks some input tokens and trains the model to predict the original tokens."
    },

    new EvaluationCase
    {
        Question = "What is Next Sentence Prediction in BERT?",
        ExpectedAnswer =
            "Next Sentence Prediction is a task where BERT predicts whether two sentences or document segments are related in the original text."
    },

    new EvaluationCase
    {
        Question = "What is the main idea behind the T5 text-to-text framework?",
        ExpectedAnswer =
            "T5 treats every text processing problem as a text-to-text problem, taking text as input and producing text as output."
    }
};

using var httpClient = new HttpClient
{
    BaseAddress = new Uri("http://localhost:5195"),
    Timeout = TimeSpan.FromMinutes(3)
};

var results = new List<EvaluationResult>();

Console.WriteLine("RAG Evaluation");
Console.WriteLine("==============");
Console.WriteLine();

for (int i = 0; i < evaluations.Length; i++)
{
    var evaluation = evaluations[i];

    Console.WriteLine($"Question {i + 1}: {evaluation.Question}");

    using var request = new HttpRequestMessage(
        HttpMethod.Post,
        "/api/chat");

    request.Headers.Add(
    "X-Conversation-Id",
    $"evaluation-session-{i + 1}");

    request.Content = JsonContent.Create(
        new
        {
            message = evaluation.Question
        });

    try
    {
        var response = await httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<EvaluationResult>();

        if (result != null)
        {
            results.Add(result);

            Console.WriteLine("Answer:");
            Console.WriteLine(result.Answer);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR: {ex.Message}");

        results.Add(new EvaluationResult
        {
            Question = evaluation.Question,
            Answer = $"ERROR: {ex.Message}",
            Sources = []
        });
    }

    Console.WriteLine();
}

var jsonOptions = new JsonSerializerOptions
{
    WriteIndented = true
};

var outputPath = Path.Combine(
    AppContext.BaseDirectory,
    "evaluation-results.json");

await File.WriteAllTextAsync(
    outputPath,
    JsonSerializer.Serialize(results, jsonOptions));

Console.WriteLine("================================");
Console.WriteLine("Evaluation completed.");
Console.WriteLine($"Results saved to:");
Console.WriteLine(outputPath);


public class EvaluationCase
{
    public string Question { get; set; } = string.Empty;

    public string ExpectedAnswer { get; set; } = string.Empty;
}


public class EvaluationResult
{
    public string Question { get; set; } = string.Empty;

    public string Answer { get; set; } = string.Empty;

    public List<EvaluationSource> Sources { get; set; } = [];
}


public class EvaluationSource
{
    public string DocumentId { get; set; } = string.Empty;

    public int PageNumber { get; set; }

    public int ChunkIndex { get; set; }
}