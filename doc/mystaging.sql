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
  `UserId` int(255) DEFAULT NULL COMMENT '�û����',
  `Resource` varchar(255) CHARACTER SET utf8 DEFAULT NULL COMMENT '��Դ����',
  `ResourceId` int(255) DEFAULT NULL COMMENT '��Դ���',
  `ReqContent` text CHARACTER SET utf8 COMMENT '��������',
  `ResContent` text CHARACTER SET utf8 COMMENT '��Ӧ����',
  `IP` varchar(255) CHARACTER SET utf8 DEFAULT NULL COMMENT '�ͻ���IP��ַ',
  `Code` int(11) DEFAULT NULL COMMENT '��Ӧ����',
  `Remark` varchar(255) CHARACTER SET utf8 DEFAULT NULL COMMENT '��ע',
  `CreateTime` datetime NOT NULL COMMENT '����ʱ��',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Table structure for M_Mapping
-- ----------------------------
DROP TABLE IF EXISTS `M_Mapping`;
CREATE TABLE `M_Mapping` (
  `UserId` int(11) NOT NULL COMMENT '�û����',
  `RoleId` int(11) NOT NULL COMMENT '��ɫ���',
  `CreateTime` datetime NOT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT '����ʱ��',
  PRIMARY KEY (`UserId`,`RoleId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- ----------------------------
-- Table structure for M_Resource
-- ----------------------------
DROP TABLE IF EXISTS `M_Resource`;
CREATE TABLE `M_Resource` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `ParentId` int(11) DEFAULT NULL COMMENT '�ϼ����',
  `Name` varchar(255) CHARACTER SET utf8 DEFAULT NULL COMMENT '��Դ����',
  `Content` varchar(255) CHARACTER SET utf8 DEFAULT NULL COMMENT '��Դ����',
  `Type` int(11) NOT NULL COMMENT '��Դ���ͣ�0=API��1=��ҳԪ��',
  `State` int(11) NOT NULL COMMENT '״̬��0=������1=���ᣬ2=ɾ��',
  `Authorize` tinyint(1) NOT NULL COMMENT '�Ƿ���Ҫ��Ȩ����',
  `Sort` int(11) NOT NULL COMMENT '����ţ�������˳������',
  `CreateTime` datetime NOT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT '����ʱ��',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- ----------------------------
-- Table structure for M_Role
-- ----------------------------
DROP TABLE IF EXISTS `M_Role`;
CREATE TABLE `M_Role` (
  `Id` int(11) NOT NULL AUTO_INCREMENT COMMENT '���',
  `Name` varchar(255) CHARACTER SET utf8 NOT NULL COMMENT '����',
  `State` int(11) NOT NULL COMMENT '״̬��0=������1=���ᣬ2=ɾ��',
  `CreateTime` datetime NOT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT '����ʱ��',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- ----------------------------
-- Table structure for M_Roleresource
-- ----------------------------
DROP TABLE IF EXISTS `M_Roleresource`;
CREATE TABLE `M_Roleresource` (
  `RoleId` int(11) NOT NULL COMMENT '��ɫ���',
  `ResourceId` int(11) NOT NULL COMMENT '��Դ���',
  PRIMARY KEY (`RoleId`,`ResourceId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;

-- ----------------------------
-- Table structure for M_User
-- ----------------------------
DROP TABLE IF EXISTS `M_User`;
CREATE TABLE `M_User` (
  `Id` int(11) NOT NULL AUTO_INCREMENT COMMENT '���',
  `ImgFace` varchar(700) CHARACTER SET utf8 DEFAULT NULL COMMENT 'ͷ��',
  `Name` varchar(255) CHARACTER SET utf8 NOT NULL COMMENT '����',
  `Phone` varchar(11) CHARACTER SET utf8 DEFAULT NULL COMMENT '�ֻ�����',
  `LoginName` varchar(255) CHARACTER SET utf8 NOT NULL COMMENT '��¼��',
  `Password` varchar(255) CHARACTER SET utf8 NOT NULL COMMENT '��¼����',
  `State` int(11) NOT NULL COMMENT '״̬��0=������1=δ���2=���ᣬ3=ɾ��',
  `CreateTime` datetime NOT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT '����ʱ��',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;
