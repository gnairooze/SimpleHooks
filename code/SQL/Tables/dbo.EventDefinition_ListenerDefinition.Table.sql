USE [SimpleHooks]
GO
/****** Object:  Table [dbo].[EventDefinition_ListenerDefinition]    Script Date: 2021-11-03 4:27:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EventDefinition_ListenerDefinition](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[EventDefinition_Id] [bigint] NOT NULL,
	[ListenerDefinition_Id] [bigint] NOT NULL,
	[Active] [bit] NOT NULL,
	[CreateBy] [nvarchar](50) NOT NULL,
	[CreateDate] [datetime2](7) NOT NULL,
	[ModifyBy] [nvarchar](50) NOT NULL,
	[ModifyDate] [datetime2](7) NOT NULL,
	[Notes] [nvarchar](1000) NOT NULL,
 CONSTRAINT [PK_EventDefinition_ListenerDefinition] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Index [IX_EventDefinition_ListenerDefinition]    Script Date: 2021-11-03 4:27:36 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_EventDefinition_ListenerDefinition] ON [dbo].[EventDefinition_ListenerDefinition]
(
	[EventDefinition_Id] ASC,
	[ListenerDefinition_Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_EventDefinition_ListenerDefinition_1]    Script Date: 2021-11-03 4:27:36 PM ******/
CREATE NONCLUSTERED INDEX [IX_EventDefinition_ListenerDefinition_1] ON [dbo].[EventDefinition_ListenerDefinition]
(
	[Active] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[EventDefinition_ListenerDefinition] ADD  CONSTRAINT [DF_EventDefinition_ListenerDefinition_Active]  DEFAULT ((1)) FOR [Active]
GO
ALTER TABLE [dbo].[EventDefinition_ListenerDefinition] ADD  CONSTRAINT [DF_EventDefinition_ListenerDefinition_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO
ALTER TABLE [dbo].[EventDefinition_ListenerDefinition] ADD  CONSTRAINT [DF_EventDefinition_ListenerDefinition_ModifyDate]  DEFAULT (getdate()) FOR [ModifyDate]
GO
ALTER TABLE [dbo].[EventDefinition_ListenerDefinition] ADD  CONSTRAINT [DF_EventDefinition_ListenerDefinition_Notes]  DEFAULT ('') FOR [Notes]
GO
ALTER TABLE [dbo].[EventDefinition_ListenerDefinition]  WITH CHECK ADD  CONSTRAINT [FK_EventDefinition_ListenerDefinition_EventDefinition] FOREIGN KEY([EventDefinition_Id])
REFERENCES [dbo].[EventDefinition] ([Id])
GO
ALTER TABLE [dbo].[EventDefinition_ListenerDefinition] CHECK CONSTRAINT [FK_EventDefinition_ListenerDefinition_EventDefinition]
GO
ALTER TABLE [dbo].[EventDefinition_ListenerDefinition]  WITH CHECK ADD  CONSTRAINT [FK_EventDefinition_ListenerDefinition_ListenerDefinition] FOREIGN KEY([ListenerDefinition_Id])
REFERENCES [dbo].[ListenerDefinition] ([Id])
GO
ALTER TABLE [dbo].[EventDefinition_ListenerDefinition] CHECK CONSTRAINT [FK_EventDefinition_ListenerDefinition_ListenerDefinition]
GO
