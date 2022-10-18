USE [qa_search]
GO
/****** Object:  Table [crawler].[DomainGroups]    Script Date: 20.06.2019 16:58:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [crawler].[DomainGroups](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](450) NOT NULL,
	[IndexingConfig] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_crawler.DomainGroups] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
ALTER TABLE [crawler].[DomainGroups] ADD  CONSTRAINT [DF_crawler.DomainGroups_IndexingConfig]  DEFAULT (N'null') FOR [IndexingConfig]
GO
