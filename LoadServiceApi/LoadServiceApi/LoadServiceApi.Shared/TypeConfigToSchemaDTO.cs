using DataAccess.DTO;
using DataAnalyticsPlatform.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
namespace LoadServiceApi.Shared
{


    public class TypeConfigToSchemaDTO
    {
        public static string GetDataType(DataAnalyticsPlatform.Shared.DataType type)
        {
            if (type == DataAnalyticsPlatform.Shared.DataType.Boolean)
                return "bool";
            else if (type == DataAnalyticsPlatform.Shared.DataType.Char)
                return "char";
            else if (type == DataAnalyticsPlatform.Shared.DataType.DateTime)
                return "datetime";
            else if (type == DataAnalyticsPlatform.Shared.DataType.Double)
                return "double";
            else if (type == DataAnalyticsPlatform.Shared.DataType.FloatArray)
                return "floatarray";
            else if (type == DataAnalyticsPlatform.Shared.DataType.Int)
                return "int";
            else if (type == DataAnalyticsPlatform.Shared.DataType.IntArray)
                return "intarray";
            else if (type == DataAnalyticsPlatform.Shared.DataType.Long)
                return "long";
            else if (type == DataAnalyticsPlatform.Shared.DataType.Object)
                return "object";
            else if (type == DataAnalyticsPlatform.Shared.DataType.ObjectArray)
                return "objectarray";
            else if (type == DataAnalyticsPlatform.Shared.DataType.String)
                return "string";
            else if (type == DataAnalyticsPlatform.Shared.DataType.StringArray)
                return "stringarray";
            return "";
        }
        public static SchemaDTO Tranform(DataAnalyticsPlatform.Shared.TypeConfig typeConfig, int projectId, int schemaId, int userId = 0)
        {
            try
            {
                SchemaDTO s = new SchemaDTO();
                //  s.SchemaName = typeConfig;
                s.TypeConfig = JsonConvert.SerializeObject(typeConfig);
                s.ProjectId = projectId;
                s.SchemaName = typeConfig.SchemaName;
                bool haveModels = false;
                if (typeConfig.ModelInfoList.Count > 0)
                {
                    haveModels = true;
                    int index = 0;
                    s.SchemaModels = new SchemaModelDTO[typeConfig.ModelInfoList.Count];

                    foreach (var modelConfig in typeConfig.ModelInfoList)
                    {
                        s.SchemaModels[index] = new SchemaModelDTO();
                        var ModelConfigData = modelConfig;
                        ModelMetadataDTO mDto = new ModelMetadataDTO();
                        if (ModelConfigData.ModelFields.Count > 0)
                        {
                            s.SchemaModels[index].ModelMetadatas = new ModelMetadataDTO[ModelConfigData.ModelFields.Count];
                            int modelIndex = 0;
                            foreach (var modelFieldInfo in ModelConfigData.ModelFields)
                            {
                                s.SchemaModels[index].ModelMetadatas[modelIndex] = new ModelMetadataDTO();
                                s.SchemaModels[index].ModelMetadatas[modelIndex].ColumnName = modelFieldInfo.DisplayName == null ? modelFieldInfo.Name : modelFieldInfo.DisplayName;
                                s.SchemaModels[index].ModelMetadatas[modelIndex].DataType = GetDataType(modelFieldInfo.DataType);
                                s.SchemaModels[index].ModelMetadatas[modelIndex].ProjectId = projectId;
                                modelIndex++;
                            }
                        }

                        s.SchemaModels[index].ModelConfig = "";
                        s.SchemaModels[index].ModelConfig = JsonConvert.SerializeObject(modelConfig.ModelFields);
                        s.SchemaModels[index].ModelName = modelConfig.ModelName;
                        s.SchemaModels[index].ProjectId = projectId;

                        if (schemaId != 0)
                            s.SchemaModels[index].ModelId = modelConfig.ModelId;
                        index++;
                    }

                }
                else if (typeConfig.BaseClassFields.Count > 0)
                {
                    int index = 0;
                    s.ProjectId = projectId;
                    s.SchemaName = typeConfig.SchemaName;
                    s.TypeConfig = JsonConvert.SerializeObject(typeConfig);
                    typeConfig.ModelInfoList = new List<ModelInfo>();
                    typeConfig.ModelInfoList[0] = new ModelInfo();
                    int modelIndex = 0;
                    s.SchemaModels = new SchemaModelDTO[1];
                    s.SchemaModels[0] = new SchemaModelDTO();

                    s.SchemaModels[0].ModelMetadatas = new ModelMetadataDTO[typeConfig.BaseClassFields.Count];
                    foreach (var fieldinfo in typeConfig.BaseClassFields)
                    {

                        var Fieldinfo = fieldinfo;
                        ModelMetadataDTO mDto = new ModelMetadataDTO();

                        s.SchemaModels[0].ModelMetadatas[modelIndex] = new ModelMetadataDTO();
                        s.SchemaModels[0].ModelMetadatas[modelIndex].ColumnName = fieldinfo.DisplayName == null ? fieldinfo.Name : fieldinfo.DisplayName;
                        s.SchemaModels[0].ModelMetadatas[modelIndex].DataType = GetDataType(fieldinfo.DataType);
                        s.SchemaModels[0].ModelMetadatas[modelIndex].ProjectId = projectId;


                        modelIndex++;


                    }
                }

                return s;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
