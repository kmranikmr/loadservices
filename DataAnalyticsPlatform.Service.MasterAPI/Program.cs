using DataAnalyticsPlatform.Service.MasterAPI;
using System;

namespace DataAnalyticsPlatform.MasterAPI
{
    class Program
    {
        private static void Main(string[] args)
        {
            var masterApiService = new MasterService();



            masterApiService.Start();

            //Console.ReadLine();

            //masterApiService.SpawnMaster();

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                masterApiService.Stop();
                eventArgs.Cancel = true;
            };

            masterApiService.WhenTerminated.Wait();
        }
    }
}
