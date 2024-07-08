USE [sampledb]
GO

CREATE TABLE [dbo].[SampleModels](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[EventName] [nvarchar](max) NOT NULL,
	[CorrelationId] [nvarchar](50) NOT NULL,
	[EventData] [nvarchar](max) NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_SampleModels] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


