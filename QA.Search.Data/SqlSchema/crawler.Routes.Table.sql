USE [qa_search]
GO
/****** Object:  Table [crawler].[Routes]    Script Date: 20.06.2019 16:58:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [crawler].[Routes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DomainGroupId] [int] NOT NULL,
	[Route] [nvarchar](max) NOT NULL,
	[ScanPeriodMsec] [int] NOT NULL,
	[IndexingConfig] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_crawler.Routes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Index [IX_crawler.Routes_DomainGroupId]    Script Date: 20.06.2019 16:58:55 ******/
CREATE NONCLUSTERED INDEX [IX_crawler.Routes_DomainGroupId] ON [crawler].[Routes]
(
	[DomainGroupId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [crawler].[Routes] ADD  CONSTRAINT [DF_crawler.Routes_IndexingConfig]  DEFAULT (N'null') FOR [IndexingConfig]
GO
ALTER TABLE [crawler].[Routes]  WITH CHECK ADD  CONSTRAINT [FK_Routes_DomainGroup] FOREIGN KEY([DomainGroupId])
REFERENCES [crawler].[DomainGroups] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [crawler].[Routes] CHECK CONSTRAINT [FK_Routes_DomainGroup]
GO
