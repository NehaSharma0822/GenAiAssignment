using System.Text.RegularExpressions;
using RagAssignment.Api.Interfaces;

namespace RagAssignment.Api.Services;

public class TextPreprocessor : ITextPreprocessor
{
    public string Clean(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        // Normalize different newline formats
        text = text
            .Replace("\r\n", "\n")
            .Replace("\r", "\n");

        // Process each line separately
        var lines = text
            .Split('\n')
            .Select(line =>
                Regex.Replace(line.Trim(), @"[ \t]+", " "))
            .Where(line => !string.IsNullOrWhiteSpace(line));

        // Reconstruct the text
        return string.Join("\n", lines);
    }
}