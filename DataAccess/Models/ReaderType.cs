using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class ReaderType
    {
        public ReaderType()
        {
            Readers = new HashSet<Reader>();
        }

        public int ReaderTypeId { get; set; }
        public string ReaderTypeName { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public ICollection<Reader> Readers { get; set; }
    }
}
