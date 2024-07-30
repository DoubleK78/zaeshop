CREATE OR ALTER PROCEDURE Comic_Metadata (
    @comicFriendlyName varchar(350)
)
AS
BEGIN
    SELECT
        c.Id,
        c.AlbumId,
        c.Title,
        dbo.getNumberByText(c.Title) AS ChapterNumber
    INTO #collectionTmp
    FROM dbo.[Collection] c
        JOIN dbo.Album a ON c.AlbumId = a.Id
    WHERE a.FriendlyName = @comicFriendlyName

    SELECT
        a.Id,
        a.Title,
        a.FriendlyName,
        (select top 1
            Title
        from #collectionTmp
        order by ChapterNumber desc ) as LastestChapter,
        a.CdnThumbnailUrl as [ComicImageUrl],
		a.Region
    FROM dbo.Album a
    WHERE a.FriendlyName = @comicFriendlyName
    GROUP BY
        a.Id,
        a.Title,
        a.FriendlyName,
		a.CdnThumbnailUrl,
		a.Region

    drop table #collectionTmp
END