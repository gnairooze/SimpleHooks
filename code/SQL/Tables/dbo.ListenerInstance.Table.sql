USE [SimpleHooks]
GO
/****** Object:  Table [dbo].[ListenerInstance]    Script Date: 2021-11-03 4:27:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ListenerInstance](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[EventInstance_Id] [bigint] NOT NULL,
	[ListenerDefinition_Id] [bigint] NOT NULL,
	[ListenerInstanceStatus_Id] [int] NOT NULL,
	[RemainingTrialCount] [int] NOT NULL,
	[NextRun] [datetime2](7) NOT NULL,
	[Active] [bit] NOT NULL,
	[CreateBy] [nvarchar](50) NOT NULL,
	[CreateDate] [datetime2](7) NOT NULL,
	[ModifyBy] [nvarchar](50) NOT NULL,
	[ModifyDate] [datetime2](7) NOT NULL,
	[Notes] [nvarchar](1000) NOT NULL,
	[TimeStamp] [timestamp] NOT NULL,
 CONSTRAINT [PK_ListenerInstance] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Index [IX_ListenerInstance]    Script Date: 2021-11-03 4:27:36 PM ******/
CREATE NONCLUSTERED INDEX [IX_ListenerInstance] ON [dbo].[ListenerInstance]
(
	[Active] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ListenerInstance_1]    Script Date: 2021-11-03 4:27:36 PM ******/
CREATE NONCLUSTERED INDEX [IX_ListenerInstance_1] ON [dbo].[ListenerInstance]
(
	[EventInstance_Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ListenerInstance_2]    Script Date: 2021-11-03 4:27:36 PM ******/
CREATE NONCLUSTERED INDEX [IX_ListenerInstance_2] ON [dbo].[ListenerInstance]
(
	[ListenerDefinition_Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ListenerInstance_3]    Script Date: 2021-11-03 4:27:36 PM ******/
CREATE NONCLUSTERED INDEX [IX_ListenerInstance_3] ON [dbo].[ListenerInstance]
(
	[ListenerInstanceStatus_Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ListenerInstance_4]    Script Date: 2021-11-03 4:27:36 PM ******/
CREATE NONCLUSTERED INDEX [IX_ListenerInstance_4] ON [dbo].[ListenerInstance]
(
	[NextRun] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ListenerInstance_5]    Script Date: 2021-11-03 4:27:36 PM ******/
CREATE NONCLUSTERED INDEX [IX_ListenerInstance_5] ON [dbo].[ListenerInstance]
(
	[RemainingTrialCount] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[ListenerInstance] ADD  CONSTRAINT [DF_ListenerInstance_Active]  DEFAULT ((1)) FOR [Active]
GO
ALTER TABLE [dbo].[ListenerInstance] ADD  CONSTRAINT [DF_ListenerInstance_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO
ALTER TABLE [dbo].[ListenerInstance] ADD  CONSTRAINT [DF_ListenerInstance_ModifyDate]  DEFAULT (getdate()) FOR [ModifyDate]
GO
ALTER TABLE [dbo].[ListenerInstance] ADD  CONSTRAINT [DF_ListenerInstance_Notes]  DEFAULT ('') FOR [Notes]
GO
ALTER TABLE [dbo].[ListenerInstance]  WITH CHECK ADD  CONSTRAINT [FK_ListenerInstance_EventInstance] FOREIGN KEY([EventInstance_Id])
REFERENCES [dbo].[EventInstance] ([Id])
GO
ALTER TABLE [dbo].[ListenerInstance] CHECK CONSTRAINT [FK_ListenerInstance_EventInstance]
GO
ALTER TABLE [dbo].[ListenerInstance]  WITH CHECK ADD  CONSTRAINT [FK_ListenerInstance_ListenerDefinition] FOREIGN KEY([ListenerDefinition_Id])
REFERENCES [dbo].[ListenerDefinition] ([Id])
GO
ALTER TABLE [dbo].[ListenerInstance] CHECK CONSTRAINT [FK_ListenerInstance_ListenerDefinition]
GO
ALTER TABLE [dbo].[ListenerInstance]  WITH CHECK ADD  CONSTRAINT [FK_ListenerInstance_ListenerInstanceStatus] FOREIGN KEY([ListenerInstanceStatus_Id])
REFERENCES [dbo].[ListenerInstanceStatus] ([Id])
GO
ALTER TABLE [dbo].[ListenerInstance] CHECK CONSTRAINT [FK_ListenerInstance_ListenerInstanceStatus]
GO
