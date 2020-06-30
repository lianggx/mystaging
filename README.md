# 欢迎使用 MyStaging

 MyStaging 是一款基于 .NETCore 平台的 ORM 中间件，提供简单易用的接入工具，支持 DbFirst/CodeFirst，并支持多种数据库类型，和 EF 不同的是，对单个项目的多路上下文支持中引进了主从数据库概念，查询默认从库，也可以指定主库，删除/修改/新增操作默认走主库，地层还提供了对单个查询数据的分布式缓存操作，可以自由灵活配置，目前 MyStaging 还在持续完善中，欢迎加入 Star/Contributors/Fork。

```
////////////////////////////////////////////////////////
///                                                  ///
///                        | |      (_)              ///
///    _ __ ___  _   _ ___| |_ __ _ _ _ __   __ _    ///
///   | '_ ` _ \| | | / __| __/ _` | | '_ \ / _` |   ///
///   | | | | | | |_| \__ \ || (_| | | | | | (_| |   ///
///   |_| |_| |_|\__, |___/\__\__,_|_|_| |_|\__, |   ///
///               __/ |                      __/ |   ///
///              |___/                      |___/    ///
///                                                  ///
////////////////////////////////////////////////////////
```

# 在包管理控制台安装 MyStaging.Gen 到 dotnet tool 命令集
  MyStaging.Gen 是一个独立的数据库迁移组件，其本质上是一个控制台程序，你可以单独下载这个包到本地，也可以将他注册到 dotnet tool ，注册到 dotnet tool 后，你就可以在 visual studio 中使用命令进行数据库的迁移工作。
  
* 安装

```
dotnet tool install -g MyStaging.Gen
```

要使用 MyStaging.Gen 请跟进下面的参数说明，执行创建实体对象映射.

```
--help 查看帮助
-m [mode，db[DbFirst]/code[CodeFirst]，默认为 DbFirst
-t [dbtype[Mysql/PostgreSQL]，数据库提供程序]  required
-d [database，数据库连接字符串] required
-p [project，项目名称]  required
-o [output，实体对象输出路径]，默认为 {project}/Models
```
```
==============示例==============
  CodeFirst：
  mystaging.gen -m code -t PostgreSQL -p Pgsql -d "Host=127.0.0.1;Port=5432;Username=postgres;Password=postgres;Database=mystaging;"

  DbFirst：
  mystaging.gen -m db -t PostgreSQL -p Pgsql -d "Host=127.0.0.1;Port=5432;Username=postgres;Password=postgres;Database=mystaging;"
================================
```

# 如何选择数据库提供程序
 MyStaging 提供了多种数据库的支持，目前提供了 PostgreSQL/Mysql 的支持，后续将陆续开发更多提供程序，比如基于 PostgreSQL 进行开发的程序，那么可以选择引用包 MyStaing.PostgreSQL。
 
| 数据库 | 提供程序 |
|-----|-----|
| PostgreSQL | MyStaing.PostgreSQL  |
| Mysql | MyStaging.Mysql   |


更多示例，请访问 /examples
