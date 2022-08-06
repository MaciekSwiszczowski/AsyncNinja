using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using static Scenarios.Helper;

namespace Scenarios.AsyncVsSyncFileRead
{
    // WORK IN PROGRESS
    
    public class AsyncVsSyncFileRead //: IRunnable
    {
        public string Title { get; } = "Async vs sync file read";
        public Order Order { get; } = Order.AsyncFileRead;
        public string Comment { get; } = "to do";

        public async Task RunAsync()
        {
            const int bufferSize = 1000000;

            StartSpan("Read async");

            for (var i = 0; i < 100; i++)
                using (var sourceStream = new FileStream(@"AsyncVsSyncFileRead//LoremIpsum.txt", FileMode.Open,
                    FileAccess.Read, FileShare.Read, 4096,
                    true))
                {
                    var sb = new StringBuilder();

                    var buffer = new byte[bufferSize];
                    int numRead;
                    while ((numRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                    {
                        var text = Encoding.UTF8.GetString(buffer, 0, numRead);
                        sb.Append(text);
                    }

                    //Console.WriteLine(sb.ToString());
                }

            EndSpan("Read async");

            var consoleColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Gray;


            StartSpan("Read sync");

            for (var i = 0; i < 100; i++)
                using (var sourceStream = new FileStream(@"AsyncVsSyncFileRead//LoremIpsum.txt", FileMode.Open,
                    FileAccess.Read, FileShare.Read, 4096,
                    false))
                {
                    var sb = new StringBuilder();

                    var buffer = new byte[bufferSize];
                    int numRead;
                    while ((numRead = sourceStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        var text = Encoding.UTF8.GetString(buffer, 0, numRead);
                        sb.Append(text);
                    }

                    //Console.WriteLine(sb.ToString());
                }

            EndSpan("Read sync");

            Console.ForegroundColor = consoleColour;
        }
    }
}