CREATE OR ALTER PROCEDURE Album_RecalculateViewsTopType (
    @collectionIds NVARCHAR(MAX)
)
AS
BEGIN
    DECLARE @startDate DATETIME = DATEADD(d, -1, GETUTCDATE());
    DECLARE @endDate DATETIME = GETUTCDATE();

    DECLARE @startDateOfWeek DATETIME = DATEADD(wk, DATEDIFF(wk, 0, GETUTCDATE()), 0);
    DECLARE @startDateOfMonth DATETIME = DATEADD(mm, DATEDIFF(mm, 0, @startDateOfWeek), 0);
    DECLARE @startDateOfYear DATETIME = DATEADD(yy, DATEDIFF(yy, 0, @startDateOfMonth), 0);

    DECLARE @CollectionIdTable TABLE (CollectionId INT);

    INSERT INTO @CollectionIdTable (CollectionId)
    SELECT CAST(value AS INT)
    FROM STRING_SPLIT(@collectionIds, ',');

    CREATE TABLE #temp (AlbumId INT PRIMARY KEY, ViewsByTopDay INT, ViewsByTopWeek INT, ViewsByTopMonth INT, ViewsByTopYear INT);

    -- Select albums that have at least one collection in the specified @collectionIds
    WITH AlbumIds AS (
        SELECT DISTINCT a.Id AS AlbumId
        FROM dbo.Album a
        WHERE EXISTS (
            SELECT 1
            FROM dbo.Collection c
            WHERE c.AlbumId = a.Id
            AND EXISTS (
                SELECT 1
                FROM @CollectionIdTable ct
                WHERE ct.CollectionId = c.Id
            )
        )
    )
    
    INSERT INTO #temp (AlbumId, ViewsByTopDay, ViewsByTopWeek, ViewsByTopMonth, ViewsByTopYear)
    SELECT 
        a.AlbumId,
        SUM(CASE WHEN cv.Date >= @startDate AND cv.Date < @endDate THEN cv.[View] ELSE 0 END) AS ViewsByTopDay,
        SUM(CASE WHEN cv.Date >= @startDateOfWeek AND cv.Date < @endDate THEN cv.[View] ELSE 0 END) AS ViewsByTopWeek,
        SUM(CASE WHEN cv.Date >= @startDateOfMonth AND cv.Date < @endDate THEN cv.[View] ELSE 0 END) AS ViewsByTopMonth,
        SUM(CASE WHEN cv.Date >= @startDateOfYear AND cv.Date < @endDate THEN cv.[View] ELSE 0 END) AS ViewsByTopYear
    FROM AlbumIds a
    JOIN dbo.Collection c ON c.AlbumId = a.AlbumId
    JOIN dbo.CollectionView cv ON cv.CollectionId = c.Id
    GROUP BY a.AlbumId;

    UPDATE a
    SET 
        ViewsByTopDay = t.ViewsByTopDay,
        ViewsByTopWeek = t.ViewsByTopWeek,
        ViewsByTopMonth = t.ViewsByTopMonth,
        ViewsByTopYear = t.ViewsByTopYear
    FROM dbo.Album a
    JOIN #temp t ON a.Id = t.AlbumId;
END;
