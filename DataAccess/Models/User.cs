using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class User
    {
        public User()
        {
            Job = new HashSet<Job>();
            Project = new HashSet<Project>();
            ProjectSchema = new HashSet<ProjectSchema>();
            ProjectUser = new HashSet<ProjectUser>();
            Reader = new HashSet<Reader>();
            SchemaModel = new HashSet<SchemaModel>();
            Writer = new HashSet<Writer>();
        }

        public int UserId { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }

        public ICollection<Job> Job { get; set; }
        public ICollection<Project> Project { get; set; }
        public ICollection<ProjectSchema> ProjectSchema { get; set; }
        public ICollection<ProjectUser> ProjectUser { get; set; }
        public ICollection<Reader> Reader { get; set; }
        public ICollection<SchemaModel> SchemaModel { get; set; }
        public ICollection<Writer> Writer { get; set; }
    }
}
