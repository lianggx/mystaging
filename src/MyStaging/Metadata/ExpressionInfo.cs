using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MyStaging.Metadata
{
    /// <summary>
    ///  lambda 表达式模型对象
    /// </summary>
    public class ExpressionInfo
    {
        /// <summary>
        ///  连接表别名
        /// </summary>
        public string UnionAlisName { get; set; }
        /// <summary>
        ///  实体对象模型类型
        /// </summary>
        public Type Model { get; set; }
        /// <summary>
        ///  查询表达式
        /// </summary>
        public Expression Body { get; set; }
    }
}
