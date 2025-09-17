using DataAnalyticsPlatform.Actors.Master;
using DataAnalyticsPlatform.Actors.System;
using DataAnalyticsPlatform.Readers;
using DataAnalyticsPlatform.Shared.Types;
using DataAnalyticsPlatform.Writers;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;

namespace DataAnalyticsPlatform.Application
{
    class Program
    {

        static void Main(string[] args)
        {

            Console.WriteLine("Press Enter to start...");

            Console.ReadLine();
            string sourcePath = @"FL_insurance_sample.csv";

            Post(@"http://localhost:50926/api/Preview/1/generatemodel", sourcePath);

            var rConf = new ReaderConfiguration(typeof(Model), typeof(ModelClassMap), sourcePath, SourceType.Csv);

            var wConf = new WriterConfiguration(DestinationType.Mongo, "", typeof(ModelClassMap));

            IngestionJob ingestionJob = new IngestionJob(1, rConf, wConf);

            DAP d = new DAP();

            d.Feed(ingestionJob);

            Console.ReadLine();
        }

        private static void Post(string url, string fileName)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {

                string json = JsonConvert.SerializeObject(new Body(fileName));
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
            }
        }
    }

    public class Body
    {
        public string FileName { get; set; }

        public Body(string fileName)
        {
            FileName = fileName;
        }
    }
}
