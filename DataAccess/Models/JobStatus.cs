using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class JobStatus
    {
        public JobStatus()
        {
            Jobs = new HashSet<Job>();
        }

        public int JobStatusId { get; set; }
        public string StatusName { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public ICollection<Job> Jobs { get; set; }
    }
}
