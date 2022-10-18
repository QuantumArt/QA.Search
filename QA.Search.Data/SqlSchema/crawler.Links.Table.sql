USE [qa_search]
GO
/****** Object:  Table [crawler].[Links]    Script Date: 20.06.2019 16:58:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ARITHABORT ON
GO
CREATE TABLE [crawler].[Links](
	[Hash]  AS (lower(CONVERT([char](64),hashbytes('SHA2_256',[url]),(2)))) PERSISTED NOT NULL,
	[Url] [nvarchar](max) NOT NULL,
	[NextIndexingUtc] [datetimeoffset](7) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Version] [bigint] NOT NULL,
 CONSTRAINT [PK_crawler.Links] PRIMARY KEY CLUSTERED 
(
	[Hash] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_crawler.Links_NextIndexingUtc]    Script Date: 20.06.2019 16:58:55 ******/
CREATE NONCLUSTERED INDEX [IX_crawler.Links_NextIndexingUtc] ON [crawler].[Links]
(
	[NextIndexingUtc] ASC
)
INCLUDE ( 	[Url],
	[Version]) 
WHERE ([IsActive]=(1))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
