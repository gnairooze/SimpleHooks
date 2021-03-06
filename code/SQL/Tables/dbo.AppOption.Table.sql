USE [SimpleHooks]
GO
/****** Object:  Table [dbo].[AppOption]    Script Date: 2021-11-03 4:27:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AppOption](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Category] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Value] [nvarchar](max) NOT NULL,
	[Active] [bit] NOT NULL,
	[CreateBy] [nvarchar](50) NOT NULL,
	[CreateDate] [datetime2](7) NOT NULL,
	[ModifyBy] [nvarchar](50) NOT NULL,
	[ModifyDate] [datetime2](7) NOT NULL,
	[Notes] [nvarchar](1000) NOT NULL,
 CONSTRAINT [PK_AppOption] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Index [IX_AppOption]    Script Date: 2021-11-03 4:27:36 PM ******/
CREATE NONCLUSTERED INDEX [IX_AppOption] ON [dbo].[AppOption]
(
	[Active] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AppOption_1]    Script Date: 2021-11-03 4:27:36 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_AppOption_1] ON [dbo].[AppOption]
(
	[Category] ASC,
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[AppOption] ADD  CONSTRAINT [DF_Table_1_Active]  DEFAULT ((1)) FOR [Value]
GO
ALTER TABLE [dbo].[AppOption] ADD  CONSTRAINT [DF_AppOption_Active]  DEFAULT ((1)) FOR [Active]
GO
ALTER TABLE [dbo].[AppOption] ADD  CONSTRAINT [DF_AppOption_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO
ALTER TABLE [dbo].[AppOption] ADD  CONSTRAINT [DF_AppOption_ModifyDate]  DEFAULT (getdate()) FOR [ModifyDate]
GO
ALTER TABLE [dbo].[AppOption] ADD  CONSTRAINT [DF_AppOption_Notes]  DEFAULT ('') FOR [Notes]
GO
