CREATE OR ALTER PROCEDURE Album_All_Paging
    @pageNumber INT,
    @pageSize INT,
	@searchTerm NVARCHAR(MAX) = null,
	@sortColumn VARCHAR(100) = null,
	@sortDirection varchar(4) = 'ASC',
	@firstChar VARCHAR(100) = null,
	@language VARCHAR(20) = null,
	@country VARCHAR(100) = null,
	@genre VARCHAR(100) = null,
	@status BIT = 0,
	@year VARCHAR(100) = null,
	@topType VARCHAR(100) = null,
	@region VARCHAR(100) = null
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

	/* Genere Filter */
	CREATE TABLE #genereAlbums(AlbumId INT)
	IF ISNULL(@genre, '') <> ''
	BEGIN
		INSERT INTO #genereAlbums
		SELECT a.Id AS AlbumId
		FROM dbo.Album a
		WHERE EXISTS (
		    SELECT 1
		    FROM (
		        SELECT act.AlbumId, STRING_AGG(CAST(act.ContentTypeId AS VARCHAR(10)), ',') AS ContentTypeIds
		        FROM dbo.AlbumContentType act
		        GROUP BY act.AlbumId
		    ) AS act_agg
		    CROSS JOIN STRING_SPLIT(CASE WHEN ',' + @genre + ',' LIKE '%,%' THEN @genre ELSE ',' + @genre + ',' END, ',') s
		    WHERE act_agg.AlbumId = a.Id
		    AND ',' + act_agg.ContentTypeIds + ',' LIKE '%,' + CAST(TRIM(s.value) AS VARCHAR(10)) + ',%'
		    GROUP BY act_agg.AlbumId
		    HAVING COUNT(DISTINCT CAST(TRIM(s.value) AS INT)) = (
		        SELECT COUNT(DISTINCT CAST(TRIM(value) AS INT))
		        FROM STRING_SPLIT(CASE WHEN ',' + @genre + ',' LIKE '%,%' THEN @genre ELSE ',' + @genre + ',' END, ',')
		    )
		)
	END

    ;WITH FilteredData
    AS (
		SELECT ROW_NUMBER() OVER (ORDER BY
			CASE WHEN ISNULL(@sortColumn, '') = '' THEN a.Id END,
			CASE WHEN @sortColumn = 'Views' AND @sortDirection = 'DESC' THEN a.Views END DESC,
			CASE WHEN @sortColumn = 'UpdatedOnUtc' AND @sortDirection = 'DESC' THEN a.UpdatedOnUtc END DESC,
			CASE WHEN @sortColumn = 'Title' AND @sortDirection = 'ASC' THEN a.Title END,			
			CASE WHEN @topType = 'day' AND @sortColumn = 'ViewByTopType' AND @sortDirection = 'DESC' THEN a.ViewsByTopDay END DESC,		
			CASE WHEN @topType = 'week' AND @sortColumn = 'ViewByTopType' AND @sortDirection = 'DESC' THEN a.ViewsByTopWeek END DESC,
			CASE WHEN @topType = 'month' AND @sortColumn = 'ViewByTopType' AND @sortDirection = 'DESC' THEN a.ViewsByTopMonth END DESC,
			CASE WHEN @topType = 'year' AND @sortColumn = 'ViewByTopType' AND @sortDirection = 'DESC' THEN a.ViewsByTopYear END DESC,
			CASE WHEN @sortColumn = 'CreatedOnUtc' AND @sortDirection = 'DESC' THEN a.CreatedOnUtc END DESC,
			CASE WHEN @sortColumn = 'Views' AND @sortDirection = 'ASC' THEN a.Views END,
			CASE WHEN @sortColumn = 'UpdatedOnUtc' AND @sortDirection = 'ASC' THEN a.UpdatedOnUtc END,
			CASE WHEN @sortColumn = 'CreatedOnUtc' AND @sortDirection = 'ASC' THEN a.CreatedOnUtc END,
			CASE WHEN @topType = 'day' AND @sortColumn = 'ViewByTopType' AND @sortDirection = 'ASC' THEN a.ViewsByTopDay END,
			CASE WHEN @topType = 'week' AND @sortColumn = 'ViewByTopType' AND @sortDirection = 'ASC' THEN a.ViewsByTopWeek END,
			CASE WHEN @topType = 'month' AND @sortColumn = 'ViewByTopType' AND @sortDirection = 'ASC' THEN a.ViewsByTopMonth END,
			CASE WHEN @topType = 'year' AND @sortColumn = 'ViewByTopType' AND @sortDirection = 'ASC' THEN a.ViewsByTopYear END,
			CASE WHEN @sortColumn = 'Title' AND @sortDirection = 'DESC' THEN a.Title END DESC,
			CASE WHEN @sortColumn = 'IsPublic' AND @sortDirection = 'ASC' THEN a.IsPublic END,
			CASE WHEN @sortColumn = 'IsPublic' AND @sortDirection = 'DESC' THEN a.IsPublic END DESC
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
			   a.CdnThumbnailUrl,
			   a.CdnOriginalUrl,
			   a.FriendlyName,
			   a.Views,
			   CASE
			   	WHEN @topType = 'day' THEN a.ViewsByTopDay
			   	WHEN @topType = 'week' THEN a.ViewsByTopWeek
			   	WHEN @topType = 'month' THEN a.ViewsByTopMonth
			   	WHEN @topType = 'year' THEN a.ViewsByTopYear
				ELSE a.Views END AS ViewByTopType,
			   a.Tags,
			   a.Region
        FROM dbo.Album a
			LEFT JOIN #genereAlbums ga ON ga.AlbumId = a.Id
			LEFT JOIN dbo.AlbumAlertMessage aam ON aam.Id = a.AlbumAlertMessageId
			LEFT JOIN dbo.AlbumContentType act ON act.AlbumId = a.Id
			LEFT JOIN dbo.ContentType ct ON ct.Id = act.ContentTypeId
		WHERE (ISNULL(@searchTerm, '') = '' OR 
			(a.Title LIKE '%' + @searchTerm + '%') OR
			(a.Description LIKE '%' + @searchTerm + '%') OR
			(a.Tags LIKE '%' + @searchTerm + '%'))
			AND (ISNULL(@firstChar, '') = '' OR
			(a.FriendlyName LIKE @firstChar + '%'))
			AND (ISNULL(@genre, '') = '' OR ga.AlbumId IS NOT NULL)
			AND (ISNULL(@year, '') = '' OR (a.ReleaseYear LIKE @year + '%'))
			AND (ISNULL(@status, '') = '' OR (a.AlbumStatus = @status))
			AND a.Region = @region
			AND a.IsPublic = 1
        GROUP BY a.Id,
			   a.Title,
			   a.Description,
			   a.AlbumAlertMessageId,
			   aam.Name,
			   aam.Description,
			   a.IsPublic,
			   a.CreatedOnUtc,
			   a.UpdatedOnUtc,
			   a.CdnThumbnailUrl,
			   a.CdnOriginalUrl,
			   a.FriendlyName,
			   a.Views,
			   a.Tags,
			   a.Region,
			   a.ViewsByTopDay,
			   a.ViewsByTopWeek,
			   a.ViewsByTopMonth,
			   a.ViewsByTopYear
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
		 null [CdnOriginalUrl],
		 NULL FriendlyName,
		 NULL Views,
		 0 ViewByTopType,
		 NULL Tags,
		 0 [Region],
		NULL LastCollectionTitle,
		 1 AS IsTotalRecord
    FROM FilteredData
    UNION
    SELECT *,
           0 AS IsTotalRecord
    FROM FilteredData
	OUTER APPLY (
        SELECT TOP 1 c.Title
        FROM dbo.Collection c
        WHERE c.AlbumId = FilteredData.Id
        ORDER BY c.Id DESC
    ) c
	WHERE FilteredData.RowNum
		BETWEEN @offset + 1 AND @offset + @pageSize
	
	DROP TABLE #genereAlbums
END