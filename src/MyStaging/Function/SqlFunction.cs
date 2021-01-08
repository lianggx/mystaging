using System;

namespace MyStaging.Function
{
    public static class SqlFunction
    {
        /// <summary>
        /// 转换为数据库查询 in 查询
        /// </summary>
        /// <typeparam name="T">in 查询的字段类型</typeparam>
        /// <param name="sender"></param>
        /// <param name="vals">in 查询的值列表</param>
        /// <returns></returns>
        public static bool In<T>(this T sender, params T[] vals)
        {
            if (vals is null)
            {
                throw new ArgumentNullException(nameof(vals));
            }

            return true;
        }

        public static bool In<T>(this T? sender, params T[] _vals) where T : struct
        {
            if (sender is null)
            {
                throw new ArgumentNullException(nameof(sender));
            }

            if (_vals is null)
            {
                throw new ArgumentNullException(nameof(_vals));
            }

            return true;
        }

        /// <summary>
        /// 转换为数据库查询 not in 查询
        /// </summary>
        /// <typeparam name="T">not in 查询的字段类型</typeparam>
        /// <param name="sender"></param>
        /// <param name="vals">not in 查询的值列表</param>
        /// <returns></returns>
        public static bool NotIn<T>(this T sender, params T[] vals)
        {
            return true;
        }

        public static bool NotIn<T>(this T? sender, params T[] vals) where T : struct
        {
            return true;
        }

        /// <summary>
        /// 转换为数据库查询 like 查询
        /// </summary>
        /// <typeparam name="T">like 查询的字段类型</typeparam>
        /// <returns></returns>
        public static bool Like<T>(this T sender, T val)
        {
            return true;
        }

        /// <summary>
        /// 转换为数据库查询 not like 查询
        /// </summary>
        /// <typeparam name="T">not like 查询的字段类型</typeparam>
        /// <param name="sender"></param>
        /// <param name="vals">not like 查询的值列表</param>
        /// <returns></returns>
        public static bool NotLike<T>(this T sender, T vals)
        {
            return true;
        }
    }
}
