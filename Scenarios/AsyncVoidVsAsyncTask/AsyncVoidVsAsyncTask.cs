﻿#pragma warning disable VSTHRD100
#pragma warning disable VSTHRD200

using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Scenarios.AsyncVoidVsAsyncTask;

public class AsyncVoidVsAsyncTask : IRunnable
{
    public string Title => "async void vs async task";
    public Order Order => Order.AsyncVoidVsAsyncTask;

    public string Comment { get; } = "async void methods are pure evil!" + Environment.NewLine +
                                     Environment.NewLine +
                                     "The first exception is caught, even though it was fired from a different thread." +
                                     Environment.NewLine +
                                     "The second exception isn't caught and the application may even crash";

    // https://www.youtube.com/watch?v=bda13k0vfc0 36:00, 
    // https://www.youtube.com/watch?v=1S4_m5I_5gA 43 // example for exception with GetValueAsync.Wait()

    public async Task RunAsync()
    {
        // TODO: investigate - on a second run app crashes, unless I comment out the second try-catch. Why exactly?
        // TODO: fix the issue - app in an incorrect state after the first run.

        try
        {
            await ThrowExceptionFromAsyncTask();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Caught exception: {e.Message}");

            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }
        }

        try
        {
            ThrowExceptionFromAsyncVoid();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);

            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }
        } 
    }

    internal static async Task ThrowExceptionFromAsyncTask()
    {
        await Task.Run(static () =>
        {
            Console.WriteLine();
            Console.WriteLine("Throwing InvalidOperationException from an async task.");
            Console.WriteLine();

            throw new InvalidOperationException("Exception thrown in an async task.");
        });
    }

    internal static async void ThrowExceptionFromAsyncVoid()
    {
        Console.WriteLine();
        Console.WriteLine("Throwing InvalidOperationException from a void task.");
        Console.WriteLine();

        await Task.Run(static () => throw new InvalidOperationException("Catch me if you can!"));
    }
}