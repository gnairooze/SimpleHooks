use SimpleHooks;
GO

-- create table ListenerDefinitionType
CREATE TABLE [dbo].[ListenerDefinitionType](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Path] [nvarchar](1000) NOT NULL,
	[CreateBy] [nvarchar](50) NOT NULL,
	[CreateDate] [datetime2](7) NOT NULL,
	[ModifyBy] [nvarchar](50) NOT NULL,
	[ModifyDate] [datetime2](7) NOT NULL,
	[Notes] [nvarchar](1000) NOT NULL,
 CONSTRAINT [PK_ListenerDefinitionAuthType] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_ListenerDefinitionType] ON [dbo].[ListenerDefinitionType]
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[ListenerDefinitionType] ADD  CONSTRAINT [DF_ListenerDefinitionType_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO
ALTER TABLE [dbo].[ListenerDefinitionType] ADD  CONSTRAINT [DF_ListenerDefinitionType_ModifyDate]  DEFAULT (getdate()) FOR [ModifyDate]
GO
ALTER TABLE [dbo].[ListenerDefinitionType] ADD  CONSTRAINT [DF_ListenerDefinitionType_Notes]  DEFAULT ('') FOR [Notes]
GO

-- add data for table ListenerDefinitionType
INSERT INTO [dbo].[ListenerDefinitionType] ([Id], [Name], [Path], [CreateBy], [CreateDate], [ModifyBy], [ModifyDate], [Notes])
VALUES
(1, 'Anonymous', 'listener-plugins/Anonymous/Anonymous.dll', 'system', GETDATE(), 'system', GETDATE(), ''),
(2, 'TypeA', 'listener-plugins/TypeA/ListenerTypeA.dll', 'system', GETDATE(), 'system', GETDATE(), 'include Bearer token in header');

-- alter table ListenerDefinition by adding column Type_Id int not null default 1
ALTER TABLE [dbo].[ListenerDefinition] ADD [Type_Id] [int] NOT NULL DEFAULT ((1)), [Type_Options] [nvarchar](max) NOT NULL DEFAULT ('{}')
GO

-- add foreign key constraint FK_ListenerDefinition_ListenerDefinitionType on ListenerDefinition(Type_Id) references ListenerDefinitionType(Id)
ALTER TABLE [dbo].[ListenerDefinition]  WITH CHECK ADD  CONSTRAINT [FK_ListenerDefinition_ListenerDefinitionType] FOREIGN KEY([Type_Id])
REFERENCES [dbo].[ListenerDefinitionType] ([Id])
GO
ALTER TABLE [dbo].[ListenerDefinition] CHECK CONSTRAINT [FK_ListenerDefinition_ListenerDefinitionType]
GO
