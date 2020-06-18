using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyStaging.Mapping
{
    /// <summary>
    ///  标识列主键
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, Inherited = true)]
    public class PrimaryKeyAttribute : Attribute
    {
    }
}
