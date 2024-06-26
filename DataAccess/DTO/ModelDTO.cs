using AutoMapper;
using DataAccess.Models;

namespace DataAccess.DTO
{
    [AutoMap(typeof(SchemaModel))]
    public class SchemaModelDTO
    {
        public int ModelId { get; set; }

        public int ProjectId { get; set; }

        public string ModelName { get; set; }

        public string ModelConfig { get; set; }

        public ModelMetadataDTO[] ModelMetadatas { get; set; }
    }
}
