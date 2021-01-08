/*
Navicat MySQL Data Transfer

Source Server         : 127.0.0.1-mysql
Source Server Version : 50711
Source Host           : 127.0.0.1:3306
Source Database       : mystaging

Target Server Type    : MYSQL
Target Server Version : 50711
File Encoding         : 65001

Date: 2020-11-24 16:53:59
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for Article
-- ----------------------------
DROP TABLE IF EXISTS `Article`;
CREATE TABLE `Article` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `State` tinyint(1) NOT NULL,
  `UserId` int(11) NOT NULL,
  `Title` varchar(255) DEFAULT NULL,
  `Content` varchar(255) DEFAULT NULL,
  `CreateTime` datetime NOT NULL,
  `IP` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Table structure for Customer
-- ----------------------------
DROP TABLE IF EXISTS `Customer`;
CREATE TABLE `Customer` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Table structure for M_Accesslog
-- ----------------------------
DROP TABLE IF EXISTS `M_Accesslog`;
CREATE TABLE `M_Accesslog` (
  `Id` int(255) NOT NULL AUTO_INCREMENT,
  `UserId` int(255) DEFAULT NULL COMMENT '用户编号',
  `Resource` varchar(255) CHARACTER SET utf8 DEFAULT NULL COMMENT '资源内容',
  `ResourceId` int(255) DEFAULT NULL COMMENT '资源编号',
  `ReqContent` text CHARACTER SET utf8 COMMENT '请求内容',
  `ResContent` text CHARACTER SET utf8 COMMENT '响应内容',
  `IP` varchar(255) CHARACTER SET utf8 DEFAULT NULL COMMENT '客户端IP地址',
  `Code` int(11) DEFAULT NULL COMMENT '响应代码',
  `Remark` varchar(255) CHARACTER SET utf8 DEFAULT NULL COMMENT '备注',
  `CreateTime` datetime NOT NULL COMMENT '创建时间',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Table structure for M_Mapping
-- ----------------------------
DROP TABLE IF EXISTS `M_Mapping`;
CREATE TABLE `M_Mapping` (
  `UserId` int(11) NOT NULL COMMENT '用户编号',
  `RoleId` int(11) NOT NULL COMMENT '角色编号',
  `CreateTime` datetime NOT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`UserId`,`RoleId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- ----------------------------
-- Table structure for M_Resource
-- ----------------------------
DROP TABLE IF EXISTS `M_Resource`;
CREATE TABLE `M_Resource` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `ParentId` int(11) DEFAULT NULL COMMENT '上级编号',
  `Name` varchar(255) CHARACTER SET utf8 DEFAULT NULL COMMENT '资源名称',
  `Content` varchar(255) CHARACTER SET utf8 DEFAULT NULL COMMENT '资源内容',
  `Type` int(11) NOT NULL COMMENT '资源类型，0=API，1=网页元素',
  `State` int(11) NOT NULL COMMENT '状态，0=正常，1=冻结，2=删除',
  `Authorize` tinyint(1) NOT NULL COMMENT '是否需要授权访问',
  `Sort` int(11) NOT NULL COMMENT '排序号，按数字顺序排序',
  `CreateTime` datetime NOT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- ----------------------------
-- Table structure for M_Role
-- ----------------------------
DROP TABLE IF EXISTS `M_Role`;
CREATE TABLE `M_Role` (
  `Id` int(11) NOT NULL AUTO_INCREMENT COMMENT '编号',
  `Name` varchar(255) CHARACTER SET utf8 NOT NULL COMMENT '名称',
  `State` int(11) NOT NULL COMMENT '状态，0=正常，1=冻结，2=删除',
  `CreateTime` datetime NOT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- ----------------------------
-- Table structure for M_Roleresource
-- ----------------------------
DROP TABLE IF EXISTS `M_Roleresource`;
CREATE TABLE `M_Roleresource` (
  `RoleId` int(11) NOT NULL COMMENT '角色编号',
  `ResourceId` int(11) NOT NULL COMMENT '资源编号',
  PRIMARY KEY (`RoleId`,`ResourceId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- ----------------------------
-- Table structure for M_User
-- ----------------------------
DROP TABLE IF EXISTS `M_User`;
CREATE TABLE `M_User` (
  `Id` int(11) NOT NULL AUTO_INCREMENT COMMENT '编号',
  `ImgFace` varchar(700) CHARACTER SET utf8 DEFAULT NULL COMMENT '头像',
  `Name` varchar(255) CHARACTER SET utf8 NOT NULL COMMENT '姓名',
  `Phone` varchar(11) CHARACTER SET utf8 DEFAULT NULL COMMENT '手机号码',
  `LoginName` varchar(255) CHARACTER SET utf8 NOT NULL COMMENT '登录名',
  `Password` varchar(255) CHARACTER SET utf8 NOT NULL COMMENT '登录密码',
  `State` int(11) NOT NULL COMMENT '状态，0=正常，1=未激活，2=冻结，3=删除',
  `CreateTime` datetime NOT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;
