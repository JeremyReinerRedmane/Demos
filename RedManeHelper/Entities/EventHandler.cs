namespace DemoKatan.Entities;

public class EventHandler
{
    private CancellationTokenSource _cts;
    public EventHandler(CancellationTokenSource cts)
    {
        _cts = cts;
    }

    public delegate void MyEventHandler(object sender, EventArgs e); 

    public event MyEventHandler MyEvent; 

    public void Raise(EventArgs e)
    {
        MyEvent.Invoke(this, e);
    }
}