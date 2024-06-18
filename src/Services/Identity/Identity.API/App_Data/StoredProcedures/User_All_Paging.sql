CREATE OR ALTER PROCEDURE User_All_Paging
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
			CASE WHEN @sortColumn = 'CreatedOnUtc' AND @sortDirection = 'DESC' THEN u.CreatedOnUtc END DESC,
			CASE WHEN ISNULL(@sortColumn, '') = '' THEN u.Id END,		
			CASE WHEN @sortColumn = 'CreatedOnUtc' AND @sortDirection = 'ASC' THEN u.CreatedOnUtc END,
			CASE WHEN @sortColumn = 'FullName' AND @sortDirection = 'ASC' THEN u.FullName END,
			CASE WHEN @sortColumn = 'FullName' AND @sortDirection = 'DESC' THEN u.FullName END DESC,
			CASE WHEN @sortColumn = 'UserName' AND @sortDirection = 'ASC' THEN u.UserName END,
			CASE WHEN @sortColumn = 'UserName' AND @sortDirection = 'DESC' THEN u.UserName END DESC,
			CASE WHEN @sortColumn = 'Email' AND @sortDirection = 'ASC' THEN u.Email END,
			CASE WHEN @sortColumn = 'Email' AND @sortDirection = 'DESC' THEN u.Email END DESC,
			CASE WHEN @sortColumn = 'UpdatedOnUtc' AND @sortDirection = 'ASC' THEN u.UpdatedOnUtc END,
			CASE WHEN @sortColumn = 'UpdatedOnUtc' AND @sortDirection = 'DESC' THEN u.UpdatedOnUtc END DESC,
			CASE WHEN @sortColumn = 'IsBanned' AND @sortDirection = 'ASC' THEN u.IsBanned END,
			CASE WHEN @sortColumn = 'IsBanned' AND @sortDirection = 'DESC' THEN u.IsBanned END DESC
		) AS RowNum,
			u.Id,
			u.FullName,
			u.UserName,
			u.Email,
			u.EmailConfirmed,
			u.CreatedOnUtc,
			u.UpdatedOnUtc,
			STRING_AGG(r.Name, ', ') WITHIN GROUP (ORDER BY r.Name) AS [Roles],
			u.IsBanned
		FROM dbo.[User] u
		LEFT JOIN dbo.AspNetUserRoles ur ON u.Id = ur.UserId
		LEFT JOIN dbo.AspNetRoles r ON ur.RoleId = r.Id
		WHERE (ISNULL(@searchTerm, '') = '' OR
			(u.Email LIKE '' + @searchTerm + '%') OR
			(u.UserName LIKE '' + @searchTerm + '%') OR
			(u.FullName LIKE '' + @searchTerm + '%')		
		)
		GROUP BY u.Id,
				 u.FullName,
				 u.UserName,
				 u.Email,
				 u.EmailConfirmed,
				 u.CreatedOnUtc,
				 u.UpdatedOnUtc,
				 u.IsBanned
	)
	SELECT
		COUNT_BIG(1) AS RowNum,
		NULL AS Id,
		NULL AS FullName,
		NULL AS UserName,
		NULL AS Email,
		NULL AS EmailConfirmed,
		NULL AS CreatedOnUtc,
		NULL AS UpdatedOnUtc,
		NULL AS [Roles],
		0 AS [IsBanned],
		1 as IsTotalRecord
	FROM FilteredData
	UNION
    SELECT *,
           0 AS IsTotalRecord
    FROM FilteredData
	WHERE FilteredData.RowNum
		BETWEEN @offset + 1 AND @offset + @pageSize
END