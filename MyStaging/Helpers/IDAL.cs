using System;
using System.Collections.Generic;
using System.Text;

namespace MyStaging.Helpers
{
    public interface IDAL
    {
        /// <summary>
        ///  datatable name
        /// </summary>
        string TableName { get; }
        /// <summary>
        ///  datatable to entitymodel
        /// </summary>
        object DbModel { get; }
    }
}
