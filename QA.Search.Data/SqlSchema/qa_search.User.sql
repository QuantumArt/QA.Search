USE [qa_search]
GO
/****** Object:  User [qa_search]    Script Date: 20.06.2019 16:58:55 ******/
CREATE USER [qa_search] FOR LOGIN [qa_search] WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_owner] ADD MEMBER [qa_search]
GO
