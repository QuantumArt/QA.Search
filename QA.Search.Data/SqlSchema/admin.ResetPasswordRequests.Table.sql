USE [qa_search]
GO
/****** Object:  Table [admin].[ResetPasswordRequests]    Script Date: 20.06.2019 16:58:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [admin].[ResetPasswordRequests](
	[Id] [uniqueidentifier] NOT NULL,
	[UserId] [int] NOT NULL,
	[Timestamp] [datetimeoffset](7) NOT NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_admin.ResetPasswordRequests] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Index [IX_admin.ResetPasswordRequests_Timestamp]    Script Date: 20.06.2019 16:58:55 ******/
CREATE NONCLUSTERED INDEX [IX_admin.ResetPasswordRequests_Timestamp] ON [admin].[ResetPasswordRequests]
(
	[Timestamp] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [admin].[ResetPasswordRequests] ADD  DEFAULT (newid()) FOR [Id]
GO
ALTER TABLE [admin].[ResetPasswordRequests]  WITH CHECK ADD  CONSTRAINT [FK_ResetPasswordRequests_Users] FOREIGN KEY([UserId])
REFERENCES [admin].[Users] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [admin].[ResetPasswordRequests] CHECK CONSTRAINT [FK_ResetPasswordRequests_Users]
GO
