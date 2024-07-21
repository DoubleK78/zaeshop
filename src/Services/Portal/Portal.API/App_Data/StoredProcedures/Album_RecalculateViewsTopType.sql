CREATE OR ALTER PROCEDURE Album_RecalculateViewsTopType (
    @collectionIds VARCHAR(MAX)
)
AS
BEGIN
    DECLARE @startDate DATETIME = DATEADD(d, -1, GETUTCDATE());
    DECLARE @endDate DATETIME = GETUTCDATE();

    DECLARE @startDateOfMonth DATETIME = DATEADD(mm, DATEDIFF(mm, 0, GETUTCDATE()), 0);
    DECLARE @isInFirstTenMinutes BIT;
    DECLARE @isFirstDayOfMonth BIT;

    SET @isInFirstTenMinutes = CASE 
        WHEN CONVERT(TIME, GETUTCDATE()) >= '00:00:00' AND CONVERT(TIME, GETUTCDATE()) < '00:10:00' THEN 1
        ELSE 0
    END

    SET @isFirstDayOfMonth = CASE 
        WHEN DAY(GETUTCDATE()) = 1 THEN 1 
        ELSE 0 
    END

    IF @isInFirstTenMinutes = 1
    BEGIN
        UPDATE Album
        SET [ViewsByTopWeek] = [ViewsByTopDay]
    END

    DECLARE @CollectionIdTable TABLE (CollectionId INT PRIMARY KEY CLUSTERED);

    INSERT INTO @CollectionIdTable (CollectionId)
    SELECT value
    FROM STRING_SPLIT(@collectionIds, ',');

    CREATE TABLE #temp (AlbumId INT PRIMARY KEY, ViewsByTopDay INT);

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
    INSERT INTO #temp (AlbumId, ViewsByTopDay)
    SELECT 
        a.AlbumId,
        SUM(cv.[View]) AS ViewsByTopDay
    FROM AlbumIds a
    JOIN dbo.Collection c ON c.AlbumId = a.AlbumId
    JOIN dbo.CollectionView cv ON cv.CollectionId = c.Id
    WHERE cv.Date >= @startDate AND cv.Date < @endDate
    GROUP BY a.AlbumId;

    UPDATE a
    SET 
        ViewsByTopDay = t.ViewsByTopDay,
        ViewsByTopMonth = IIF(@isFirstDayOfMonth = 1, t.ViewsByTopDay, a.ViewsByTopMonth)
    FROM dbo.Album a
    JOIN #temp t ON a.Id = t.AlbumId;

    DROP TABLE #temp;
END;