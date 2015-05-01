
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, and Azure
-- --------------------------------------------------
-- Date Created: 04/29/2015 23:32:54
-- Generated from EDMX file: D:\Repos\mediaviewer\MediaViewer\MediaDatabase\MediaDatabase.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [MediaViewer.MediaDatabase.MediaDatabaseContext];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_TagCategoryTag]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TagSet] DROP CONSTRAINT [FK_TagCategoryTag];
GO
IF OBJECT_ID(N'[dbo].[FK_PresetMetadataTag_PresetMetadata]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[PresetMetadataTag] DROP CONSTRAINT [FK_PresetMetadataTag_PresetMetadata];
GO
IF OBJECT_ID(N'[dbo].[FK_PresetMetadataTag_Tag]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[PresetMetadataTag] DROP CONSTRAINT [FK_PresetMetadataTag_Tag];
GO
IF OBJECT_ID(N'[dbo].[FK_TagTag_Tag]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TagTag] DROP CONSTRAINT [FK_TagTag_Tag];
GO
IF OBJECT_ID(N'[dbo].[FK_TagTag_Tag1]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TagTag] DROP CONSTRAINT [FK_TagTag_Tag1];
GO
IF OBJECT_ID(N'[dbo].[FK_BaseMetadataTag_BaseMetadata]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[BaseMetadataTag] DROP CONSTRAINT [FK_BaseMetadataTag_BaseMetadata];
GO
IF OBJECT_ID(N'[dbo].[FK_BaseMetadataTag_Tag]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[BaseMetadataTag] DROP CONSTRAINT [FK_BaseMetadataTag_Tag];
GO
IF OBJECT_ID(N'[dbo].[FK_ThumbnailBaseMetadata]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[BaseMetadataSet] DROP CONSTRAINT [FK_ThumbnailBaseMetadata];
GO
IF OBJECT_ID(N'[dbo].[FK_ImageMetadata_inherits_BaseMetadata]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[BaseMetadataSet_ImageMetadata] DROP CONSTRAINT [FK_ImageMetadata_inherits_BaseMetadata];
GO
IF OBJECT_ID(N'[dbo].[FK_VideoMetadata_inherits_BaseMetadata]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[BaseMetadataSet_VideoMetadata] DROP CONSTRAINT [FK_VideoMetadata_inherits_BaseMetadata];
GO
IF OBJECT_ID(N'[dbo].[FK_UnknownMetadata_inherits_BaseMetadata]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[BaseMetadataSet_UnknownMetadata] DROP CONSTRAINT [FK_UnknownMetadata_inherits_BaseMetadata];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[TagSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TagSet];
GO
IF OBJECT_ID(N'[dbo].[TagCategorySet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TagCategorySet];
GO
IF OBJECT_ID(N'[dbo].[BaseMetadataSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[BaseMetadataSet];
GO
IF OBJECT_ID(N'[dbo].[PresetMetadataSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[PresetMetadataSet];
GO
IF OBJECT_ID(N'[dbo].[ThumbnailSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ThumbnailSet];
GO
IF OBJECT_ID(N'[dbo].[BaseMetadataSet_ImageMetadata]', 'U') IS NOT NULL
    DROP TABLE [dbo].[BaseMetadataSet_ImageMetadata];
GO
IF OBJECT_ID(N'[dbo].[BaseMetadataSet_VideoMetadata]', 'U') IS NOT NULL
    DROP TABLE [dbo].[BaseMetadataSet_VideoMetadata];
GO
IF OBJECT_ID(N'[dbo].[BaseMetadataSet_UnknownMetadata]', 'U') IS NOT NULL
    DROP TABLE [dbo].[BaseMetadataSet_UnknownMetadata];
GO
IF OBJECT_ID(N'[dbo].[PresetMetadataTag]', 'U') IS NOT NULL
    DROP TABLE [dbo].[PresetMetadataTag];
GO
IF OBJECT_ID(N'[dbo].[TagTag]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TagTag];
GO
IF OBJECT_ID(N'[dbo].[BaseMetadataTag]', 'U') IS NOT NULL
    DROP TABLE [dbo].[BaseMetadataTag];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'TagSet'
CREATE TABLE [dbo].[TagSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [TagCategoryId] int  NULL,
    [Used] bigint  NOT NULL,
    [TimeStamp] TIMESTAMP  NOT NULL
);
GO

-- Creating table 'TagCategorySet'
CREATE TABLE [dbo].[TagCategorySet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [TimeStamp] TIMESTAMP  NOT NULL
);
GO

-- Creating table 'BaseMetadataSet'
CREATE TABLE [dbo].[BaseMetadataSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Location] nvarchar(max)  NOT NULL,
    [Title] nvarchar(max)  NULL,
    [Rating] float  NULL,
    [Description] nvarchar(max)  NULL,
    [Author] nvarchar(max)  NULL,
    [Copyright] nvarchar(max)  NULL,
    [LastModifiedDate] datetime  NOT NULL,
    [CreationDate] datetime  NULL,
    [MetadataModifiedDate] datetime  NULL,
    [MetadataDate] datetime  NULL,
    [MimeType] nvarchar(max)  NOT NULL,
    [SizeBytes] bigint  NOT NULL,
    [Software] nvarchar(max)  NULL,
    [SupportsXMPMetadata] bit  NOT NULL,
    [TimeStamp] TIMESTAMP  NOT NULL,
    [Latitude] float  NULL,
    [Longitude] float  NULL,
    [FileDate] datetime  NOT NULL,
    [ThumbnailId] int  NULL
);
GO

-- Creating table 'PresetMetadataSet'
CREATE TABLE [dbo].[PresetMetadataSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [Title] nvarchar(max)  NULL,
    [Rating] float  NULL,
    [Description] nvarchar(max)  NULL,
    [Author] nvarchar(max)  NULL,
    [Copyright] nvarchar(max)  NULL,
    [IsNameEnabled] bit  NOT NULL,
    [IsTitleEnabled] bit  NOT NULL,
    [IsRatingEnabled] bit  NOT NULL,
    [IsDescriptionEnabled] bit  NOT NULL,
    [IsAuthorEnabled] bit  NOT NULL,
    [IsCopyrightEnabled] bit  NOT NULL,
    [CreationDate] datetime  NULL,
    [IsCreationDateEnabled] bit  NOT NULL,
    [TimeStamp] TIMESTAMP  NOT NULL
);
GO

-- Creating table 'ThumbnailSet'
CREATE TABLE [dbo].[ThumbnailSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [ImageData] varbinary(max)  NOT NULL,
    [Width] smallint  NOT NULL,
    [Height] smallint  NOT NULL
);
GO

-- Creating table 'BaseMetadataSet_ImageMetadata'
CREATE TABLE [dbo].[BaseMetadataSet_ImageMetadata] (
    [Width] int  NOT NULL,
    [Height] int  NOT NULL,
    [LightSource] smallint  NULL,
    [MeteringMode] smallint  NULL,
    [Saturation] smallint  NULL,
    [SceneCaptureType] smallint  NULL,
    [SensingMethod] smallint  NULL,
    [Sharpness] smallint  NULL,
    [SubjectDistance] float  NULL,
    [ShutterSpeedValue] float  NULL,
    [SubjectDistanceRange] smallint  NULL,
    [WhiteBalance] smallint  NULL,
    [FlashFired] bit  NULL,
    [FlashMode] smallint  NULL,
    [FlashReturn] smallint  NULL,
    [CameraMake] nvarchar(max)  NULL,
    [CameraModel] nvarchar(max)  NULL,
    [Lens] nvarchar(max)  NULL,
    [SerialNumber] nvarchar(max)  NULL,
    [ExposureTime] float  NULL,
    [FNumber] float  NULL,
    [ExposureBiasValue] float  NULL,
    [ExposureProgram] smallint  NULL,
    [FocalLength] float  NULL,
    [ISOSpeedRating] int  NULL,
    [Contrast] smallint  NULL,
    [Orientation] smallint  NULL,
    [Id] int  NOT NULL
);
GO

-- Creating table 'BaseMetadataSet_VideoMetadata'
CREATE TABLE [dbo].[BaseMetadataSet_VideoMetadata] (
    [Width] int  NOT NULL,
    [Height] int  NOT NULL,
    [BitsPerSample] smallint  NULL,
    [DurationSeconds] int  NOT NULL,
    [FramesPerSecond] float  NOT NULL,
    [NrChannels] smallint  NULL,
    [PixelFormat] nvarchar(max)  NULL,
    [SamplesPerSecond] int  NULL,
    [VideoCodec] nvarchar(max)  NULL,
    [VideoContainer] nvarchar(max)  NULL,
    [AudioCodec] nvarchar(max)  NULL,
    [MajorBrand] nvarchar(max)  NULL,
    [MinorVersion] int  NULL,
    [IsVariableBitRate] bit  NULL,
    [WMFSDKVersion] nvarchar(max)  NULL,
    [Id] int  NOT NULL
);
GO

-- Creating table 'BaseMetadataSet_UnknownMetadata'
CREATE TABLE [dbo].[BaseMetadataSet_UnknownMetadata] (
    [Id] int  NOT NULL
);
GO

-- Creating table 'PresetMetadataTag'
CREATE TABLE [dbo].[PresetMetadataTag] (
    [PresetMetadataTag_Tag_Id] int  NOT NULL,
    [Tags_Id] int  NOT NULL
);
GO

-- Creating table 'TagTag'
CREATE TABLE [dbo].[TagTag] (
    [ParentTags_Id] int  NOT NULL,
    [ChildTags_Id] int  NOT NULL
);
GO

-- Creating table 'BaseMetadataTag'
CREATE TABLE [dbo].[BaseMetadataTag] (
    [BaseMetadatas_Id] int  NOT NULL,
    [Tags_Id] int  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'TagSet'
ALTER TABLE [dbo].[TagSet]
ADD CONSTRAINT [PK_TagSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'TagCategorySet'
ALTER TABLE [dbo].[TagCategorySet]
ADD CONSTRAINT [PK_TagCategorySet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'BaseMetadataSet'
ALTER TABLE [dbo].[BaseMetadataSet]
ADD CONSTRAINT [PK_BaseMetadataSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'PresetMetadataSet'
ALTER TABLE [dbo].[PresetMetadataSet]
ADD CONSTRAINT [PK_PresetMetadataSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'ThumbnailSet'
ALTER TABLE [dbo].[ThumbnailSet]
ADD CONSTRAINT [PK_ThumbnailSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'BaseMetadataSet_ImageMetadata'
ALTER TABLE [dbo].[BaseMetadataSet_ImageMetadata]
ADD CONSTRAINT [PK_BaseMetadataSet_ImageMetadata]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'BaseMetadataSet_VideoMetadata'
ALTER TABLE [dbo].[BaseMetadataSet_VideoMetadata]
ADD CONSTRAINT [PK_BaseMetadataSet_VideoMetadata]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'BaseMetadataSet_UnknownMetadata'
ALTER TABLE [dbo].[BaseMetadataSet_UnknownMetadata]
ADD CONSTRAINT [PK_BaseMetadataSet_UnknownMetadata]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [PresetMetadataTag_Tag_Id], [Tags_Id] in table 'PresetMetadataTag'
ALTER TABLE [dbo].[PresetMetadataTag]
ADD CONSTRAINT [PK_PresetMetadataTag]
    PRIMARY KEY NONCLUSTERED ([PresetMetadataTag_Tag_Id], [Tags_Id] ASC);
GO

-- Creating primary key on [ParentTags_Id], [ChildTags_Id] in table 'TagTag'
ALTER TABLE [dbo].[TagTag]
ADD CONSTRAINT [PK_TagTag]
    PRIMARY KEY NONCLUSTERED ([ParentTags_Id], [ChildTags_Id] ASC);
GO

-- Creating primary key on [BaseMetadatas_Id], [Tags_Id] in table 'BaseMetadataTag'
ALTER TABLE [dbo].[BaseMetadataTag]
ADD CONSTRAINT [PK_BaseMetadataTag]
    PRIMARY KEY NONCLUSTERED ([BaseMetadatas_Id], [Tags_Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [TagCategoryId] in table 'TagSet'
ALTER TABLE [dbo].[TagSet]
ADD CONSTRAINT [FK_TagCategoryTag]
    FOREIGN KEY ([TagCategoryId])
    REFERENCES [dbo].[TagCategorySet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_TagCategoryTag'
CREATE INDEX [IX_FK_TagCategoryTag]
ON [dbo].[TagSet]
    ([TagCategoryId]);
GO

-- Creating foreign key on [PresetMetadataTag_Tag_Id] in table 'PresetMetadataTag'
ALTER TABLE [dbo].[PresetMetadataTag]
ADD CONSTRAINT [FK_PresetMetadataTag_PresetMetadata]
    FOREIGN KEY ([PresetMetadataTag_Tag_Id])
    REFERENCES [dbo].[PresetMetadataSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Tags_Id] in table 'PresetMetadataTag'
ALTER TABLE [dbo].[PresetMetadataTag]
ADD CONSTRAINT [FK_PresetMetadataTag_Tag]
    FOREIGN KEY ([Tags_Id])
    REFERENCES [dbo].[TagSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_PresetMetadataTag_Tag'
CREATE INDEX [IX_FK_PresetMetadataTag_Tag]
ON [dbo].[PresetMetadataTag]
    ([Tags_Id]);
GO

-- Creating foreign key on [ParentTags_Id] in table 'TagTag'
ALTER TABLE [dbo].[TagTag]
ADD CONSTRAINT [FK_TagTag_Tag]
    FOREIGN KEY ([ParentTags_Id])
    REFERENCES [dbo].[TagSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [ChildTags_Id] in table 'TagTag'
ALTER TABLE [dbo].[TagTag]
ADD CONSTRAINT [FK_TagTag_Tag1]
    FOREIGN KEY ([ChildTags_Id])
    REFERENCES [dbo].[TagSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_TagTag_Tag1'
CREATE INDEX [IX_FK_TagTag_Tag1]
ON [dbo].[TagTag]
    ([ChildTags_Id]);
GO

-- Creating foreign key on [BaseMetadatas_Id] in table 'BaseMetadataTag'
ALTER TABLE [dbo].[BaseMetadataTag]
ADD CONSTRAINT [FK_BaseMetadataTag_BaseMetadata]
    FOREIGN KEY ([BaseMetadatas_Id])
    REFERENCES [dbo].[BaseMetadataSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Tags_Id] in table 'BaseMetadataTag'
ALTER TABLE [dbo].[BaseMetadataTag]
ADD CONSTRAINT [FK_BaseMetadataTag_Tag]
    FOREIGN KEY ([Tags_Id])
    REFERENCES [dbo].[TagSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_BaseMetadataTag_Tag'
CREATE INDEX [IX_FK_BaseMetadataTag_Tag]
ON [dbo].[BaseMetadataTag]
    ([Tags_Id]);
GO

-- Creating foreign key on [ThumbnailId] in table 'BaseMetadataSet'
ALTER TABLE [dbo].[BaseMetadataSet]
ADD CONSTRAINT [FK_ThumbnailBaseMetadata]
    FOREIGN KEY ([ThumbnailId])
    REFERENCES [dbo].[ThumbnailSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ThumbnailBaseMetadata'
CREATE INDEX [IX_FK_ThumbnailBaseMetadata]
ON [dbo].[BaseMetadataSet]
    ([ThumbnailId]);
GO

-- Creating foreign key on [Id] in table 'BaseMetadataSet_ImageMetadata'
ALTER TABLE [dbo].[BaseMetadataSet_ImageMetadata]
ADD CONSTRAINT [FK_ImageMetadata_inherits_BaseMetadata]
    FOREIGN KEY ([Id])
    REFERENCES [dbo].[BaseMetadataSet]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Id] in table 'BaseMetadataSet_VideoMetadata'
ALTER TABLE [dbo].[BaseMetadataSet_VideoMetadata]
ADD CONSTRAINT [FK_VideoMetadata_inherits_BaseMetadata]
    FOREIGN KEY ([Id])
    REFERENCES [dbo].[BaseMetadataSet]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Id] in table 'BaseMetadataSet_UnknownMetadata'
ALTER TABLE [dbo].[BaseMetadataSet_UnknownMetadata]
ADD CONSTRAINT [FK_UnknownMetadata_inherits_BaseMetadata]
    FOREIGN KEY ([Id])
    REFERENCES [dbo].[BaseMetadataSet]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------