using System;
using System.Collections.Generic;
using System.Text;

namespace LoadServiceApi.Shared.Models
{
    //public interface IModelMap
    //{
    //    void Map();
    //    List<BaseModel> GetModels();
    //}
    //public class BaseModel
    //{

    //}

    public class BaseModel
    {
        public string ModelName { get; set; }
        public int ModelId { get; set; }
        public int UserId { get; set; }
        public int SchemaId { get; set; }
    }
}
