
-- You Should be Create Database First.


/*
Navicat PGSQL Data Transfer

Source Server         : 127.0.0.1
Source Server Version : 90602
Source Host           : 127.0.0.1:5432
Source Database       : mystaging
Source Schema         : public

Target Server Type    : PGSQL
Target Server Version : 90602
File Encoding         : 65001

Date: 2019-02-21 18:20:27
*/

-- ----------------------------
-- Table structure for user
-- ----------------------------
DROP TABLE IF EXISTS "public"."user";
CREATE TABLE "public"."user" (
"id" varchar COLLATE "default" NOT NULL,
"loginname" varchar(255) COLLATE "default",
"password" varchar(255) COLLATE "default",
"nickname" varchar(255) COLLATE "default",
"sex" bool,
"age" int4 NOT NULL,
"money" numeric(10,2) NOT NULL,
"createtime" timestamp(6) NOT NULL
)
WITH (OIDS=FALSE);

ALTER TABLE "public"."user" ADD PRIMARY KEY ("id");


CREATE TABLE "public"."article" (
"id" varchar COLLATE "default" NOT NULL,
"userid" varchar COLLATE "default" NOT NULL,
"title" varchar(255) COLLATE "default",
"content" jsonb,
"createtime" timestamp(6) NOT NULL,
CONSTRAINT "article_pkey" PRIMARY KEY ("id", "userid"),
CONSTRAINT "fk_userid" FOREIGN KEY ("userid") REFERENCES "public"."user" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION
)
WITH (OIDS=FALSE)
;

ALTER TABLE "public"."article" OWNER TO "postgres";


CREATE TABLE "public"."topic" (
"id" uuid NOT NULL,
"title" varchar(255) COLLATE "default",
"create_time" timestamp(6),
"update_time" timestamp(6),
"last_time" timestamp(6),
"user_id" uuid,
"name" varchar(255) COLLATE "default",
"age" int4,
"sex" bool,
"createtime" date,
"updatetime" time(6),
CONSTRAINT "topic_pkey" PRIMARY KEY ("id")
)
WITH (OIDS=FALSE)
;

ALTER TABLE "public"."topic" OWNER TO "postgres";


CREATE TABLE "public"."post" (
"id" uuid NOT NULL,
"title" varchar(255) COLLATE "default" NOT NULL,
"content" jsonb,
"state" "public"."et_data_state" NOT NULL,
"role" "public"."et_role"[] NOT NULL,
"text" json,
CONSTRAINT "post_pkey" PRIMARY KEY ("id")
)
WITH (OIDS=FALSE)
;

ALTER TABLE "public"."post" OWNER TO "postgres";