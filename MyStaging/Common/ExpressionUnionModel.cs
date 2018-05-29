using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MyStaging.Common
{
    public class ExpressionUnionModel
    {
        public string AlisName { get; set; }
        public Type Model { get; set; }
        public Type MasterType { get; set; }
        public Expression Body { get; set; }
        public UnionType UnionType { get; set; }
    }
}
