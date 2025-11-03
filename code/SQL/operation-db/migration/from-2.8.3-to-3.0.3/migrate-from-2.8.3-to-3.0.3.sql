use SimpleHooks;
GO

CREATE TABLE [dbo].[ListenerType](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Path] [nvarchar](1000) NOT NULL,
	[CreateBy] [nvarchar](50) NOT NULL,
	[CreateDate] [datetime2](7) NOT NULL,
	[ModifyBy] [nvarchar](50) NOT NULL,
	[ModifyDate] [datetime2](7) NOT NULL,
	[Notes] [nvarchar](1000) NOT NULL,
 CONSTRAINT [PK_ListenerType] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_ListenerType] ON [dbo].[ListenerType]
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[ListenerType] ADD  CONSTRAINT [DF_ListenerType_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO
ALTER TABLE [dbo].[ListenerType] ADD  CONSTRAINT [DF_ListenerType_ModifyDate]  DEFAULT (getdate()) FOR [ModifyDate]
GO
ALTER TABLE [dbo].[ListenerType] ADD  CONSTRAINT [DF_ListenerType_Notes]  DEFAULT ('') FOR [Notes]
GO

-- add data for table ListenerType
insert ListenerType(Id, [Name], [Path], CreateBy, ModifyBy, Notes) values (1, 'Anonymous', 'listener-plugins/anonymous/SimpleHooks.ListenerPlugins.Anonymous.dll', 'system.admin', 'system.admin', '')
insert ListenerType(Id, [Name], [Path], CreateBy, ModifyBy, Notes) values (2, 'TypeA', 'listener-plugins/typea/SimpleHooks.ListenerPlugins.TypeA.dll', 'system.admin', 'system.admin', 'include Bearer token in header')

-- alter table ListenerDefinition by adding column Type_Id int not null default 1
ALTER TABLE [dbo].[ListenerDefinition] ADD [Type_Id] [int] NOT NULL DEFAULT ((1)), [Type_Options] [nvarchar](max) NOT NULL DEFAULT ('')
GO

-- add foreign key constraint FK_ListenerDefinition_ListenerType on ListenerDefinition(Type_Id) references ListenerType(Id)
ALTER TABLE [dbo].[ListenerDefinition]  WITH CHECK ADD  CONSTRAINT [FK_ListenerDefinition_ListenerType] FOREIGN KEY([Type_Id])
REFERENCES [dbo].[ListenerType] ([Id])
GO
ALTER TABLE [dbo].[ListenerDefinition] CHECK CONSTRAINT [FK_ListenerDefinition_ListenerType]
GO

alter procedure dbo.ListenerDefinition_GetAll
as
begin
	select
		Id,
		[Name],
		Active,
		CreateBy,
		CreateDate,
		ModifyBy,
		ModifyDate,
		Notes,
		[URL],
		Headers,
		[Timeout],
		TrialCount,
		RetrialDelay,
		Type_Id,
		Type_Options
	from ListenerDefinition
end

go

create procedure dbo.ListenerType_GetAll
as
begin
	select
		Id,
		[Name],
		[Path],
		CreateBy,
		CreateDate,
		ModifyBy,
		ModifyDate,
		Notes
	from ListenerType
end

go
