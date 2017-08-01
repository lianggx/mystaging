using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MyStaging.Common
{
    public class ExpressionModel
    {
        public Type Model { get; set; }
        public Expression Body { get; set; }
    }
}
