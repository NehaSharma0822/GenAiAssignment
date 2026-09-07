using RagAssignment.Api.Interfaces;
using RagAssignment.Api.Services;
using Qdrant.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddHttpClient<IOllamaService, OllamaService>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Ollama:BaseUrl"]
        ?? throw new InvalidOperationException(
            "Ollama:BaseUrl is not configured."));

    client.Timeout = TimeSpan.FromMinutes(3);
});
builder.Services.AddScoped<IPdfTextExtractor, PdfTextExtractor>();
builder.Services.AddHttpClient<PdfDownloadService>();
builder.Services.AddScoped<ITextPreprocessor, TextPreprocessor>();
builder.Services.AddScoped<IChunkingService, ChunkingService>();
builder.Services.AddOpenApi();
builder.Services.AddHttpClient<IEmbeddingService, OllamaEmbeddingService>(
    client =>
    {
        client.BaseAddress = new Uri(
            builder.Configuration["Ollama:BaseUrl"]
            ?? throw new InvalidOperationException(
                "Ollama:BaseUrl is not configured."));
    });
builder.Services.AddSingleton<QdrantClient>(
    _ => new QdrantClient(
        host: "localhost",
        port: 6334));
        
builder.Services.AddScoped<IQdrantService, QdrantService>();
builder.Services.AddScoped<IRetrievalService, RetrievalService>();
builder.Services.AddSingleton<
    IConversationMemory,
    ConversationMemoryService>();

var app = builder.Build();
app.MapControllers();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
