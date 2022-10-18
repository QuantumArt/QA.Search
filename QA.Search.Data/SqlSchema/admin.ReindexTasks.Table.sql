USE [qa_search]
GO
/****** Object:  Table [admin].[ReindexTasks]    Script Date: 20.06.2019 16:58:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [admin].[ReindexTasks](
	[SourceIndex] [nvarchar](225) NOT NULL,
	[DestinationIndex] [nvarchar](225) NOT NULL,
	[ElasticTaskId] [nvarchar](250) NULL,
	[Status] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[Finished] [datetime] NULL,
	[Timestamp] [timestamp] NOT NULL,
	[LastUpdated] [datetime] NULL,
	[ShortIndexName] [nvarchar](250) NOT NULL,
 CONSTRAINT [PK_admin.ReindexTasks] PRIMARY KEY CLUSTERED 
(
	[SourceIndex] ASC,
	[DestinationIndex] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_admin.ReindexTasks_ActiveTasks]    Script Date: 20.06.2019 16:58:55 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_admin.ReindexTasks_ActiveTasks] ON [admin].[ReindexTasks]
(
	[SourceIndex] ASC
)
WHERE ([Status]<(3))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_admin.ReindexTasks_ShortIndexName]    Script Date: 20.06.2019 16:58:55 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_admin.ReindexTasks_ShortIndexName] ON [admin].[ReindexTasks]
(
	[ShortIndexName] ASC,
	[Timestamp] ASC
)
INCLUDE ( 	[Status]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
