using System;
using System.Collections.Generic;
using System.Text;

namespace DataAnalyticsPlatform.Shared
{
    public class ColumnName : System.Attribute
    {
        public string Name { get; set; }

        public ColumnName(string name)
        {
            Name = name;
        }
    }
    public class TableName : System.Attribute
    {
        public string Name { get; set; }

        public TableName(string name)
        {
            Name = name;
        }
    }
}
