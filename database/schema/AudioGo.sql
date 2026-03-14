CREATE TABLE [Account] (
  [AccountId] nvarchar(255) PRIMARY KEY,
  [Username] nvarchar(255),
  [PasswordHash] nvarchar(255),
  [Role] nvarchar(255),
  [CreatedAt] datetime DEFAULT (GETDATE()),
  [UpdatedAt] datetime
)
GO

CREATE TABLE [Poi] (
  [PoiId] nvarchar(255) PRIMARY KEY,
  [AccountId] nvarchar(255),
  [Latitude] float,
  [Longitude] float,
  [ActivationRadius] int,
  [Priority] int,
  [Status] nvarchar(255),
  [LogoUrl] nvarchar(255),
  [CreatedAt] datetime DEFAULT (GETDATE()),
  [UpdatedAt] datetime
)
GO

CREATE TABLE [PoiContent] (
  [ContentId] nvarchar(255) PRIMARY KEY,
  [PoiId] nvarchar(255),
  [LanguageCode] nvarchar(255),
  [Title] nvarchar(255),
  [Description] text,
  [AudioUrl] nvarchar(255),
  [LocalAudioPath] nvarchar(255),
  [IsMaster] bit,
  [CreatedAt] datetime DEFAULT (GETDATE()),
  [UpdatedAt] datetime
)
GO

CREATE TABLE [PoiGallery] (
  [ImageId] nvarchar(255) PRIMARY KEY,
  [PoiId] nvarchar(255),
  [ImageUrl] nvarchar(255),
  [SortOrder] int,
  [CreatedAt] datetime DEFAULT (GETDATE()),
  [UpdatedAt] datetime
)
GO

CREATE TABLE [Category] (
  [CategoryId] nvarchar(255) PRIMARY KEY,
  [Name] nvarchar(255),
  [CreatedAt] datetime DEFAULT (GETDATE()),
  [UpdatedAt] datetime
)
GO

CREATE TABLE [CategoryPoi] (
  [CategoryId] nvarchar(255),
  [PoiId] nvarchar(255),
  PRIMARY KEY ([CategoryId], [PoiId])
)
GO

CREATE TABLE [Tour] (
  [TourId] nvarchar(255) PRIMARY KEY,
  [Name] nvarchar(255),
  [Description] text,
  [CreatedAt] datetime DEFAULT (GETDATE()),
  [UpdatedAt] datetime
)
GO

CREATE TABLE [TourPoi] (
  [TourId] nvarchar(255),
  [PoiId] nvarchar(255),
  [StepOrder] int,
  PRIMARY KEY ([TourId], [PoiId])
)
GO

CREATE TABLE [ListenHistory] (
  [HistoryId] nvarchar(255) PRIMARY KEY,
  [DeviceId] nvarchar(255),
  [PoiId] nvarchar(255),
  [Timestamp] datetime DEFAULT (GETDATE()),
  [ListenDuration] int
)
GO

CREATE TABLE [LocationLog] (
  [LocationId] nvarchar(255) PRIMARY KEY,
  [DeviceId] nvarchar(255),
  [Latitude] float,
  [Longitude] float,
  [Timestamp] datetime DEFAULT (GETDATE())
)
GO

ALTER TABLE [Poi] ADD FOREIGN KEY ([AccountId]) REFERENCES [Account] ([AccountId])
GO

ALTER TABLE [PoiContent] ADD FOREIGN KEY ([PoiId]) REFERENCES [Poi] ([PoiId])
GO

ALTER TABLE [PoiGallery] ADD FOREIGN KEY ([PoiId]) REFERENCES [Poi] ([PoiId])
GO

ALTER TABLE [CategoryPoi] ADD FOREIGN KEY ([CategoryId]) REFERENCES [Category] ([CategoryId])
GO

ALTER TABLE [CategoryPoi] ADD FOREIGN KEY ([PoiId]) REFERENCES [Poi] ([PoiId])
GO

ALTER TABLE [TourPoi] ADD FOREIGN KEY ([TourId]) REFERENCES [Tour] ([TourId])
GO

ALTER TABLE [TourPoi] ADD FOREIGN KEY ([PoiId]) REFERENCES [Poi] ([PoiId])
GO

ALTER TABLE [ListenHistory] ADD FOREIGN KEY ([PoiId]) REFERENCES [Poi] ([PoiId])
GO
