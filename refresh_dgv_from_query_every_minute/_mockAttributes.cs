using System;

namespace refresh_dgv_from_query_every_minute
{
    internal class PrimaryKeyAttribute : Attribute { }
    internal class TableAttribute : Attribute 
    {
        public TableAttribute(string table) => Table = table;
        public string Table { get; }
    }
}