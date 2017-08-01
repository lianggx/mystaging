using System;
using System.Collections.Generic;
using System.Text;

namespace MyStaging.Common
{
    /// <summary>
    ///  Set the action of the conditaion 
    /// </summary>
    public enum ConditionType
    {
        /// <summary>
        ///  =
        /// </summary>
        Equal,
        /// <summary>
        ///  !=
        /// </summary>
        NotEqual,
        /// <summary>
        ///  >
        /// </summary>
        Greater,
        /// <summary>
        ///  >=
        /// </summary>
        Gt,
        /// <summary>
        ///  &lt;
        /// </summary>
        Less,
        /// <summary>
        ///  &lt;=
        /// </summary>
        Lt,
        /// <summary>
        ///  IS NULL
        /// </summary>
        Is_Null,
        /// <summary>
        ///  IS NOT NULL
        /// </summary>
        Is_Not_Null
    }
}
