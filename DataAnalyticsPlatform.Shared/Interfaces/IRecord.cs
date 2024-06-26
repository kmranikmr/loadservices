namespace DataAnalyticsPlatform.Shared.Interfaces
{
    public interface IRecord
    {
        object Instance { get; set; }
        long RecordId { get; set; }
        long FileId { get; set; }
        //Entity Instance { get; set; }

        string FileName { get; set; }
    }
}
