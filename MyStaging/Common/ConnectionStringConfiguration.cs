using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace MyStaging.Common
{
    /// <summary>
    ///  数据库连接字符串管理对象
    /// </summary>
    public class ConnectionStringConfiguration
    {
        /// <summary>
        ///  获取序号
        /// </summary>
        public int Id { get; internal set; }
        /// <summary>
        ///  数据库连接字符串
        /// </summary>
        public string ConnectionString { get; set; }
        public DbConnection DbConnection { get; set; }
        /// <summary>
        ///  获取使用次数
        /// </summary>
        public long Used { get; internal set; }
        /// <summary>
        ///  获取错误重试次数
        /// </summary>
        public long Error { get; internal set; }
    }
}
