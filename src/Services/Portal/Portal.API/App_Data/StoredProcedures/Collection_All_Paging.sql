CREATE OR ALTER PROCEDURE Collection_All_Paging
    @pageNumber INT,
    @pageSize INT,
    @albumId INT,
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
			CASE WHEN @sortColumn = 'Title' AND @sortDirection = 'ASC' THEN c.Title END,
			CASE WHEN @sortColumn = 'Title' AND @sortDirection = 'DESC' THEN c.Title END DESC,
			CASE WHEN @sortColumn = 'Description' AND @sortDirection = 'ASC' THEN c.Description END,
			CASE WHEN @sortColumn = 'Description' AND @sortDirection = 'DESC' THEN c.Description END DESC,
			CASE WHEN @sortColumn = 'IsPublic' AND @sortDirection = 'ASC' THEN c.IsPublic END,
			CASE WHEN @sortColumn = 'IsPublic' AND @sortDirection = 'DESC' THEN c.IsPublic END DESC,
			CASE WHEN @sortColumn = 'CreatedOnUtc' AND @sortDirection = 'ASC' THEN c.CreatedOnUtc END,
			CASE WHEN @sortColumn = 'CreatedOnUtc' AND @sortDirection = 'DESC' THEN c.CreatedOnUtc END DESC,
			CASE WHEN @sortColumn = 'UpdatedOnUtc' AND @sortDirection = 'ASC' THEN c.UpdatedOnUtc END,
			CASE WHEN @sortColumn = 'UpdatedOnUtc' AND @sortDirection = 'DESC' THEN c.UpdatedOnUtc END DESC,
			CASE WHEN @sortColumn = 'Views' AND @sortDirection = 'ASC' THEN c.Views END,
			CASE WHEN @sortColumn = 'Views' AND @sortDirection = 'DESC' THEN c.Views END DESC,
			CASE WHEN @sortColumn = 'LevelPublic' AND @sortDirection = 'ASC' THEN c.LevelPublic END,
			CASE WHEN @sortColumn = 'LevelPublic' AND @sortDirection = 'DESC' THEN c.LevelPublic END DESC
		) AS RowNum,
               c.Id,
			   c.Title,
			   c.Description,
			   c.AlbumId,
			   a.Title AS [AlbumTitle],
			   c.IsPublic,
			   c.Volume,
			   c.ExtendName,
			   c.CreatedOnUtc,
			   c.UpdatedOnUtc,
			   c.Views,
			   c.LevelPublic,
			   c.StorageType
        FROM dbo.Collection c
			JOIN dbo.Album a ON a.Id = c.AlbumId
		WHERE a.Id = @albumId AND 
		(ISNULL(@searchTerm, '') = '' OR 
			(c.Title LIKE '%' + @searchTerm + '%') OR
			(c.Description LIKE '' + @searchTerm + '%') OR
			(c.ExtendName LIKE '' + @searchTerm + '%')
		)
	)
    SELECT COUNT_BIG(1) AS RowNum,
		 0 Id,
		 NULL Title,
		 NULL Description,
		 0 AlbumId,
		 NULL [AlbumTitle],
		 0 IsPublic,
		 0 Volume,
		 NULL ExtendName,
		 GETDATE() CreatedOnUtc,
		 NULL UpdatedOnUtc,
		 0 Views,
		 0 LevelPublic,
		 0 StorageType,
		1 AS IsTotalRecord
    FROM FilteredData
    UNION
    SELECT *,
           0 AS IsTotalRecord
    FROM FilteredData
	WHERE FilteredData.RowNum
		BETWEEN @offset + 1 AND @offset + @pageSize
END