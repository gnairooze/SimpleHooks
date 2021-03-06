USE [SimpleHooks]
GO
/****** Object:  Table [dbo].[ListenerDefinition]    Script Date: 2021-11-03 4:27:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ListenerDefinition](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Active] [bit] NOT NULL,
	[URL] [nvarchar](4000) NOT NULL,
	[Headers] [nvarchar](max) NOT NULL,
	[Timeout] [int] NOT NULL,
	[TrialCount] [int] NOT NULL,
	[RetrialDelay] [int] NOT NULL,
	[CreateBy] [nvarchar](50) NOT NULL,
	[CreateDate] [datetime2](7) NOT NULL,
	[ModifyBy] [nvarchar](50) NOT NULL,
	[ModifyDate] [datetime2](7) NOT NULL,
	[Notes] [nvarchar](1000) NOT NULL,
 CONSTRAINT [PK_ListenerDefinition] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_ListenerDefinition]    Script Date: 2021-11-03 4:27:36 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_ListenerDefinition] ON [dbo].[ListenerDefinition]
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ListenerDefinition_1]    Script Date: 2021-11-03 4:27:36 PM ******/
CREATE NONCLUSTERED INDEX [IX_ListenerDefinition_1] ON [dbo].[ListenerDefinition]
(
	[Active] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[ListenerDefinition] ADD  CONSTRAINT [DF_ListenerDefinition_Active]  DEFAULT ((1)) FOR [Active]
GO
ALTER TABLE [dbo].[ListenerDefinition] ADD  CONSTRAINT [DF_ListenerDefinition_Timeout]  DEFAULT ((1)) FOR [Timeout]
GO
ALTER TABLE [dbo].[ListenerDefinition] ADD  CONSTRAINT [DF_ListenerDefinition_TrialCount]  DEFAULT ((1)) FOR [TrialCount]
GO
ALTER TABLE [dbo].[ListenerDefinition] ADD  CONSTRAINT [DF_ListenerDefinition_RetrialDelay]  DEFAULT ((1)) FOR [RetrialDelay]
GO
ALTER TABLE [dbo].[ListenerDefinition] ADD  CONSTRAINT [DF_ListenerDefinition_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO
ALTER TABLE [dbo].[ListenerDefinition] ADD  CONSTRAINT [DF_ListenerDefinition_ModifyDate]  DEFAULT (getdate()) FOR [ModifyDate]
GO
ALTER TABLE [dbo].[ListenerDefinition] ADD  CONSTRAINT [DF_ListenerDefinition_Notes]  DEFAULT ('') FOR [Notes]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'minutes' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ListenerDefinition', @level2type=N'COLUMN',@level2name=N'Timeout'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'minutes' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ListenerDefinition', @level2type=N'COLUMN',@level2name=N'RetrialDelay'
GO
