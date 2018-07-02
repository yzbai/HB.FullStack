CREATE TABLE tb_role (
`Id` bigint(20) NOT NULL AUTO_INCREMENT,
`Deleted` bit(1) NOT NULL DEFAULT b'0',
`LastUser` varchar(100) DEFAULT NULL,
`LastTime` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
`Version` bigint(20) NOT NULL DEFAULT '0',
  `Name` VARCHAR(100)   NOT NULL    UNIQUE ,
 `DisplayName` VARCHAR(500)    ,
 `IsActivated` BOOL    ,
 `Comment` VARCHAR(1024)    ,
  PRIMARY KEY (`Id`) 
 ) ENGINE=InnoDB   DEFAULT CHARSET=utf8;

CREATE TABLE tb_user (
`Id` bigint(20) NOT NULL AUTO_INCREMENT,
`Deleted` bit(1) NOT NULL DEFAULT b'0',
`LastUser` varchar(100) DEFAULT NULL,
`LastTime` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
`Version` bigint(20) NOT NULL DEFAULT '0',
  `Guid` VARCHAR(50)   NOT NULL   ,
 `SecurityStamp` VARCHAR(50)   NOT NULL   ,
 `UserName` VARCHAR(100)    ,
 `Mobile` VARCHAR(100)    ,
 `Email` VARCHAR(100)    ,
 `PasswordHash` VARCHAR(100)    ,
 `IsActivated` BOOL    ,
 `MobileConfirmed` BOOL    ,
 `EmailConfirmed` BOOL    ,
 `TwoFactorEnabled` BOOL    ,
 `LockoutEnabled` BOOL    ,
 `LockoutEndDate` DATETIME    ,
 `AccessFailedCount` BIGINT    ,
 `AccessFailedLastTime` DATETIME    ,
  PRIMARY KEY (`Id`) 
 ) ENGINE=InnoDB   DEFAULT CHARSET=utf8;

CREATE TABLE tb_userclaim (
`Id` bigint(20) NOT NULL AUTO_INCREMENT,
`Deleted` bit(1) NOT NULL DEFAULT b'0',
`LastUser` varchar(100) DEFAULT NULL,
`LastTime` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
`Version` bigint(20) NOT NULL DEFAULT '0',
  `UserId` BIGINT   NOT NULL   ,
 `ClaimType` TEXT(65530)    ,
 `ClaimValue` TEXT(65530)    ,
  PRIMARY KEY (`Id`) 
 ) ENGINE=InnoDB   DEFAULT CHARSET=utf8;

CREATE TABLE tb_userrole (
`Id` bigint(20) NOT NULL AUTO_INCREMENT,
`Deleted` bit(1) NOT NULL DEFAULT b'0',
`LastUser` varchar(100) DEFAULT NULL,
`LastTime` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
`Version` bigint(20) NOT NULL DEFAULT '0',
  `UserId` BIGINT   NOT NULL   ,
 `RoleId` BIGINT   NOT NULL   ,
  PRIMARY KEY (`Id`) 
 ) ENGINE=InnoDB   DEFAULT CHARSET=utf8;

CREATE TABLE tb_signintoken (
`Id` bigint(20) NOT NULL AUTO_INCREMENT,
`Deleted` bit(1) NOT NULL DEFAULT b'0',
`LastUser` varchar(100) DEFAULT NULL,
`LastTime` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
`Version` bigint(20) NOT NULL DEFAULT '0',
  `SignInTokenIdentifier` VARCHAR(100)    ,
 `UserId` BIGINT   NOT NULL   ,
 `RefreshToken` VARCHAR(100)    ,
 `ExpireAt` DATETIME    ,
 `Blacked` BOOL    ,
 `ClientId` VARCHAR(100)    ,
 `ClientType` VARCHAR(100)    ,
 `ClientVersion` VARCHAR(100)    ,
 `ClientAddress` VARCHAR(100)    ,
 `ClientIp` VARCHAR(100)    ,
  PRIMARY KEY (`Id`) 
 ) ENGINE=InnoDB   DEFAULT CHARSET=utf8;

CREATE TABLE tb_thirdpartylogin (
`Id` bigint(20) NOT NULL AUTO_INCREMENT,
`Deleted` bit(1) NOT NULL DEFAULT b'0',
`LastUser` varchar(100) DEFAULT NULL,
`LastTime` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
`Version` bigint(20) NOT NULL DEFAULT '0',
  `UserId` BIGINT   NOT NULL   ,
 `LoginProvider` VARCHAR(500)    ,
 `ProviderKey` VARCHAR(500)    ,
 `ProviderDisplayName` VARCHAR(500)    ,
 `SnsName` VARCHAR(100)    ,
 `SnsId` VARCHAR(100)    ,
 `AccessToken` VARCHAR(100)    ,
 `IconUrl` VARCHAR(1024)    ,
  PRIMARY KEY (`Id`) 
 ) ENGINE=InnoDB   DEFAULT CHARSET=utf8;

