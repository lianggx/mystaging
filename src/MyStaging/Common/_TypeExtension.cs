using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;

namespace MyStaging.Common
{
	/// <summary>
	///  扩展方法对象
	/// </summary>
	public static class _TypeExtension
	{
		/// <summary>
		/// 转换为数据库查询 in 查询
		/// </summary>
		/// <typeparam name="T">in 查询的字段类型</typeparam>
		/// <param name="sender"></param>
		/// <param name="_vals">in 查询的值列表</param>
		/// <returns></returns>
		public static bool In<T>(this T sender, params T[] _vals)
		{
			return true;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="sender"></param>
		/// <param name="_vals"></param>
		/// <returns></returns>
		public static bool In<T>(this T? sender, params T[] _vals) where T : struct
		{
			return true;
		}

		/// <summary>
		/// 转换为数据库查询 not in 查询
		/// </summary>
		/// <typeparam name="T">not in 查询的字段类型</typeparam>
		/// <param name="sender"></param>
		/// <param name="_vals">not in 查询的值列表</param>
		/// <returns></returns>
		public static bool NotIn<T>(this T sender, params T[] _vals)
		{
			return true;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="sender"></param>
		/// <param name="_vals"></param>
		/// <returns></returns>
		public static bool NotIn<T>(this T? sender, params T[] _vals) where T : struct
		{
			return true;
		}
		/// <summary>
		/// 转换为数据库查询 like 查询
		/// </summary>
		/// <typeparam name="T">like 查询的字段类型</typeparam>
		/// <param name="sender"></param>
		/// <param name="_vals">like 查询的值列表</param>
		/// <returns></returns>
		public static bool Like<T>(this T sender, T _val)
		{
			return true;
		}

		/// <summary>
		/// 转换为数据库查询 not like 查询
		/// </summary>
		/// <typeparam name="T">not like 查询的字段类型</typeparam>
		/// <param name="sender"></param>
		/// <param name="_vals">not like 查询的值列表</param>
		/// <returns></returns>
		public static bool NotLike<T>(this T sender, T vals)
		{
			return true;
		}

		/// <summary>
		///  将扩展对象输出为指定格式数据
		/// </summary>
		/// <typeparam name="TTarget">输出源</typeparam>
		/// <typeparam name="TResult">输出结果</typeparam>
		/// <param name="list">来源对象</param>
		/// <param name="selector">格式选择器</param>
		/// <returns></returns>
		public static List<TResult> ToBson<TTarget, TResult>(this IEnumerable<TTarget> list, Expression<Func<TTarget, TResult>> selector)
		{
			List<TResult> result = new List<TResult>();
			foreach (var item in list)
			{
				TResult tr = selector.Compile().Invoke(item);
				result.Add(tr);
			}

			return result;
		}

		/// <summary>
		///  将首字母转大写
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static string ToUpperPascal(this string text)
		{
			if (string.IsNullOrEmpty(text)) return text;

			string _first = text.Substring(0, 1).ToUpper();
			string _value = text.Substring(1);

			return $"{_first}{_value}";
		}

		/// <summary>
		///  将首字母转小写
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static string ToLowerPascal(this string text)
		{
			if (string.IsNullOrEmpty(text)) return text;

			string _first = text.Substring(0, 1).ToLower();
			string _value = text.Substring(1);

			return $"{_first}{_value}";
		}
	}
}
