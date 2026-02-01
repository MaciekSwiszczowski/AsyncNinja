using Microsoft.ConcurrencyVisualizer.Instrumentation;

namespace Scenarios;

public static class Helper
{
    public static readonly TimeSpan DefDelay = TimeSpan.FromSeconds(1);

    private static readonly Dictionary<string, Span> Spans = new();

    public static void StartSpan(string message)
    {
        Console.WriteLine($"Before: {message} on ThreadId: {Environment.CurrentManagedThreadId}");
        Spans.Add(message, Markers.EnterSpan(message));
    }

    public static void EndSpan(string message)
    {
        Spans[message].Leave();
        Spans.Remove(message);

        Console.WriteLine($"Ended: {message} on ThreadId: {Thread.CurrentThread.ManagedThreadId}");
    }
}