using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Scenarios.Channels
{
    //
    // https://www.youtube.com/watch?v=ZPZTa3iLXNY
    // https://deniskyashif.com/2019/12/08/csharp-channels-part-1/, https://deniskyashif.com/2019/12/11/csharp-channels-part-2/, https://deniskyashif.com/2020/01/07/csharp-channels-part-3/

    public class Channels : IRunnable
    {
        public string Title { get; } = "Channels";
        public int Order { get; } = 400;
        public string Comment { get; } = "Solves producer/consumer problems";
        public async Task RunAsync()
        {

            //var channel = Channel.CreateBounded<int>(10);

            //var _ = Task.Run(async () =>
            //{
            //    for (var i = 0; i < 10; i++)
            //    {
            //        await channel.Writer.WriteAsync(i);
            //    }

            //    channel.Writer.Complete();
            //});

            //await foreach (var i in channel.Reader.ReadAllAsync())
            //{
            //    Console.WriteLine($"Received: {i}");
            //}

            //Console.WriteLine("Received all!");


            var channel = Channel.CreateBounded<Action>(2);

            var _ = Task.Run(async () =>
            {
                for (var i = 0; i < 6; i++)
                {
                    var index = i;
                    Console.WriteLine($"Sending: {index}");
                    await channel.Writer.WriteAsync(() => Console.WriteLine($"Action: {index}"));
                    await Task.Delay(300);
                }

                channel.Writer.Complete();
            });

            await Task.Delay(1000);
            Console.WriteLine("Starting reading.");

            await foreach (var i in channel.Reader.ReadAllAsync())
            {
                Console.WriteLine($"New item received.");
                i.Invoke();
            }

            Console.WriteLine("All was read!");
        }
    }
}
