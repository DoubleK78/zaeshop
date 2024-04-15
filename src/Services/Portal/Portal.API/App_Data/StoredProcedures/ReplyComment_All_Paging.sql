CREATE OR ALTER PROCEDURE ReplyComment_All_Paging
    @pageNumber INT,
    @pageSize INT,
    @searchTerm NVARCHAR(MAX) = null,
    @sortColumn VARCHAR(100) = null,
    @sortDirection varchar(4) = 'ASC',
    @userId INT,
    @commentId INT
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
			CASE WHEN ISNULL(@sortColumn, '') = '' THEN rc.Id END DESC,
			CASE WHEN @sortColumn = 'CreatedOnUtc' AND @sortDirection = 'ASC' THEN rc.CreatedOnUtc END,
			CASE WHEN @sortColumn = 'CreatedOnUtc' AND @sortDirection = 'DESC' THEN rc.CreatedOnUtc END DESC,
			CASE WHEN @sortColumn = 'UpdatedOnUtc' AND @sortDirection = 'ASC' THEN rc.UpdatedOnUtc END,
			CASE WHEN @sortColumn = 'UpdatedOnUtc' AND @sortDirection = 'DESC' THEN rc.UpdatedOnUtc END DESC
		) AS RowNum,
                rc.Id,
                rc.Text,
                c.AlbumId,
                c.CollectionId,
                rc.UserId,
                u.FullName,
                u.UserName,
                rc.CreatedOnUtc,
                rc.UpdatedOnUtc,
                u.Avatar,
                c2.Title,
                a.FriendlyName as AlbumFriendlyName,
                c2.FriendlyName as FriendlyName,
                u.LevelId,
                u.CurrentExp,
                u.NextLevelExp,
                u.RoleType
            FROM dbo.ReplyComment rc
                JOIN dbo.[User] u ON u.Id = rc.UserId
                JOIN Comment c ON rc.CommentId = c.Id
                LEFT JOIN dbo.Album a ON a.Id = c.AlbumId
                LEFT JOIN dbo.Collection c2 ON c2.Id = c.CollectionId
            WHERE c.IsDeleted = 0 AND
                (ISNULL(@userId, '') = '' OR c.UserId = @userId) AND rc.CommentId = @commentId
            GROUP BY
				        rc.Id,
                rc.Text,
                c.AlbumId,
                c.CollectionId,
                rc.UserId,
                u.FullName,
                u.UserName,
                rc.CreatedOnUtc,
                rc.UpdatedOnUtc,
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
            1 AS IsTotalRecord
        FROM FilteredData
    UNION
        SELECT *,
            0 AS IsTotalRecord
        FROM FilteredData
        WHERE FilteredData.RowNum
		BETWEEN @offset + 1 AND @offset + @pageSize
END