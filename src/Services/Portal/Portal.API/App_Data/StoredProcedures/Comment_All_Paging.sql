CREATE OR ALTER PROCEDURE Comment_All_Paging
    @pageNumber INT,
    @pageSize INT,
    @searchTerm NVARCHAR(MAX) = null,
    @sortColumn VARCHAR(100) = null,
    @sortDirection varchar(4) = 'ASC',
    @userId INT,
    @albumId INT,
    @collectionId INT
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
    WITH
        FilteredData
        AS
        (
            SELECT ROW_NUMBER() OVER (ORDER BY
			CASE WHEN ISNULL(@sortColumn, '') = '' THEN c.Id END DESC,
			CASE WHEN @sortColumn = 'CreatedOnUtc' AND @sortDirection = 'ASC' THEN c.CreatedOnUtc END,
			CASE WHEN @sortColumn = 'CreatedOnUtc' AND @sortDirection = 'DESC' THEN c.CreatedOnUtc END DESC,
			CASE WHEN @sortColumn = 'UpdatedOnUtc' AND @sortDirection = 'ASC' THEN c.UpdatedOnUtc END,
			CASE WHEN @sortColumn = 'UpdatedOnUtc' AND @sortDirection = 'DESC' THEN c.UpdatedOnUtc END DESC
		) AS RowNum,
                c.Id,
                c.Text,
                c.AlbumId,
                c.CollectionId,
                c.UserId,
                u.FullName,
                u.UserName,
                c.CreatedOnUtc,
                c.UpdatedOnUtc,
                u.Avatar,
                c2.Title,
                a.FriendlyName as AlbumFriendlyName,
                c2.FriendlyName as FriendlyName,
                u.LevelId,
                u.CurrentExp,
                u.NextLevelExp,
                u.RoleType,
				COUNT(rc.Id) AS ReplyCount
            FROM dbo.Comment c
                JOIN dbo.[User] u ON u.Id = c.UserId
                LEFT JOIN dbo.Album a ON a.Id = c.AlbumId
                LEFT JOIN dbo.Collection c2 ON c2.Id = c.CollectionId
				LEFT JOIN dbo.ReplyComment rc ON c.Id = rc.CommentId
            WHERE c.IsDeleted = 0 AND
                (ISNULL(@userId, '') = '' OR c.UserId = @userId) AND
                (ISNULL(@albumId, '') = '' OR a.Id = @albumId) AND
                (ISNULL(@collectionId, '') = '' OR c2.Id = @collectionId)
            GROUP BY
				c.Id,
                c.Text,
                c.AlbumId,
                c.CollectionId,
                c.UserId,
                u.FullName,
                u.UserName,
                c.CreatedOnUtc,
                c.UpdatedOnUtc,
                u.Avatar,
                c2.Title,
                a.FriendlyName,
                c2.FriendlyName,
                u.LevelId,
                u.CurrentExp,
                u.NextLevelExp,
                u.RoleType
        )
            SELECT COUNT_BIG(1) AS RowNum,
            0 Id,
            NULL [Text],
            0 AlbumId,
            0 CollectionId,
            0 UserId,
            NULL FullName,
            NULL [UserName],
            GETDATE() CreatedOnUtc,
            NULL UpdatedOnUtc,
            NULL Avatar,
            NULL Title,
            NULL AlbumFriendlyName,
            NULL FriendlyName,
            0 LevelId,
            0 CurrentExp,
            0 NextLevelExp,
            0 RoleType,
			0 ReplyCount,
            1 AS IsTotalRecord
        FROM FilteredData
    UNION
        SELECT *,
            0 AS IsTotalRecord
        FROM FilteredData
        WHERE FilteredData.RowNum
		BETWEEN @offset + 1 AND @offset + @pageSize
END