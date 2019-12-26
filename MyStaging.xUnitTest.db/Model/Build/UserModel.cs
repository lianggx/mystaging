using System;
using System.Linq;
using MyStaging.xUnitTest.DAL;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MyStaging.Mapping;
using NpgsqlTypes;
using MyStaging.Helpers;

namespace MyStaging.xUnitTest.Model
{
	[EntityMapping(name: "user", Schema = "public")]
	public partial class UserModel
	{
		[PrimaryKey]
		public string Id { get; set; }

		public string Loginname { get; set; }

		public string Password { get; set; }

		public string Nickname { get; set; }

		public bool? Sex { get; set; }

		public int Age { get; set; }

		public decimal Money { get; set; }

		public DateTime Createtime { get; set; }

		public decimal Wealth { get; set; }

		[NonDbColumnMapping, JsonIgnore] public MyStaging.xUnitTest.DAL.User.UserUpdateBuilder UpdateBuilder { get { return new MyStaging.xUnitTest.DAL.User.UserUpdateBuilder(model =>{MyStaging.Helpers.MyStagingUtils.CopyProperty<UserModel>(this, model); PgSqlHelper.CacheManager?.RemoveItemCache<UserModel>(this.Id); }, this.Id); } }

		public UserModel Insert() { return MyStaging.xUnitTest.DAL.User.Insert(this); }

	}
}
