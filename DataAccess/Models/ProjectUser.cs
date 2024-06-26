using System;

namespace DataAccess.Models
{
    public partial class ProjectUser
    {
        public int ProjectId { get; set; }
        public int UserId { get; set; }
        public long PermissionBit { get; set; }
        public DateTime CreatedOn { get; set; }

        public Project Project { get; set; }
    }
}
