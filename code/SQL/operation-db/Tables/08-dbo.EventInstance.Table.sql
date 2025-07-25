USE [SimpleHooks]
GO
/****** Object:  Table [dbo].[EventInstance]    Script Date: 2021-11-03 4:27:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EventInstance](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[EventDefinition_Id] [bigint] NOT NULL,
	[BusinessId] [uniqueidentifier] NOT NULL,
	[EventData] [nvarchar](max) NOT NULL,
	[ReferenceName] [nvarchar](50) NOT NULL,
	[ReferenceValue] [nvarchar](100) NOT NULL,
	[EventInstanceStatus_Id] [int] NOT NULL,
	[Active] [bit] NOT NULL,
	[CreateBy] [nvarchar](50) NOT NULL,
	[CreateDate] [datetime2](7) NOT NULL,
	[ModifyBy] [nvarchar](50) NOT NULL,
	[ModifyDate] [datetime2](7) NOT NULL,
	[Notes] [nvarchar](1000) NOT NULL,
	[GroupId] [int] NOT NULL,
	[TimeStamp] [timestamp] NOT NULL,
 CONSTRAINT [PK_EventInstance] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Index [IX_EventInstance]    Script Date: 2021-11-03 4:27:36 PM ******/
CREATE NONCLUSTERED INDEX [IX_EventInstance] ON [dbo].[EventInstance]
(
	[EventDefinition_Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_EventInstance_1]    Script Date: 2021-11-03 4:27:36 PM ******/
CREATE NONCLUSTERED INDEX [IX_EventInstance_1] ON [dbo].[EventInstance]
(
	[EventInstanceStatus_Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_EventInstance_2]    Script Date: 2021-11-03 4:27:36 PM ******/
CREATE NONCLUSTERED INDEX [IX_EventInstance_2] ON [dbo].[EventInstance]
(
	[Active] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_EventInstance_BusinessId] ON [dbo].[EventInstance]
(
	[BusinessId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[EventInstance] ADD  CONSTRAINT [DF_EventInstance_Active]  DEFAULT ((1)) FOR [Active]
GO
ALTER TABLE [dbo].[EventInstance] ADD  CONSTRAINT [DF_EventInstance_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO
ALTER TABLE [dbo].[EventInstance] ADD  CONSTRAINT [DF_EventInstance_ModifyDate]  DEFAULT (getdate()) FOR [ModifyDate]
GO
ALTER TABLE [dbo].[EventInstance] ADD  CONSTRAINT [DF_EventInstance_Notes]  DEFAULT ('') FOR [Notes]
GO
ALTER TABLE [dbo].[EventInstance] ADD  CONSTRAINT [DF_EventInstance_GroupId]  DEFAULT (1) FOR [GroupId]
GO
ALTER TABLE [dbo].[EventInstance]  WITH CHECK ADD  CONSTRAINT [FK_EventInstance_EventDefinition] FOREIGN KEY([EventDefinition_Id])
REFERENCES [dbo].[EventDefinition] ([Id])
GO
ALTER TABLE [dbo].[EventInstance] CHECK CONSTRAINT [FK_EventInstance_EventDefinition]
GO
ALTER TABLE [dbo].[EventInstance]  WITH CHECK ADD  CONSTRAINT [FK_EventInstance_EventInstanceStatus] FOREIGN KEY([EventInstanceStatus_Id])
REFERENCES [dbo].[EventInstanceStatus] ([Id])
GO
ALTER TABLE [dbo].[EventInstance] CHECK CONSTRAINT [FK_EventInstance_EventInstanceStatus]
GO
