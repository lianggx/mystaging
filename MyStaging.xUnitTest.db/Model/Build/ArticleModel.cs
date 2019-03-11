using System;
using System.Linq;
using MyStaging.xUnitTest.DAL;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MyStaging.Mapping;
using NpgsqlTypes;

namespace MyStaging.xUnitTest.Model
{
	[EntityMapping(name: "article", Schema = "public")]
	public partial class ArticleModel
	{
		public string Id { get; set; }

		public string Userid { get; set; }

		public string Title { get; set; }

		public JToken Content { get; set; }

		public DateTime Createtime { get; set; }

		private UserModel user = null;
		[ForeignKeyMapping(name: "userid"), JsonIgnore] public UserModel User { get { if (user == null) user = MyStaging.xUnitTest.DAL.User.Context.Where(f => f.Id == this.Userid).ToOne(); return user; } }

		[NonDbColumnMapping, JsonIgnore] public MyStaging.xUnitTest.DAL.Article.ArticleUpdateBuilder UpdateBuilder { get { return new MyStaging.xUnitTest.DAL.Article.ArticleUpdateBuilder(model =>{MyStaging.Helpers.MyStagingUtils.CopyProperty<ArticleModel>(this, model);}, this.Id,this.Userid); } }

		public ArticleModel Insert() { return MyStaging.xUnitTest.DAL.Article.Insert(this); }

	}
}
