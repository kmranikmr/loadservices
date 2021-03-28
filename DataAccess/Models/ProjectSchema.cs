using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class ProjectSchema
    {
        public ProjectSchema()
        {
            Jobs = new HashSet<Job>();
            ProjectFiles = new HashSet<ProjectFile>();
            SchemaModels = new HashSet<SchemaModel>();
            ProjectAutomations = new HashSet<ProjectAutomation>();
            //   ModelMetadatas = new HashSet<ModelMetadata>();
        }

        public int SchemaId { get; set; }
        public string SchemaName { get; set; }
        public int ProjectId { get; set; }
        public string TypeConfig { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }
        //  public ICollection<ModelMetadata> ModelMetadatas { get; set; }
        public ICollection<ProjectAutomation> ProjectAutomations { get; set; }
        public Project Project { get; set; }
        public ICollection<Job> Jobs { get; set; }
        public ICollection<ProjectFile> ProjectFiles { get; set; }
        public ICollection<SchemaModel> SchemaModels { get; set; }
    }
}
