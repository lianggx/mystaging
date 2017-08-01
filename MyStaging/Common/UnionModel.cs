using MyStaging.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyStaging.Common
{
    /// <summary>
    ///  Union query model
    /// </summary>
    public class UnionModel
    {
        /// <summary>
        ///  Union type
        /// </summary>
        public UnionType JoinType { get; set; }
        /// <summary>
        ///  Union table
        /// </summary>
        public IDAL Obj_Dal { get; set; }
        /// <summary>
        ///  Union table fields
        /// </summary>
        public List<string> Fields { get; set; } = new List<string>();
        /// <summary>
        ///  Union table in database name
        /// </summary>
        public string Table { get; set; }
        /// <summary>
        ///  alias
        /// </summary>
        public string Alias { get; set; }
        /// <summary>
        ///  on expression
        /// </summary>
        public string On { get; set; }
    }
}
