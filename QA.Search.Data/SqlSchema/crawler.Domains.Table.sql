USE [qa_search]
GO
/****** Object:  Table [crawler].[Domains]    Script Date: 20.06.2019 16:58:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [crawler].[Domains](
	[Origin] [nvarchar](450) NOT NULL,
	[DomainGroupId] [int] NOT NULL,
	[LastFastCrawlingUtc] [datetimeoffset](7) NULL,
	[LastDeepCrawlingUtc] [datetimeoffset](7) NULL,
 CONSTRAINT [PK_crawler.Domains] PRIMARY KEY CLUSTERED 
(
	[Origin] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [crawler].[Domains]  WITH CHECK ADD  CONSTRAINT [FK_crawler.Domains_DomainGroup] FOREIGN KEY([DomainGroupId])
REFERENCES [crawler].[DomainGroups] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [crawler].[Domains] CHECK CONSTRAINT [FK_crawler.Domains_DomainGroup]
GO
