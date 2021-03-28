using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class ProjectWriter
    {
        public int ProjectId { get; set; }
        public int WriterId { get; set; }

        public Project Project { get; set; }
        public Writer Writer { get; set; }
    }
}
