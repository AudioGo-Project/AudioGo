CREATE TABLE [Account] (
  [AccountID] nvarchar(255) PRIMARY KEY,
  [Username] nvarchar(255),
  [PasswordHash] nvarchar(255),
  [Role] nvarchar(255),
  [CreatedAt] datetime DEFAULT (now()),
  [UpdatedAt] datetime
)
GO

CREATE TABLE [POI] (
  [POI_ID] nvarchar(255) PRIMARY KEY,
  [AccountID] nvarchar(255),
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
  [Content_ID] nvarchar(255) PRIMARY KEY,
  [POI_ID] nvarchar(255),
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
  [Image_ID] nvarchar(255) PRIMARY KEY,
  [POI_ID] nvarchar(255),
  [Image_URL] nvarchar(255),
  [SortOrder] int,
  [CreatedAt] datetime DEFAULT (now()),
  [UpdatedAt] datetime
)
GO

CREATE TABLE [Category] (
  [Category_ID] nvarchar(255) PRIMARY KEY,
  [Name] nvarchar(255),
  [CreatedAt] datetime DEFAULT (now()),
  [UpdatedAt] datetime
)
GO

CREATE TABLE [Category_POI] (
  [Category_ID] nvarchar(255),
  [POI_ID] nvarchar(255),
  PRIMARY KEY ([Category_ID], [POI_ID])
)
GO

CREATE TABLE [Tour] (
  [Tour_ID] nvarchar(255) PRIMARY KEY,
  [Name] nvarchar(255),
  [Description] text,
  [CreatedAt] datetime DEFAULT (now()),
  [UpdatedAt] datetime
)
GO

CREATE TABLE [Tour_POI] (
  [Tour_ID] nvarchar(255),
  [POI_ID] nvarchar(255),
  [StepOrder] int,
  PRIMARY KEY ([Tour_ID], [POI_ID])
)
GO

CREATE TABLE [Listen_History] (
  [History_ID] nvarchar(255) PRIMARY KEY,
  [DeviceID] nvarchar(255),
  [POI_ID] nvarchar(255),
  [Timestamp] datetime DEFAULT (now()),
  [ListenDuration] int
)
GO

CREATE TABLE [Location_Log] (
  [Location_ID] nvarchar(255) PRIMARY KEY,
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
