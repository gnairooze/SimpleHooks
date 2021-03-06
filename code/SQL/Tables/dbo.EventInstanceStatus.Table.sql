USE [SimpleHooks]
GO
/****** Object:  Table [dbo].[EventInstanceStatus]    Script Date: 2021-11-03 4:27:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EventInstanceStatus](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[CreateBy] [nvarchar](50) NOT NULL,
	[CreateDate] [datetime2](7) NOT NULL,
	[ModifyBy] [nvarchar](50) NOT NULL,
	[ModifyDate] [datetime2](7) NOT NULL,
	[Notes] [nvarchar](1000) NOT NULL,
 CONSTRAINT [PK_EventInstanceStatus] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_EventInstanceStatus]    Script Date: 2021-11-03 4:27:36 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_EventInstanceStatus] ON [dbo].[EventInstanceStatus]
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[EventInstanceStatus] ADD  CONSTRAINT [DF_EventInstanceStatus_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO
ALTER TABLE [dbo].[EventInstanceStatus] ADD  CONSTRAINT [DF_EventInstanceStatus_ModifyDate]  DEFAULT (getdate()) FOR [ModifyDate]
GO
ALTER TABLE [dbo].[EventInstanceStatus] ADD  CONSTRAINT [DF_EventInstanceStatus_Notes]  DEFAULT ('') FOR [Notes]
GO
