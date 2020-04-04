using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Scenarios.AsyncVoidVsAsyncTask
{
    public class AsyncVoidVsAsyncTask : IRunnable
    {
        public string Title { get; } = "async void vs async task";
        public Order Order { get; } = Order.AsyncVoidVsAsyncTask;

        public string Comment { get; } = "async void methods are pure evil!" + Environment.NewLine +
                                         Environment.NewLine +
                                         "The first exception is caught, even though it was fired from a different thread." +
                                         Environment.NewLine +
                                         "The second exception isn't caught and the application may even crash";

        // https://www.youtube.com/watch?v=bda13k0vfc0 36:00, 

        // https://www.youtube.com/watch?v=1S4_m5I_5gA 43 // example for exception with GetValueAsync.Wait()

        //public async Task RunAsync()
        //{
        //    try
        //    {
        //        var task1 = ThrowExceptionFromAsyncTask();
        //        var task2 = ThrowExceptionFromAsyncTask();

        //        await task1;
        //        await task2;
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);

        //        if (Debugger.IsAttached)
        //        {
        //            Debugger.Break();
        //        }
        //    }
        //}

        public async Task RunAsync()
        {
            // TODO: investigate - on a second run app crasher, unless I comment out the second try-catch. Why exactly?

            try
            {
                await ThrowExceptionFromAsyncTask();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                if (Debugger.IsAttached) Debugger.Break();
            }

            try
            {
                ThrowExceptionFromAsyncVoid();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                if (Debugger.IsAttached) Debugger.Break();
            }
        }

        internal static async Task ThrowExceptionFromAsyncTask()
        {
            await Task.Run(() =>
            {
                Console.WriteLine();
                Console.WriteLine("Throwing InvalidOperationException");
                Console.WriteLine();

                throw new InvalidOperationException();
            });
        }

        internal static async void ThrowExceptionFromAsyncVoid()
        {
            Console.WriteLine();
            Console.WriteLine("Throwing InvalidOperationException");
            Console.WriteLine();

            await Task.Run(() => throw new InvalidOperationException("Catch me if you can!"));
        }
    }
}