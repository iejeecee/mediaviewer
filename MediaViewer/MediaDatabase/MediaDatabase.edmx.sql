
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, and Azure
-- --------------------------------------------------
-- Date Created: 09/08/2014 19:56:31
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
IF OBJECT_ID(N'[dbo].[FK_BaseMediaTag_BaseMedia]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[BaseMediaTag] DROP CONSTRAINT [FK_BaseMediaTag_BaseMedia];
GO
IF OBJECT_ID(N'[dbo].[FK_BaseMediaTag_Tag]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[BaseMediaTag] DROP CONSTRAINT [FK_BaseMediaTag_Tag];
GO
IF OBJECT_ID(N'[dbo].[FK_BaseMediaThumbnail]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ThumbnailSet] DROP CONSTRAINT [FK_BaseMediaThumbnail];
GO
IF OBJECT_ID(N'[dbo].[FK_ImageMedia_inherits_BaseMedia]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[BaseMediaSet_ImageMedia] DROP CONSTRAINT [FK_ImageMedia_inherits_BaseMedia];
GO
IF OBJECT_ID(N'[dbo].[FK_VideoMedia_inherits_BaseMedia]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[BaseMediaSet_VideoMedia] DROP CONSTRAINT [FK_VideoMedia_inherits_BaseMedia];
GO
IF OBJECT_ID(N'[dbo].[FK_UnknownMedia_inherits_BaseMedia]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[BaseMediaSet_UnknownMedia] DROP CONSTRAINT [FK_UnknownMedia_inherits_BaseMedia];
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
IF OBJECT_ID(N'[dbo].[BaseMediaSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[BaseMediaSet];
GO
IF OBJECT_ID(N'[dbo].[PresetMetadataSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[PresetMetadataSet];
GO
IF OBJECT_ID(N'[dbo].[ThumbnailSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ThumbnailSet];
GO
IF OBJECT_ID(N'[dbo].[BaseMediaSet_ImageMedia]', 'U') IS NOT NULL
    DROP TABLE [dbo].[BaseMediaSet_ImageMedia];
GO
IF OBJECT_ID(N'[dbo].[BaseMediaSet_VideoMedia]', 'U') IS NOT NULL
    DROP TABLE [dbo].[BaseMediaSet_VideoMedia];
GO
IF OBJECT_ID(N'[dbo].[BaseMediaSet_UnknownMedia]', 'U') IS NOT NULL
    DROP TABLE [dbo].[BaseMediaSet_UnknownMedia];
GO
IF OBJECT_ID(N'[dbo].[PresetMetadataTag]', 'U') IS NOT NULL
    DROP TABLE [dbo].[PresetMetadataTag];
GO
IF OBJECT_ID(N'[dbo].[TagTag]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TagTag];
GO
IF OBJECT_ID(N'[dbo].[BaseMediaTag]', 'U') IS NOT NULL
    DROP TABLE [dbo].[BaseMediaTag];
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

-- Creating table 'BaseMediaSet'
CREATE TABLE [dbo].[BaseMediaSet] (
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
    [Latitude] nvarchar(max)  NULL,
    [Longitude] nvarchar(max)  NULL,
    [FileDate] datetime  NOT NULL
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
    [Height] smallint  NOT NULL,
    [BaseMedia_Id] int  NOT NULL
);
GO

-- Creating table 'BaseMediaSet_ImageMedia'
CREATE TABLE [dbo].[BaseMediaSet_ImageMedia] (
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
    [Id] int  NOT NULL
);
GO

-- Creating table 'BaseMediaSet_VideoMedia'
CREATE TABLE [dbo].[BaseMediaSet_VideoMedia] (
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

-- Creating table 'BaseMediaSet_UnknownMedia'
CREATE TABLE [dbo].[BaseMediaSet_UnknownMedia] (
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

-- Creating table 'BaseMediaTag'
CREATE TABLE [dbo].[BaseMediaTag] (
    [BaseMedias_Id] int  NOT NULL,
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

-- Creating primary key on [Id] in table 'BaseMediaSet'
ALTER TABLE [dbo].[BaseMediaSet]
ADD CONSTRAINT [PK_BaseMediaSet]
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

-- Creating primary key on [Id] in table 'BaseMediaSet_ImageMedia'
ALTER TABLE [dbo].[BaseMediaSet_ImageMedia]
ADD CONSTRAINT [PK_BaseMediaSet_ImageMedia]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'BaseMediaSet_VideoMedia'
ALTER TABLE [dbo].[BaseMediaSet_VideoMedia]
ADD CONSTRAINT [PK_BaseMediaSet_VideoMedia]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'BaseMediaSet_UnknownMedia'
ALTER TABLE [dbo].[BaseMediaSet_UnknownMedia]
ADD CONSTRAINT [PK_BaseMediaSet_UnknownMedia]
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

-- Creating primary key on [BaseMedias_Id], [Tags_Id] in table 'BaseMediaTag'
ALTER TABLE [dbo].[BaseMediaTag]
ADD CONSTRAINT [PK_BaseMediaTag]
    PRIMARY KEY NONCLUSTERED ([BaseMedias_Id], [Tags_Id] ASC);
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

-- Creating foreign key on [BaseMedias_Id] in table 'BaseMediaTag'
ALTER TABLE [dbo].[BaseMediaTag]
ADD CONSTRAINT [FK_BaseMediaTag_BaseMedia]
    FOREIGN KEY ([BaseMedias_Id])
    REFERENCES [dbo].[BaseMediaSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Tags_Id] in table 'BaseMediaTag'
ALTER TABLE [dbo].[BaseMediaTag]
ADD CONSTRAINT [FK_BaseMediaTag_Tag]
    FOREIGN KEY ([Tags_Id])
    REFERENCES [dbo].[TagSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_BaseMediaTag_Tag'
CREATE INDEX [IX_FK_BaseMediaTag_Tag]
ON [dbo].[BaseMediaTag]
    ([Tags_Id]);
GO

-- Creating foreign key on [BaseMedia_Id] in table 'ThumbnailSet'
ALTER TABLE [dbo].[ThumbnailSet]
ADD CONSTRAINT [FK_BaseMediaThumbnail]
    FOREIGN KEY ([BaseMedia_Id])
    REFERENCES [dbo].[BaseMediaSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_BaseMediaThumbnail'
CREATE INDEX [IX_FK_BaseMediaThumbnail]
ON [dbo].[ThumbnailSet]
    ([BaseMedia_Id]);
GO

-- Creating foreign key on [Id] in table 'BaseMediaSet_ImageMedia'
ALTER TABLE [dbo].[BaseMediaSet_ImageMedia]
ADD CONSTRAINT [FK_ImageMedia_inherits_BaseMedia]
    FOREIGN KEY ([Id])
    REFERENCES [dbo].[BaseMediaSet]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Id] in table 'BaseMediaSet_VideoMedia'
ALTER TABLE [dbo].[BaseMediaSet_VideoMedia]
ADD CONSTRAINT [FK_VideoMedia_inherits_BaseMedia]
    FOREIGN KEY ([Id])
    REFERENCES [dbo].[BaseMediaSet]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Id] in table 'BaseMediaSet_UnknownMedia'
ALTER TABLE [dbo].[BaseMediaSet_UnknownMedia]
ADD CONSTRAINT [FK_UnknownMedia_inherits_BaseMedia]
    FOREIGN KEY ([Id])
    REFERENCES [dbo].[BaseMediaSet]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------