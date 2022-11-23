﻿using MyStaging.Core;
using MyStaging.Metadata;
using Npgsql;
using Pgsql.Model;
using System;
using System.Text.Json;

namespace Pgsql
{
    public class PgsqlDbContext : DbContext
    {
        public PgsqlDbContext(StagingOptions options) : base(options, ProviderType.PostgreSQL)
        {
        }

        static PgsqlDbContext()
        {
            Type[] jsonTypes = { typeof(JsonElement) };
            NpgsqlNameTranslator translator = new();
            //var dataSourceBuilder = new NpgsqlDataSourceBuilder("Host=localhost;Username=test;Password=test");
            //dataSourceBuilder.MapEnum<et_data_state>("public.et_data_state");
            //dataSourceBuilder.UseNodaTime();
            //await using var dataSource = dataSourceBuilder.Build();
            NpgsqlConnection.GlobalTypeMapper.UseJsonNet(jsonTypes);
            NpgsqlConnection.GlobalTypeMapper.MapEnum<et_data_state>("public.et_data_state", translator);
            NpgsqlConnection.GlobalTypeMapper.MapEnum<et_role>("public.et_role", translator);
        }

        public DbSet<User> User { get; set; }
        public DbSet<Post> Post { get; set; }
        public DbSet<Article> Article { get; set; }
        public DbSet<Topic> Topic { get; set; }
        public DbSet<Udt3> Udt3 { get; set; }
    }
    public partial class NpgsqlNameTranslator : INpgsqlNameTranslator
    {
        public string TranslateMemberName(string clrName) => clrName;
        public string TranslateTypeName(string clrTypeName) => clrTypeName;
    }
}
