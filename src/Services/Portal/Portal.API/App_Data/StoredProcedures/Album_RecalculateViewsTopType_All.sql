CREATE OR ALTER PROCEDURE Album_RecalculateViewsTopType_All
AS
BEGIN
    DECLARE @startDate DATETIME = DATEADD(d, -1, GETUTCDATE());
    DECLARE @endDate DATETIME = GETUTCDATE();

    DECLARE @isFirstDayOfMonth BIT;
    DECLARE @isFirstDayOfYear BIT;
    
    SET @isFirstDayOfMonth = CASE 
        WHEN DAY(GETUTCDATE()) = 1 THEN 1 
        ELSE 0 
    END
    
    SET @isFirstDayOfYear = CASE 
        WHEN DATEPART(DAYOFYEAR, GETUTCDATE()) = 1 THEN 1 
        ELSE 0 
    END

    CREATE TABLE #temp(AlbumId INT PRIMARY KEY, ViewsByTopDay INT, ViewsByTopWeek INT);

    INSERT INTO #temp(AlbumId, ViewsByTopWeek, ViewsByTopDay)
    SELECT
      a.Id AS [AlbumId],
      a.ViewsByTopWeek,
      SUM(cv.[View]) AS [ViewsByTopDay]
    FROM dbo.Album a
      JOIN dbo.Collection c ON c.AlbumId = a.Id
      JOIN dbo.CollectionView cv ON cv.CollectionId = c.Id
    WHERE cv.Date >= @startDate AND cv.Date < @endDate
    GROUP BY a.Id, a.ViewsByTopWeek

    -- ViewsByTopWeek is snapshot of previous day
    UPDATE a
    SET
      [ViewsByTopDay] = t.ViewsByTopDay,
      [ViewsByTopMonth] = IIF(@isFirstDayOfMonth = 1, t.ViewsByTopDay, a.ViewsByTopMonth + t.ViewsByTopWeek),
      [ViewsByTopYear] = IIF(@isFirstDayOfYear = 1, t.ViewsByTopDay, a.ViewsByTopYear +  t.ViewsByTopWeek) 
    FROM dbo.Album a
      JOIN #temp t ON t.AlbumId = a.Id

    -- Clean snapshot of previous day
    Update Album
    SET [ViewsByTopWeek] = 0

    DROP TABLE #temp
END