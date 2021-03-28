using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class Writer
    {
        public Writer()
        {
            ProjectWriters = new HashSet<ProjectWriter>();
        }

        public int WriterId { get; set; }
        public int WriterTypeId { get; set; }
        public int UserId { get; set; }
        public string DestinationPath { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public WriterType WriterType { get; set; }
        public ICollection<ProjectWriter> ProjectWriters { get; set; }
    }
}
