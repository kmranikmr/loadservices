using System;
using System.Collections.Generic;

namespace DataAccess.DTO
{
    public class JobSummaryDTO
    {
        public string ProjectName { get; set; }

        public int JobId { get; set; }

        public int CompletedFile { get; set; }

        public int TotalFile { get; set; }

        public List<FileDTO> FileList { get; set; }

    }

    public class FileDTO
    {
        public int JobId { get; set; }

        public int ProjectFileId { get; set; }

        public string Source { get; set; }

        public string InputFileName { get; set; }

        public string Status { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

    }
}
