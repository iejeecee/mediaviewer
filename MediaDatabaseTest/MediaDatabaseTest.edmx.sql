
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, and Azure
-- --------------------------------------------------
-- Date Created: 12/10/2013 18:29:14
-- Generated from EDMX file: D:\Repos\mediaviewer\MediaDatabaseTest\MediaDatabaseTest.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [MediaDatabaseTest.Program+DummyContext];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_LinkedTags]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TagSet] DROP CONSTRAINT [FK_LinkedTags];
GO
IF OBJECT_ID(N'[dbo].[FK_TagCategoryTag]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TagSet] DROP CONSTRAINT [FK_TagCategoryTag];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[TagCategorySet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TagCategorySet];
GO
IF OBJECT_ID(N'[dbo].[TagSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TagSet];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'TagSet'
CREATE TABLE [dbo].[TagSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [TagCategoryId] int  NULL,
    [TagId] int  NULL
);
GO

-- Creating table 'TagCategorySet'
CREATE TABLE [dbo].[TagCategorySet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL
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

-- Creating foreign key on [TagId] in table 'TagSet'
ALTER TABLE [dbo].[TagSet]
ADD CONSTRAINT [FK_LinkedTags]
    FOREIGN KEY ([TagId])
    REFERENCES [dbo].[TagSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_LinkedTags'
CREATE INDEX [IX_FK_LinkedTags]
ON [dbo].[TagSet]
    ([TagId]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------