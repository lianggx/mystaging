using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MyStaging.Mapping
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, Inherited = true)]
    public class EntityMappingAttribute : TableAttribute
    {
        public EntityMappingAttribute(string name) : base(name) {
           
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, Inherited = true)]
    public class PropertyMappingAttribute : ColumnAttribute
    {
        public string FieldName { get; set; }
        public NpgsqlDbType DbType { get; set; }
        public int Length { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class ForeignKeyMappingAttribute : ForeignKeyAttribute
    {
        public ForeignKeyMappingAttribute(string name) : base(name) { }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class NonDbColumnMappingAttribute : NotMappedAttribute
    {
    }
}
