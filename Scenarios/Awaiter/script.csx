#r "PresentationFramework"
#r "Microsoft.VisualStudio.Threading"

using System.Windows.Controls;
using System.Windows;
using System.Threading;
using System.Runtime.CompilerServices;


public struct ControlAwaiter : INotifyCompletion
{
    private readonly Control _control;

    public ControlAwaiter(Control control)
    {
        _control = control;
    }

    public bool IsCompleted => _control.Dispatcher.CheckAccess();

    public void OnCompleted(Action continuation)
    {
        _control.Dispatcher.BeginInvoke(continuation);
    }

    public void GetResult() { }
}



public static ControlAwaiter GetAwaiter(this Control control)
{
    return new ControlAwaiter(control);
}


Window window = null;
TextBox t = null;

var newWindowThread = new Thread(() =>
{
    window = new Window();
    t = new TextBox { Text = $"{SynchronizationContext.Current == null}" };
    window.Content = t;
    window.Topmost = true;
    window.Width = 300;
    window.Height = 200;
    window.Show();

    WriteLine($@"New window ThreadId: {Thread.CurrentThread.ManagedThreadId}");

    System.Windows.Threading.Dispatcher.Run();

});

newWindowThread.SetApartmentState(ApartmentState.STA);
newWindowThread.IsBackground = true;
newWindowThread.Start();

await Task.Delay(3000);

WriteLine($@"Main ThreadId: {Thread.CurrentThread.ManagedThreadId}");

await window;

WriteLine($@"after await ThreadId: {Thread.CurrentThread.ManagedThreadId}");

await window.Dispatcher.InvokeAsync(() => WriteLine($@"Dispatcher ThreadId: {Thread.CurrentThread.ManagedThreadId}"));

for (var i = 0; i < 10; i++)
{
    t.Text = i.ToString();
    await Task.Delay(1000);
    WriteLine($@"after await ThreadId: {Thread.CurrentThread.ManagedThreadId}");
}

await Task.Delay(3000);



