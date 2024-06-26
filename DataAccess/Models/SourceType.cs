using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class SourceType
    {
        public SourceType()
        {
            ProjectFiles = new HashSet<ProjectFile>();
        }

        public int SourceTypeId { get; set; }
        public string SourceTypeName { get; set; }

        public ICollection<ProjectFile> ProjectFiles { get; set; }
    }
}
