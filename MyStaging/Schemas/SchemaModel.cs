using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyStaging.Schemas
{
    /// <summary>
    ///  表结构
    /// </summary>
    public class SchemaModel
    {
        /// <summary>
        ///  字段名称
        /// </summary>
        public string FieldName { get; set; }
        /// <summary>
        /// 字段类型
        /// </summary>
        public NpgsqlDbType DbType { get; set; }
        /// <summary>
        ///  字段长度
        /// </summary>
        public int Size { get; set; }
        /// <summary>
        /// 字段映射的类型
        /// </summary>
        public Type SpecificType { get; set; }
        /// <summary>
        ///  是否主键
        /// </summary>
        public bool Primarykey { get; set; }
    }
}
