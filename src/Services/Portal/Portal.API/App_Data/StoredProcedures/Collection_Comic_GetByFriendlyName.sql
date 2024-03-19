CREATE OR ALTER PROCEDURE Collection_Comic_GetByFriendlyName
(
    @friendlyName VARCHAR(350)
)
AS
BEGIN
    select
        a.Id,
        a.Title,
        a.FriendlyName,
        a.Description,
        am.Name as [AlbumAlertMessageName],
        a.IsPublic,
        a.CreatedOnUtc,
        a.UpdatedOnUtc,
        a.AlternativeName,
        a.Type,
        a.AlbumStatus,
        a.ReleaseYear,
        a.AuthorNames,
        a.ArtitstNames,
        a.Tags,
        a.CdnThumbnailUrl as [ThumbnailUrl],
        a.Views,
        a.LevelPublic,
        a.Region,
        STRING_AGG(ct.Name, ', ') WITHIN GROUP (ORDER BY ct.Name) as ContentTypeNames
    from dbo.Album a
        left join dbo.AlbumAlertMessage am on am.Id = a.AlbumAlertMessageId
        left join dbo.AlbumContentType act on act.AlbumId = a.Id
        left join dbo.ContentType ct on ct.Id = act.ContentTypeId
    where a.FriendlyName = @friendlyName
    group by a.Id,
    a.Title,
    a.FriendlyName,
    a.Description,
    am.Name,
    a.IsPublic,
    a.CreatedOnUtc,
    a.UpdatedOnUtc,
    a.AlternativeName,
    a.Type,
    a.AlbumStatus,
    a.ReleaseYear,
    a.AuthorNames,
    a.ArtitstNames,
    a.Tags,
    a.CdnThumbnailUrl,
    a.Views,
    a.LevelPublic,
    a.Region
END