using Pgsql.Model;
using System;
using Npgsql;
using MyStaging.Core;
using MyStaging.Common;
using Newtonsoft.Json.Linq;

namespace Pgsql
{
    public class PgsqlDbContext : DbContext
    {
        public PgsqlDbContext(StagingOptions options) : base(options)
        {
            //    InitializerMapping();
        }

        static PgsqlDbContext()
        {
            InitializerMapping();
        }

        public static void InitializerMapping()
        {
            Type[] jsonTypes = { typeof(JToken), typeof(JObject), typeof(JArray) };
            NpgsqlNameTranslator translator = new NpgsqlNameTranslator();
            NpgsqlConnection.GlobalTypeMapper.UseJsonNet(jsonTypes);

            NpgsqlConnection.GlobalTypeMapper.MapEnum<Et_data_state>("public.et_data_state", translator);
            NpgsqlConnection.GlobalTypeMapper.MapEnum<Et_role>("public.et_role", translator);
        }

        public DbSet<PostModel> Post { get; set; }
        public DbSet<ArticleModel> Article { get; set; }
        public DbSet<TopicModel> Topic { get; set; }
        public DbSet<UserModel> User { get; set; }
    }
    public partial class NpgsqlNameTranslator : INpgsqlNameTranslator
    {
        public string TranslateMemberName(string clrName) => clrName;
        public string TranslateTypeName(string clrTypeName) => clrTypeName;
    }
}
