using System.Collections;
using DemoKatan.Static;
using static System.Threading.Thread;
using EventHandler = DemoKatan.Entities.EventHandler;

namespace DemoKatan.Demos;

public class InvokeEvent
{
    private readonly CancellationTokenSource _cts = new();

    private readonly Queue<ConsoleKeyInfo> _keyPresses = new(); // Not thread-safe, use ConcurrentQueue in practice

    private const int TimeOut = 10000;

    public void Run()
    {
        $"Operating Thread: {CurrentThread.ManagedThreadId}\n".Print();

        var test = new EventHandler(_cts);

        _cts.CancelAfter(TimeOut);

        test.MyEvent += async (sender, e) => await FirstRaisedEvent("1st", e);

        "Invoking events".Print();

        test.Raise(EventArgs.Empty);

        "Press Enter to dispose cancellation token and close threads".PrintCancelled();

        OpenKeyThread();

        var i = 0;

        while (true)
        {
            if (MainLoop(i).Result)
                break;
            
            i ++;
        }

        _cts.Dispose();

        "Token Disposed".PrintDisposed();
    }

    private async Task<bool> MainLoop(int i)
    {
        await Task.Delay(1000);

        $"Main: Operating Thread: {CurrentThread.ManagedThreadId}\nCurrent iteration: {i}\n".Print();

        if (_cts.Token.IsCancellationRequested)
        {
            "Event handler cancelled. From 'Main'".PrintCancelled();
            return true;
        }

        // Check for pressed keys periodically
        if (_keyPresses.Count <= 0)
            return false;

        var keyInfo = _keyPresses.Dequeue();

        $"Button pressed: {keyInfo.KeyChar}.".PrintAlert();

        // Handle key press (e.g., exit loop)
        if (keyInfo.Key != ConsoleKey.Enter) 
            return false;
            
        $"Button pressed: {keyInfo.KeyChar}. Disposing Token...".PrintDisposed();

        return true;
    }

    private async Task SecondRaisedMethod(object sender, EventArgs e)
    {
        var x = (string)sender;

        await NotMainLoop(x, 200);
    }

    private async Task FirstRaisedEvent(object sender, EventArgs e)
    {
        var x = (string)sender;

        await NotMainLoop(x, 100);
    }

    private async Task NotMainLoop(string x, int i)
    {
        var delay = x switch
        {
            "1st" => 2000,
            "2nd" => 3000,
            _ => 1500
        };
        try
        {
            while (true)
            {
                // Check for cancellation periodically
                if (_cts.Token.IsCancellationRequested)
                {
                    $"Event handler cancelled. From '{x}' invoked method".PrintCancelled();
                    break;
                }

                await Task.Delay(delay);

                $"{x}: Operating Thread: {CurrentThread.ManagedThreadId}\nCurrent iteration: {i}\n".Print();

                i += 2;
            }
        }
        catch (ObjectDisposedException ex)
        {
            $"ObjectDisposedException: Cancellation token disposed. From '{x}' invoked method".PrintAlert();
        }
    }

    private void OpenKeyThread()
    {
        var keyThread = new Thread(ReadKeys);

        keyThread.Start();
    }

    /// <summary>
    /// Not thread-safe, use thread-safe queue in practice
    /// </summary>
    private void ReadKeys()
    {
        while (true)
        {
            if (Console.KeyAvailable)
            {
                _keyPresses.Enqueue(Console.ReadKey());
            }
        }
    }

}
