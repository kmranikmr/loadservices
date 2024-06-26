using AutoMapper;
using DataAccess.Models;
using System;
using System.Collections.Generic;

namespace DataAccess.DTO
{
    [AutoMap(typeof(Project))]
    public class ProjectDTO
    {
        public int ProjectId { get; set; }

        public string ProjectName { get; set; }

        public string ProjectDescription { get; set; }

        public bool IsFavorite { get; set; }

        public DateTime CreatedOn { get; set; }

        public IEnumerable<ProjectStatusSummary> Summary { get; set; }

        public IEnumerable<ProjectConfigSummary> ConfigSummary { get; set; }

    }

    public class ProjectWriterDTO
    {
        public int WriterTypeId { get; set; }

        public int WriterId { get; set; }

        public string Path { get; set; }
    }

    [AutoMap(typeof(ProjectFile))]

    public class ProjectFileDTO
    {
        public int ProjectFileId { get; set; }

        public int SourceTypeId { get; set; }

        public string FileName { get; set; }

        public string FilePath { get; set; }

        public string SourceConfiguration { get; set; }

        public int? ReaderId { get; set; }
        public int ProjectId { get; set; }

        public int? SchemaId { get; set; }
        public int UserId { get; set; }

    }



    public class SetReaderIdDTO
    {
        public int ProjectFileId { get; set; }

        public int ReaderId { get; set; }
    }

    public class CreateProject
    {
        public string ProjectName { get; set; }

        public string ProjectDescription { get; set; }
    }

    public class ProjectStatusSummary
    {
        public string StatusName { get; set; }

        public int Count { get; set; }
    }

    public class ProjectConfigSummary
    {
        public string SchemaName { get; set; }

        public string ReaderType { get; set; }

        public string WriterType { get; set; }
    }
}
