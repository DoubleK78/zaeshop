CREATE OR ALTER PROCEDURE Album_RecalculateViewsTopType (
    @collectionIds VARCHAR(MAX)
)
AS
BEGIN
    DECLARE @startDate DATETIME = DATEADD(d, -1, GETUTCDATE());
    DECLARE @endDate DATETIME = GETUTCDATE();

    DECLARE @startDateOfMonth DATETIME = DATEADD(mm, DATEDIFF(mm, 0, GETUTCDATE()), 0);

    DECLARE @CollectionIdTable TABLE (CollectionId INT PRIMARY KEY CLUSTERED);

    INSERT INTO @CollectionIdTable (CollectionId)
    SELECT value
    FROM STRING_SPLIT(@collectionIds, ',');

    CREATE TABLE #temp (AlbumId INT PRIMARY KEY, ViewsByTopDay INT, ViewsByTopMonth INT);

    ;WITH AlbumIds AS (
        SELECT DISTINCT a.Id AS AlbumId
        FROM dbo.Album a
        WHERE EXISTS (
            SELECT 1
            FROM dbo.Collection c
            WHERE c.AlbumId = a.Id
            AND c.Id IN (SELECT CollectionId FROM @CollectionIdTable)
        )
    )
    INSERT INTO #temp (AlbumId, ViewsByTopDay, ViewsByTopMonth)
    SELECT 
        a.AlbumId,
        SUM(CASE WHEN cv.Date >= @startDate THEN cv.[View] ELSE 0 END) AS ViewsByTopDay,
        SUM(CASE WHEN cv.Date >= @startDateOfMonth THEN cv.[View] ELSE 0 END) AS ViewsByTopMonth
    FROM AlbumIds a
    JOIN dbo.Collection c ON c.AlbumId = a.AlbumId
    JOIN dbo.CollectionView cv ON cv.CollectionId = c.Id
    WHERE cv.Date >= @startDateOfMonth AND cv.Date < @endDate
    GROUP BY a.AlbumId;

    UPDATE a
    SET 
        ViewsByTopDay = t.ViewsByTopDay,
        ViewsByTopMonth = t.ViewsByTopMonth
    FROM dbo.Album a
    JOIN #temp t ON a.Id = t.AlbumId;

    DROP TABLE #temp;
END;