using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class WriterType
    {
        public WriterType()
        {
            Writers = new HashSet<Writer>();
        }

        public int WriterTypeId { get; set; }
        public string WriterTypeName { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public ICollection<Writer> Writers { get; set; }
    }
}
