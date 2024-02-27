CREATE OR ALTER PROCEDURE Collection_Content_GetByFriendlyName
(
    @comicFriendlyName NVARCHAR(MAX),
    @contentFriendlyName NVARCHAR(MAX)
)
AS
BEGIN
    select
        c.Id,
        c.Title,
        c.FriendlyName,
        c.CreatedOnUtc,
        c.UpdatedOnUtc,
        c.IsPublic,
        a.Id as [AlbumId],
        a.Title as [AlbumTitle],
        a.FriendlyName as [AlbumFriendlyName],
        c.Description,
        c.ExtendName,
        c.Volume,
        c.Views,
        c.LevelPublic,
        a.LevelPublic as [AlbumLevelPublic],
        a.Region,
        c.StorageType
    from dbo.Collection c
        join dbo.Album a on c.AlbumId = a.Id
    where a.FriendlyName = @comicFriendlyName and c.FriendlyName = @contentFriendlyName
END