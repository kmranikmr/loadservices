using System;

namespace DataAccess.Models
{
    public partial class Job
    {
        public int JobId { get; set; }
        public int ProjectFileId { get; set; }
        public int UserId { get; set; }
        public int JobStatusId { get; set; }
        public int ProjectId { get; set; }
        public string JobDescription { get; set; }
        public int? SchemaId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? StartedOn { get; set; }
        public DateTime? CompletedOn { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public JobStatus JobStatus { get; set; }
        public Project Project { get; set; }
        public ProjectFile ProjectFile { get; set; }
        public ProjectSchema Schema { get; set; }
    }
}
