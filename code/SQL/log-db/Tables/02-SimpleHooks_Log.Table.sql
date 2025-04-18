USE [SimpleHooks_Log_DB]
GO

/****** Object:  Table [dbo].[SimpleHooks_Log]    Script Date: 2021-11-06 10:17:42 AM ******/

CREATE TABLE [dbo].[SimpleHooks_Log](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[LogType] [int] NOT NULL,
	[Owner] [nvarchar](50) NOT NULL,
	[Machine] [nvarchar](50) NOT NULL,
	[Location] [nvarchar](100) NOT NULL,
	[Operation] [nvarchar](50) NOT NULL,
	[Step] [nvarchar](100) NOT NULL,
	[Counter] [int] NOT NULL,
	[Correlation] [uniqueidentifier] NOT NULL,
	[CodeReference] [varchar](200) NOT NULL,
	[ReferenceName] [nvarchar](50) NOT NULL,
	[ReferenceValue] [nvarchar](50) NOT NULL,
	[NotesA] [nvarchar](max) NOT NULL,
	[NotesB] [nvarchar](max) NOT NULL,
	[Duration] [float] NULL,
	[CreateDate] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_SimpleHooks_Log] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[SimpleHooks_Log] ADD  CONSTRAINT [DF_SimpleHooks_Log_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO

ALTER TABLE [dbo].[SimpleHooks_Log]  WITH CHECK ADD  CONSTRAINT [FK_SimpleHooks_Log_LogTypes] FOREIGN KEY([LogType])
REFERENCES [dbo].[LogTypes] ([Id])
GO

ALTER TABLE [dbo].[SimpleHooks_Log] CHECK CONSTRAINT [FK_SimpleHooks_Log_LogTypes]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'in seconds' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SimpleHooks_Log', @level2type=N'COLUMN',@level2name=N'Duration'
GO


