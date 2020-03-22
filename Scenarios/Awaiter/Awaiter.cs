using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Scenarios.Awaiter
{
    public class Awaiter : IRunnable
    {
        public string Title { get; } = "Awaiter";
        public int Order { get; } = 600;

        public string Comment { get; } =
            "Awaiter that changes to UI context. Non production code. Useful for csi.exe (?)";

        public async Task RunAsync()
        {
            // ******************************** 

            // better: https://docs.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.threading.joinabletaskfactory.switchtomainthreadasync?view=visualstudiosdk-2017
            // await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            // use in the runner?

            // ******************************** ???

            Window window = null;
            TextBox t = null;

            var newWindowThread = new Thread(() =>
            {
                window = new Window();
                t = new TextBox {Text = $"{SynchronizationContext.Current == null}"};
                window.Content = t;
                window.Topmost = true;
                window.Width = 300;
                window.Height = 200;
                window.Show();

                Console.WriteLine($@"New window ThreadId: {Thread.CurrentThread.ManagedThreadId}");

                Dispatcher.Run();
            });

            newWindowThread.SetApartmentState(ApartmentState.STA);
            newWindowThread.IsBackground = true;
            newWindowThread.Start();

            await Task.Delay(3000);

            Console.WriteLine($@"Main ThreadId: {Thread.CurrentThread.ManagedThreadId}");

            //await window.Dispatcher.InvokeAsync(() => synchronizationContext = SynchronizationContext.Current);

            await window;

            Console.WriteLine($@"after await ThreadId: {Thread.CurrentThread.ManagedThreadId}");

            await window.Dispatcher.InvokeAsync(() =>
                Debug.WriteLine($@"Dispatcher ThreadId: {Thread.CurrentThread.ManagedThreadId}"));


            await Task.Delay(1000);
        }
    }

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

        public void GetResult()
        {
        }
    }

    public static class Extension
    {
        public static ControlAwaiter GetAwaiter(this Control control)
        {
            return new ControlAwaiter(control);
        }
    }
}