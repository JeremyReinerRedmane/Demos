namespace DemoKatan.Demos;

public class TaskWhenAll
{
    private readonly CancellationTokenSource _cts = new();

    private const int TimeOut = 5000;

    public async Task ThrowIfCancelled()
    {
        Console.WriteLine($"Operating Thread: {Thread.CurrentThread.ManagedThreadId}\n");

        try
        {
            _cts.CancelAfter(TimeOut);

            var list = new List<Task> { Chicken(_cts), Nwaffles(_cts) };

            await Task.WhenAny(list);
        }
        catch (Exception ex)
        {
            // ignored
        }

        var result = _cts.IsCancellationRequested
            ? $"Task timed out after {TimeOut} seconds"
            : "Completed a task before the timeout";

        Console.ForegroundColor = _cts.IsCancellationRequested
            ? ConsoleColor.DarkRed
            : ConsoleColor.Green;

        Console.WriteLine(result);

        Console.ForegroundColor = ConsoleColor.White;

        _cts.Dispose();
    }

    public static async Task Chicken(CancellationTokenSource cts)
    {
        for (var i = 1; i < 20; i += 2)
        {
            Console.WriteLine($"Completed Thread 1 {Thread.CurrentThread.ManagedThreadId}: curr iter: {i}");

            await Task.Delay(1000, cts.Token);
        }
    }

    public static async Task Nwaffles(CancellationTokenSource cts)
    {
        for (var i = 0; i < 20; i += 2)
        {
            Console.WriteLine($"Completed Thread 2 {Thread.CurrentThread.ManagedThreadId}: curr iter: {i}");

            await Task.Delay(2000, cts.Token);
        }
    }
}