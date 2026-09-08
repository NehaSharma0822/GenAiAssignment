using System.Net.Http.Json;
using System.Text.Json;

var evaluations = new[]
{
    new EvaluationCase
    {
        Question = "What is the Transformer architecture?",
        ExpectedKeywords =
        [
            "attention",
            "encoder",
            "decoder"
        ]
    },

    new EvaluationCase
    {
        Question = "How many layers does the Transformer encoder have?",
        ExpectedKeywords =
        [
            "6",
            "layers"
        ]
    },

    new EvaluationCase
    {
        Question = "What are the two sub-layers in each encoder layer?",
        ExpectedKeywords =
        [
            "multi-head self-attention",
            "feed-forward"
        ]
    },

    new EvaluationCase
    {
        Question = "What is multi-head attention?",
        ExpectedKeywords =
        [
            "different representation subspaces",
            "positions",
            "heads"
        ]
    },

    new EvaluationCase
    {
        Question = "What is the role of positional encoding?",
        ExpectedKeywords =
        [
            "position",
            "sequence"
        ]
    },

    new EvaluationCase
    {
        Question = "Why does the decoder use masking?",
        ExpectedKeywords =
        [
            "future",
            "attending"
        ]
    },

    new EvaluationCase
    {
        Question = "What is BERT's masked language model objective?",
        ExpectedKeywords =
        [
            "masked",
            "tokens",
            "reconstruct"
        ]
    },

    new EvaluationCase
    {
        Question = "What is Next Sentence Prediction in BERT?",
        ExpectedKeywords =
        [
            "predict",
            "document",
            "segments"
        ]
    },

    new EvaluationCase
    {
        Question = "What is the main idea behind the T5 text-to-text framework?",
        ExpectedKeywords =
        [
            "text-to-text",
            "input",
            "output"
        ]
    },

    // Q10 deliberately uses "its" to test conversational memory.
    // It should refer to T5 from Question 9.
    new EvaluationCase
    {
        Question = "How many layers does its encoder have?",
        ExpectedKeywords =
        [
            "12",
            "layers"
        ]
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

    // One conversation ID is intentionally used for all 10 questions.
    // This allows Q10 to test conversational memory from Q9.
    request.Headers.Add(
        "X-Conversation-Id",
        "evaluation-session-1");

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
            var score = CalculateScore(
                result.Answer,
                evaluation.ExpectedKeywords);

            result.ExpectedKeywords =
                evaluation.ExpectedKeywords;

            result.KeywordScore = score;

            results.Add(result);

            Console.WriteLine("Answer:");
            Console.WriteLine(result.Answer);

            Console.WriteLine(
                $"Evaluation Score: {score}/100");

            Console.WriteLine(
                $"Sources Retrieved: {result.Sources.Count}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR: {ex.Message}");

        results.Add(new EvaluationResult
        {
            Question = evaluation.Question,
            Answer = $"ERROR: {ex.Message}",
            Sources = [],
            ExpectedKeywords = evaluation.ExpectedKeywords,
            KeywordScore = 0
        });
    }

    Console.WriteLine();
}

var jsonOptions = new JsonSerializerOptions
{
    WriteIndented = true
};

// Save evaluation results in the project directory rather than bin/.
var projectDirectory = Path.GetFullPath(
    Path.Combine(
        AppContext.BaseDirectory,
        "../../../../"));

var outputPath = Path.Combine(
    projectDirectory,
    "evaluation-results.json");

await File.WriteAllTextAsync(
    outputPath,
    JsonSerializer.Serialize(results, jsonOptions));

Console.WriteLine("================================");
Console.WriteLine("Evaluation completed.");
Console.WriteLine($"Questions evaluated: {results.Count}");
Console.WriteLine($"Results saved to:");
Console.WriteLine(outputPath);


// ------------------------------------------------------------
// Self-devised evaluation metric
// ------------------------------------------------------------
//
// The assignment allows a self-devised metric instead of RAGAS
// or TruLens.
//
// Each expected keyword/concept found in the generated answer
// contributes equally to the score.
//
// Example:
// Expected keywords = ["text-to-text", "input", "output"]
//
// If answer contains all three:
// Score = 100
//
// If answer contains two:
// Score = 66.67
//
// This is a simple lexical correctness indicator.
// Source count is also recorded separately to indicate whether
// retrieval returned supporting document chunks.
// ------------------------------------------------------------

static double CalculateScore(
    string answer,
    List<string> expectedKeywords)
{
    if (string.IsNullOrWhiteSpace(answer) ||
        expectedKeywords.Count == 0)
    {
        return 0;
    }

    var normalizedAnswer =
        answer.ToLowerInvariant();

    var matched = expectedKeywords.Count(keyword =>
        normalizedAnswer.Contains(
            keyword.ToLowerInvariant()));

    return Math.Round(
        matched * 100.0 / expectedKeywords.Count,
        2);
}


public class EvaluationCase
{
    public string Question { get; set; } = string.Empty;

    public List<string> ExpectedKeywords { get; set; } = [];
}


public class EvaluationResult
{
    public string Question { get; set; } = string.Empty;

    public string Answer { get; set; } = string.Empty;

    public List<EvaluationSource> Sources { get; set; } = [];

    public List<string> ExpectedKeywords { get; set; } = [];

    public double KeywordScore { get; set; }
}


public class EvaluationSource
{
    public string DocumentId { get; set; } = string.Empty;

    public int PageNumber { get; set; }

    public int ChunkIndex { get; set; }
}