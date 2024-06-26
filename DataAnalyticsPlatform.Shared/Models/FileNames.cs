namespace DataAnalyticsPlatform.Shared.Models
{
    public class FileNames
    {
        public int fileid { get; set; }
        public string filename { get; set; }
        public string processedtime { get; set; }

        public FileNames(int file_id, string filename, string time)
        {
            fileid = file_id;
            this.filename = filename;
            processedtime = time;
        }
    }
}
