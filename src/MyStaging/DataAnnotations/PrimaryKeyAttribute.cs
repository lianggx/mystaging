using System;

namespace MyStaging.DataAnnotations
{
    public class PrimaryKeyAttribute : Attribute
    {
        public bool AutoIncrement { get; set; }
    }
}
