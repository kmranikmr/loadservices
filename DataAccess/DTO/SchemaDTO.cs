using AutoMapper;
using DataAccess.Models;
using System.Collections.Generic;

namespace DataAccess.DTO
{
    [AutoMap(typeof(ProjectSchema))]
    public class SchemaDTO
    {
        public int ProjectId { get; set; }

        public int SchemaId { get; set; }

        public string SchemaName { get; set; }

        public string TypeConfig { get; set; }

        public SchemaModelDTO[] SchemaModels { get; set; }

       // public ModelMetadataDTO[] ModelMetadatas { get; set; }
        public static SchemaDTO GetSampleObject(int projectId)
        {
            SchemaDTO s = new SchemaDTO();
            s.SchemaName = "Schema1";
            s.TypeConfig = "test json";
            s.ProjectId = projectId;

            s.SchemaModels = new SchemaModelDTO[] {
                new SchemaModelDTO()
                {
                    ProjectId = projectId,
                    ModelConfig = "test json",
                    ModelName = "Table1",
                    ModelMetadatas = new ModelMetadataDTO[]
                    {
                        new ModelMetadataDTO()
                        {
                            ColumnName = "col1",
                            ProjectId = projectId,
                            DataType = "int",                            
                        },
                        new ModelMetadataDTO()
                        {
                            ColumnName = "col2",
                            ProjectId = projectId,
                            DataType = "string",
                        }
                    }
                },
                new SchemaModelDTO()
                {
                    ProjectId = projectId,
                    ModelConfig = "test json",
                    ModelName = "Table2",
                    ModelMetadatas = new ModelMetadataDTO[]
                    {
                        new ModelMetadataDTO()
                        {
                            ColumnName = "col10",
                            ProjectId = projectId,
                            DataType = "int",
                        },
                        new ModelMetadataDTO()
                        {
                            ColumnName = "col21",
                            ProjectId = projectId,
                            DataType = "string",
                        }
                    }
                }
            };

            return s;

        }
    }
}
