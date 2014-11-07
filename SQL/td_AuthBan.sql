USE [atum2_db_account]
GO

/****** Object:  Table [dbo].[td_AuthBan]    Script Date: 09.11.2013 12:29:09 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[td_AuthBan](
	[ClientID] [varchar](50) NULL UNIQUE,
	[Banned] [int] NULL,
	[LastLoginIP] [varchar](50) NULL,
	[BanDate] [datetime] NULL
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO


