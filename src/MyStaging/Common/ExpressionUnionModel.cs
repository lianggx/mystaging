using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MyStaging.Common
{
    /// <summary>
    ///  连接查询表达式模型
    /// </summary>
    public class ExpressionUnionModel
    {
        /// <summary>
        ///  连接别名
        /// </summary>
        public string AlisName { get; set; }
        /// <summary>
        ///  连接别名
        /// </summary>
        public string UnionAlisName { get; set; }
        /// <summary>
        ///  连接的实体对象模型类型
        /// </summary>
        public Type Model { get; set; }
        /// <summary>
        ///  主查询实体对象模型
        /// </summary>
        public Type MasterType { get; set; }
        /// <summary>
        ///  on 连接查询表达式
        /// </summary>
        public Expression Body { get; set; }
        /// <summary>
        ///  连接查询的类型
        /// </summary>
        public UnionType UnionType { get; set; }
    }
}
