
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
-- Table structure for article
-- ----------------------------
DROP TABLE IF EXISTS "public"."article";
CREATE TABLE "public"."article" (
"id" varchar COLLATE "default" NOT NULL,
"userid" varchar COLLATE "default",
"title" varchar(255) COLLATE "default",
"content" jsonb,
"createtime" timestamp(6) NOT NULL
)
WITH (OIDS=FALSE)

;

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
WITH (OIDS=FALSE)

;

-- ----------------------------
-- Alter Sequences Owned By 
-- ----------------------------

-- ----------------------------
-- Primary Key structure for table article
-- ----------------------------
ALTER TABLE "public"."article" ADD PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table user
-- ----------------------------
ALTER TABLE "public"."user" ADD PRIMARY KEY ("id");
