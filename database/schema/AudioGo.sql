CREATE TABLE [Account] (
  [AccountID] int PRIMARY KEY IDENTITY(1, 1),
  [Username] nvarchar(255),
  [PasswordHash] nvarchar(255),
  [Role] nvarchar(255),
  [CreatedAt] datetime DEFAULT (now()),
  [UpdatedAt] datetime
)
GO

CREATE TABLE [POI] (
  [POI_ID] int PRIMARY KEY IDENTITY(1, 1),
  [AccountID] int,
  [Latitude] float,
  [Longitude] float,
  [ActivationRadius] int,
  [Priority] int,
  [Status] nvarchar(255),
  [Logo_URL] nvarchar(255),
  [CreatedAt] datetime DEFAULT (now()),
  [UpdatedAt] datetime
)
GO

CREATE TABLE [POI_Content] (
  [Content_ID] int PRIMARY KEY IDENTITY(1, 1),
  [POI_ID] int,
  [LanguageCode] nvarchar(255),
  [Title] nvarchar(255),
  [Description] text,
  [Audio_URL] nvarchar(255),
  [localAudio_Path] nvarchar(255),
  [isMaster] boolean,
  [CreatedAt] datetime DEFAULT (now()),
  [UpdatedAt] datetime
)
GO

CREATE TABLE [POI_Gallery] (
  [Image_ID] int PRIMARY KEY IDENTITY(1, 1),
  [POI_ID] int,
  [Image_URL] nvarchar(255),
  [SortOrder] int,
  [CreatedAt] datetime DEFAULT (now()),
  [UpdatedAt] datetime
)
GO

CREATE TABLE [Category] (
  [Category_ID] int PRIMARY KEY IDENTITY(1, 1),
  [Name] nvarchar(255),
  [CreatedAt] datetime DEFAULT (now()),
  [UpdatedAt] datetime
)
GO

CREATE TABLE [Category_POI] (
  [Category_ID] int,
  [POI_ID] int,
  PRIMARY KEY ([Category_ID], [POI_ID])
)
GO

CREATE TABLE [Tour] (
  [Tour_ID] int PRIMARY KEY IDENTITY(1, 1),
  [Name] nvarchar(255),
  [Description] text,
  [CreatedAt] datetime DEFAULT (now()),
  [UpdatedAt] datetime
)
GO

CREATE TABLE [Tour_POI] (
  [Tour_ID] int,
  [POI_ID] int,
  [StepOrder] int,
  PRIMARY KEY ([Tour_ID], [POI_ID])
)
GO

CREATE TABLE [Listen_History] (
  [History_ID] int PRIMARY KEY IDENTITY(1, 1),
  [DeviceID] nvarchar(255),
  [POI_ID] int,
  [Timestamp] datetime DEFAULT (now()),
  [ListenDuration] int
)
GO

CREATE TABLE [Location_Log] (
  [Location_ID] int PRIMARY KEY IDENTITY(1, 1),
  [DeviceID] nvarchar(255),
  [Latitude] float,
  [Longitude] float,
  [Timestamp] datetime DEFAULT (now())
)
GO

ALTER TABLE [POI] ADD FOREIGN KEY ([AccountID]) REFERENCES [Account] ([AccountID])
GO

ALTER TABLE [POI_Content] ADD FOREIGN KEY ([POI_ID]) REFERENCES [POI] ([POI_ID])
GO

ALTER TABLE [POI_Gallery] ADD FOREIGN KEY ([POI_ID]) REFERENCES [POI] ([POI_ID])
GO

ALTER TABLE [Category_POI] ADD FOREIGN KEY ([Category_ID]) REFERENCES [Category] ([Category_ID])
GO

ALTER TABLE [Category_POI] ADD FOREIGN KEY ([POI_ID]) REFERENCES [POI] ([POI_ID])
GO

ALTER TABLE [Tour_POI] ADD FOREIGN KEY ([Tour_ID]) REFERENCES [Tour] ([Tour_ID])
GO

ALTER TABLE [Tour_POI] ADD FOREIGN KEY ([POI_ID]) REFERENCES [POI] ([POI_ID])
GO

ALTER TABLE [Listen_History] ADD FOREIGN KEY ([POI_ID]) REFERENCES [POI] ([POI_ID])
GO
