using System;

namespace DataAnalyticsPlatform.Service.Worker
{
    class Program
    {
        static void Main(string[] args)
        {
            var workerService = new WorkerService();
            workerService.Start();

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                workerService.Stop();
                eventArgs.Cancel = true;
            };

            workerService.WhenTerminated.Wait();
        }
    }
}
