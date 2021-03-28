using AutoMapper;
using DataAccess.Models;

namespace DataAccess.DTO
{
    [AutoMap(typeof(Reader))]
    public class ReaderDTO
    {
        public int ReaderId { get; set; }
        public int ReaderTypeId { get; set; }
        public string ReaderConfiguration { get; set; }
        public string ConfigurationName { get; set; }
    }

    [AutoMap(typeof(ReaderType))]
    public class ReaderTypeDTO
    {
        public int ReaderTypeId { get; set; }
        public string ReaderTypeName { get; set; }
    }

    [AutoMap(typeof(Writer))]
    public class WriterDTO
    {
        public int WriterId { get; set; }

        public WriterTypeDTO WriterType { get; set; }
    }

    [AutoMap(typeof(WriterType))]
    public class WriterTypeDTO
    {
        public int WriterTypeId { get; set; }
        public string WriterTypeName { get; set; }
    }

}
