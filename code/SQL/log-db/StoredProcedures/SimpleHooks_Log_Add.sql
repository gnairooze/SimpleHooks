USE [SimpleHooks_Log_DB]
GO
/****** Object:  StoredProcedure [dbo].[SimpleHooks_Log_Add]    Script Date: 2021-11-06 10:16:47 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create procedure [dbo].[SimpleHooks_Log_Add]
@Id bigint out,
@LogType int,
@Owner nvarchar(50),
@Machine nvarchar(50),
@Location nvarchar(100),
@Operation nvarchar(50),
@Step nvarchar(100),
@Counter int,
@Correlation uniqueidentifier,
@CodeReference varchar(200),
@ReferenceName nvarchar(50),
@ReferenceValue nvarchar(50),
@NotesA nvarchar(max),
@NotesB nvarchar(max),
@Duration float = null,
@CreateDate	datetime2
as
begin
	insert SimpleHooks_Log
	(
		LogType,
		[Owner],
		Machine,
		[Location],
		Operation,
		Step,
		[Counter],
		Correlation,
		CodeReference,
		ReferenceName,
		ReferenceValue,
		NotesA,
		NotesB,
		Duration,
		CreateDate
	)
	values
	(
		@LogType,
		@Owner,
		@Machine,
		@Location,
		@Operation,
		@Step,
		@Counter,
		@Correlation,
		@CodeReference,
		@ReferenceName,
		@ReferenceValue,
		@NotesA,
		@NotesB,
		@Duration,
		@CreateDate
	)

	set @Id = SCOPE_IDENTITY()
end
