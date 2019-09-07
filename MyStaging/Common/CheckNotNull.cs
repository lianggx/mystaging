using System;
using System.Collections.Generic;
using System.Text;

namespace MyStaging.Common
{
    public class CheckNotNull
    {
        public static T NotNull<T>(T model, string parameterName)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            return model;
        }

        public static IReadOnlyList<T> NotEmpty<T>(IReadOnlyList<T> value, string parameterName)
        {
            NotNull(value, parameterName);

            if (value.Count == 0)
            {
                NotEmpty(parameterName, nameof(parameterName));

                throw new ArgumentException(parameterName);
            }

            return value;
        }

        public static string NotEmpty(string value, string parameterName)
        {
            Exception e = null;
            if (value is null)
            {
                e = new ArgumentNullException(parameterName);
            }
            else if (value.Trim().Length == 0)
            {
                e = new ArgumentException(parameterName);
            }

            if (e != null)
            {
                NotEmpty(parameterName, nameof(parameterName));

                throw e;
            }

            return value;
        }
    }
}
