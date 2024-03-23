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

    CREATE TABLE #temp(AlbumId INT PRIMARY KEY, ViewsByTopDay INT, ViewsByTopWeek INT, ViewsByTopMonth INT, ViewsByTopYear INT);

    INSERT INTO #temp(AlbumId, ViewsByTopDay, ViewsByTopWeek, ViewsByTopMonth, ViewsByTopYear)
    SELECT
      a.Id AS [AlbumId],
      SUM(IIF(cv.Date >= @startDate AND cv.Date < @endDate, cv.[View], 0)) AS [ViewsByTopDay],
      SUM(IIF(cv.Date >= @startDateOfWeek and cv.Date < @endDate, cv.[View], 0)) as [ViewsByTopWeek],
      SUM(IIF(cv.Date >= @startDateOfMonth and cv.Date < @endDate, cv.[View], 0)) as [ViewsByTopMonth],
      SUM(IIF(cv.Date >= @startDateOfYear and cv.Date < @endDate, cv.[View], 0)) as [ViewsByTopYear]
    FROM dbo.Album a
      JOIN dbo.Collection c ON c.AlbumId = a.Id
      JOIN dbo.CollectionView cv ON cv.CollectionId = c.Id
    WHERE EXISTS (
		  SELECT 1
		  FROM dbo.Collection c2
		  WHERE EXISTS (SELECT value FROM STRING_SPLIT(@collectionIds, ',') WHERE value = c2.Id) AND a.Id = c2.AlbumId
	  )
    GROUP BY a.Id

    UPDATE a
    SET
      [ViewsByTopDay] = t.ViewsByTopDay,
      [ViewsByTopWeek] = t.ViewsByTopWeek,
      [ViewsByTopMonth] = t.ViewsByTopMonth,
      [ViewsByTopYear] = t.ViewsByTopYear
    FROM dbo.Album a
      JOIN #temp t ON t.AlbumId = a.Id
END