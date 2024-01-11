CREATE OR ALTER PROCEDURE Album_All_Paging
    @pageNumber INT,
    @pageSize INT,
	@searchTerm NVARCHAR(MAX) = null,
	@sortColumn VARCHAR(100) = null,
	@sortDirection varchar(4) = 'ASC'
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

    WITH FilteredData
    AS (
		SELECT ROW_NUMBER() OVER (ORDER BY
			CASE WHEN ISNULL(@sortColumn, '') = '' THEN a.Id END,
			CASE WHEN @sortColumn = 'Title' AND @sortDirection = 'ASC' THEN a.Title END,
			CASE WHEN @sortColumn = 'Title' AND @sortDirection = 'DESC' THEN a.Title END DESC,
			CASE WHEN @sortColumn = 'Description' AND @sortDirection = 'ASC' THEN a.Description END,
			CASE WHEN @sortColumn = 'Description' AND @sortDirection = 'DESC' THEN a.Description END DESC,
			CASE WHEN @sortColumn = 'IsPublic' AND @sortDirection = 'ASC' THEN a.IsPublic END,
			CASE WHEN @sortColumn = 'IsPublic' AND @sortDirection = 'DESC' THEN a.IsPublic END DESC,
			CASE WHEN @sortColumn = 'CreatedOnUtc' AND @sortDirection = 'ASC' THEN a.CreatedOnUtc END,
			CASE WHEN @sortColumn = 'CreatedOnUtc' AND @sortDirection = 'DESC' THEN a.CreatedOnUtc END DESC,
			CASE WHEN @sortColumn = 'UpdatedOnUtc' AND @sortDirection = 'ASC' THEN a.UpdatedOnUtc END,
			CASE WHEN @sortColumn = 'UpdatedOnUtc' AND @sortDirection = 'DESC' THEN a.UpdatedOnUtc END DESC
		) AS RowNum,
               a.Id,
			   a.Title,
			   a.Description,
			   a.AlbumAlertMessageId,
			   aam.Name AS [AlbumAlertMessageName],
			   aam.Description AS [AlbumAlertMessageDescription],
			   a.IsPublic,
			   STRING_AGG(ct.Id, ', ') WITHIN GROUP(ORDER BY ct.Name) AS [ContentTypeIds],
               STRING_AGG(ct.Name, ', ') WITHIN GROUP(ORDER BY ct.Name) AS [ContentTypes],
			   a.CreatedOnUtc,
			   a.UpdatedOnUtc,
			   a.CdnThumbnailUrl
        FROM dbo.Album a
			LEFT JOIN dbo.AlbumAlertMessage aam ON aam.Id = a.AlbumAlertMessageId
			LEFT JOIN dbo.AlbumContentType act ON act.AlbumId = a.Id
			LEFT JOIN dbo.ContentType ct ON ct.Id = act.ContentTypeId
		WHERE (ISNULL(@searchTerm, '') = '' OR 
			(a.Title LIKE '' + @searchTerm + '%') OR
			(a.Description LIKE '' + @searchTerm + '%')
		)
        GROUP BY a.Id,
			   a.Title,
			   a.Description,
			   a.AlbumAlertMessageId,
			   aam.Name,
			   aam.Description,
			   a.IsPublic,
			   a.CreatedOnUtc,
			   a.UpdatedOnUtc,
			   a.CdnThumbnailUrl
	)
    SELECT COUNT_BIG(1) AS RowNum,
		 0 Id,
		 NULL Title,
		 NULL Description,
		 0 AlbumAlertMessageId,
		 NULL [AlbumAlertMessageName],
		 NULL [AlbumAlertMessageDescription],
		 0 IsPublic,
		 NULL [ContentTypeIds],
		 NULL [ContentTypes],
		 GETDATE() CreatedOnUtc,
		 NULL UpdatedOnUtc,
		 null [CdnThumbnailUrl],
		1 AS IsTotalRecord
    FROM FilteredData
    UNION
    SELECT *,
           0 AS IsTotalRecord
    FROM FilteredData
	WHERE FilteredData.RowNum
		BETWEEN @offset + 1 AND @offset + @pageSize
END