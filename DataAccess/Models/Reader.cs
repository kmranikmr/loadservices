using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class Reader
    {
        public Reader()
        {
            ProjectFiles = new HashSet<ProjectFile>();
            ProjectReaders = new HashSet<ProjectReader>();
            ProjectAutomations = new HashSet<ProjectAutomation>();
        }

        public int ReaderId { get; set; }
        public int ReaderTypeId { get; set; }
        public int UserId { get; set; }
        public string ReaderConfiguration { get; set; }
        public string ConfigurationName { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public ReaderType ReaderType { get; set; }
        public ICollection<ProjectAutomation> ProjectAutomations { get; set; }
        public ICollection<ProjectFile> ProjectFiles { get; set; }
        public ICollection<ProjectReader> ProjectReaders { get; set; }
    }
}
