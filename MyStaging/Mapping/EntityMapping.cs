using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace MyStaging.Mapping
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, Inherited = true)]
    public class EntityMappingAttribute : Attribute
    {
        public string TableName { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, Inherited = true)]
    public class PropertyMappingAttribute : Attribute
    {
        public string FieldName { get; set; }
        public NpgsqlDbType DbType { get; set; }
        public int Length { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class ForeignKeyMappingAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class NonDbColumnMappingAttribute : Attribute
    {
    }
}
