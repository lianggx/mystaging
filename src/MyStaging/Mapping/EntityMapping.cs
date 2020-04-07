using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MyStaging.Mapping
{
    /// <summary>
    ///  数据关系映射对象
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, Inherited = true)]
    public class EntityMappingAttribute : TableAttribute
    {
        public EntityMappingAttribute(string name) : base(name)
        {
        }
    }

    /// <summary>
    ///  属性关系映射对象
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, Inherited = true)]
    public class PropertyMappingAttribute : ColumnAttribute
    {
        public PropertyMappingAttribute(string name) : base(name)
        {
        }

        /// <summary>
        ///  获取或者设置字段类型
        /// </summary>
        public NpgsqlDbType DbType { get; set; }

        /// <summary>
        ///  获取或者设置字段长度
        /// </summary>
        public int Length { get; set; }
    }

    /// <summary>
    ///  标识列主键
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, Inherited = true)]
    public class PrimaryKeyAttribute : ColumnAttribute
    {
    }

    /// <summary>
    ///  外键关系映射对象
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class ForeignKeyMappingAttribute : ForeignKeyAttribute
    {
        public ForeignKeyMappingAttribute(string name) : base(name) { }
    }

    /// <summary>
    ///  无关系映射对象
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class NonDbColumnMappingAttribute : NotMappedAttribute
    {
    }
}
