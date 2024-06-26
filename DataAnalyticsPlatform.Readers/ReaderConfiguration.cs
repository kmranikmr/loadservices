using DataAnalyticsPlatform.Shared;
using DataAnalyticsPlatform.Shared.Interfaces;
using DataAnalyticsPlatform.Shared.Models;
using DataAnalyticsPlatform.Shared.Types;
using System;
using System.Collections.Generic;

namespace DataAnalyticsPlatform.Readers
{

    public class ReaderConfiguration : IConfiguration
    {
        public Type ModelType { get; set; }
        public TypeConfig TypeConfig { get; set; }
        public string ModelMapName { get; set; }
        public Type ModelMap { get; set; }
        public int ProjectId { get; set; }

        public string SourcePath { get; set; }
        public int SourcePathId { get; set; }

        public SourceType SourceType { get; set; }

        public string ConfigurationName { get; set; }//get => throw new NotImplementedException(); set => throw new NotImplementedException();

        public CustomConfiguration ConfigurationDetails { get; set; }
        public List<Type> Types { get; set; }
        public ReaderConfiguration()
        {
        }

        public ReaderConfiguration(Type modelType, Type modelMap, string sourcePath, SourceType type, int fileId = 1)
        {
            ModelType = modelType;
            ModelMap = modelMap;
            SourcePath = sourcePath;
            SourceType = type;
            SourcePathId = fileId;
        }

        public ReaderConfiguration(TypeConfig typeConfig, string sourcePath, SourceType type, int fileId)
        {
            TypeConfig = typeConfig;
            SourcePath = sourcePath;
            SourceType = type;
            SourcePathId = fileId;
        }
    }
}
