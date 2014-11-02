/****** Object:  Table [dbo].[issues]    Script Date: 2014-10-29 11:55:05 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[issues](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[title] [varchar](255) NULL,
	[description] [varchar](2000) NULL,
	[action] [varchar](255) NULL,
	[reported_at] [datetime] NULL,
	[completed_at] [datetime] NULL,
	[priority] [int] NULL,
 CONSTRAINT [PK_issues] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO
