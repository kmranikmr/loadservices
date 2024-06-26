namespace DataAnalyticsPlatform.Shared.Models
{
    public class CustomConfiguration
    {
        public string readerName { get; set; }
    }
    public class CsvReaderConfiguration : CustomConfiguration
    {
        public string schemaStrategy { get; set; }
        public int skipLines { get; set; }
        public string delimiter { get; set; }
        public string quotes { get; set; }
    }
    public class TwitterAccess
    {

        public string key1 { get; set; }
        public string key2 { get; set; }
        public string key3 { get; set; }
        public string key4 { get; set; }
    }
    public class TwitterQuery
    {
        public string QueryJson { get; set; }
    }

    public class TwitterConfiguration : CustomConfiguration
    {
        public TwitterAccess twitterAccess { get; set; }
        public TwitterQuery twitterQuerry { get; set; }
        public int MaxSearchEntriesToReturn { get; set; }
        public int MaxTotalResults { get; set; }
        public int SinceId { get; set; }
    }

}
