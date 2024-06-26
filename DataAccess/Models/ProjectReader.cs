namespace DataAccess.Models
{
    public partial class ProjectReader
    {
        public int ProjectId { get; set; }
        public int ReaderId { get; set; }

        public Project Project { get; set; }
        public Reader Reader { get; set; }
    }
}
