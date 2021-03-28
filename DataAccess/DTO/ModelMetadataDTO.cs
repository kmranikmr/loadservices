using AutoMapper;
using DataAccess.Models;

namespace DataAccess.DTO
{
    [AutoMap(typeof(ModelMetadata))]
    public class ModelMetadataDTO
    {
        public int ModelId { get; set; }

        public int ProjectId { get; set; }

        public int MetadataId { get; set; }

        public string ColumnName { get; set; }

        public string DataType { get; set; }
    }
}
