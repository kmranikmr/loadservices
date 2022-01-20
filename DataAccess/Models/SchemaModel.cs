using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class SchemaModel
    {
        public SchemaModel()
        {
            ModelMetadatas = new HashSet<ModelMetadata>();
        }

        public int ModelId { get; set; }
        public int SchemaId { get; set; }
        public int ProjectId { get; set; }
        public string ModelName { get; set; }
        public string ModelConfig { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long? ModelSize { get; set; }
        public Project Project { get; set; }
        public ProjectSchema Schema { get; set; }
        public ICollection<ModelMetadata> ModelMetadatas { get; set; }
    }
}
