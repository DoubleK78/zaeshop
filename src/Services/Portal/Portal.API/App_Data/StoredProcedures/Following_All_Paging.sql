CREATE OR ALTER PROCEDURE Following_All_Paging
    @pageNumber INT,
    @pageSize INT,
    @searchTerm NVARCHAR(MAX) = NULL,
    @sortColumn VARCHAR(100) = NULL,
    @sortDirection VARCHAR(4) = 'ASC',
    @userId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate parameters
    IF @pageNumber <= 0
        SET @pageNumber = 1;

    -- Default page size
    IF @pageSize <= 0
        SET @pageSize = 10;

    DECLARE @offset INT = (@pageNumber - 1) * @pageSize;

    WITH FilteredData AS
    (
        SELECT ROW_NUMBER() OVER (ORDER BY
            CASE WHEN ISNULL(@sortColumn, '') = '' THEN f.Id END DESC,
            CASE WHEN @sortColumn = 'CreatedOnUtc' AND @sortDirection = 'ASC' THEN f.CreatedOnUtc END,
            CASE WHEN @sortColumn = 'CreatedOnUtc' AND @sortDirection = 'DESC' THEN f.CreatedOnUtc END DESC,
            CASE WHEN @sortColumn = 'UpdatedOnUtc' AND @sortDirection = 'ASC' THEN a.UpdatedOnUtc END,
            CASE WHEN @sortColumn = 'UpdatedOnUtc' AND @sortDirection = 'DESC' THEN a.UpdatedOnUtc END DESC
        ) AS RowNum,
        f.Id,
        f.UserId,
        f.AlbumId,
        a.Title,
        c.Title AS LastCollectionTitle,
        a.Views,
        f.CreatedOnUtc,
        a.CdnThumbnailUrl,
        a.FriendlyName,
		a.UpdatedOnUtc
        FROM dbo.Following f
        LEFT JOIN dbo.Album a ON a.Id = f.AlbumId
        OUTER APPLY (
            SELECT TOP 1 c.Title
            FROM dbo.Collection c
            WHERE c.AlbumId = f.AlbumId
            ORDER BY c.CreatedOnUtc DESC
        ) c
        WHERE ISNULL(@userId, '') = '' OR f.UserId = @userId
        GROUP BY
            f.Id,
            f.UserId,
            f.AlbumId,
            a.Title,
            c.Title,
            a.Views,
            f.CreatedOnUtc,
            a.CdnThumbnailUrl,
            a.FriendlyName,
			a.UpdatedOnUtc
    )

    SELECT 
        COUNT_BIG(1) AS RowNum,
        0 AS Id,
        0 AS UserId,
        0 AS AlbumId,
        NULL AS Title,
        NULL AS LastCollectionTitle,
        0 AS Views,
        GETDATE() AS CreatedOnUtc,
        NULL AS CdnThumbnailUrl,
        NULL AS FriendlyName,
		NULL AS UpdatedOnUtc,
        1 AS IsTotalRecord
    FROM FilteredData
    UNION
    SELECT *,
        0 AS IsTotalRecord
    FROM FilteredData
    WHERE RowNum BETWEEN @offset + 1 AND @offset + @pageSize
END
