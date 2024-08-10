CREATE OR ALTER PROCEDURE Album_RecalculateViewsTopType_All
AS
BEGIN
    DECLARE @startDate DATETIME = DATEADD(d, -1, GETUTCDATE());
    DECLARE @endDate DATETIME = GETUTCDATE();

    DECLARE @isFirstDayOfMonth BIT;
    DECLARE @isSecondDayOfMonth BIT;

    DECLARE @isFirstDayOfYear BIT;
    DECLARE @isSecondDayOfYear BIT;
    
    SET @isFirstDayOfMonth = CASE 
        WHEN DAY(GETUTCDATE()) = 1 THEN 1 
        ELSE 0 
    END

    SET @isSecondDayOfMonth = CASE 
        WHEN DAY(GETUTCDATE()) = 2 THEN 1 
        ELSE 0 
    END
    
    SET @isFirstDayOfYear = CASE 
        WHEN DATEPART(DAYOFYEAR, GETUTCDATE()) = 1 THEN 1 
        ELSE 0 
    END

    SET @isSecondDayOfYear = CASE 
        WHEN DATEPART(DAYOFYEAR, GETUTCDATE()) = 2 THEN 1 
        ELSE 0 
    END

    Update Album
    SET [ViewsByTopMonth] = CASE
                              WHEN @isFirstDayOfMonth = 1 OR @isSecondDayOfMonth = 1 THEN ViewsByTopMonth
                              ELSE ViewsByTopMonth + ViewsByTopWeek
                            END,
        [ViewsByTopYear] = CASE
                            WHEN @isFirstDayOfYear = 1 THEN ViewsByTopYear
                            WHEN @isSecondDayOfYear = 1 THEN ViewsByTopWeek
                            ELSE ViewsByTopYear + ViewsByTopWeek
                          END

    -- Clean snapshot of previous day
    -- Update Album
    -- SET [ViewsByTopWeek] = 0
END