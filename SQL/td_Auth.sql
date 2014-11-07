USE [atum2_db_account]
GO

/****** Object:  Table [dbo].[td_Auth]    Script Date: 09.11.2013 12:28:27 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[td_Auth](
	[ClientID] [int] NOT NULL UNIQUE,
	[LastLoginIP] [varchar](50) NULL,
	[LastLoginTime] [datetime] NULL
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO


