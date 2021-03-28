using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class Project
    {
        public Project()
        {
            Jobs = new HashSet<Job>();
            ModelMetadatas = new HashSet<ModelMetadata>();
            ProjectFiles = new HashSet<ProjectFile>();
            ProjectReaders = new HashSet<ProjectReader>();
            ProjectSchemas = new HashSet<ProjectSchema>();
            ProjectUsers = new HashSet<ProjectUser>();
            ProjectWriters = new HashSet<ProjectWriter>();
            SchemaModels = new HashSet<SchemaModel>();
            ProjectAutomations = new HashSet<ProjectAutomation>();
        }

        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string ProjectDescription { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastAccessedOn { get; set; }
        public bool IsFavorite { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public ICollection<Job> Jobs { get; set; }
        public ICollection<ModelMetadata> ModelMetadatas { get; set; }
        public ICollection<ProjectFile> ProjectFiles { get; set; }
        public ICollection<ProjectReader> ProjectReaders { get; set; }
        public ICollection<ProjectSchema> ProjectSchemas { get; set; }
        public ICollection<ProjectUser> ProjectUsers { get; set; }
        public ICollection<ProjectWriter> ProjectWriters { get; set; }
        public ICollection<SchemaModel> SchemaModels { get; set; }
        public ICollection<ProjectAutomation> ProjectAutomations { get; set; }
    }
}
