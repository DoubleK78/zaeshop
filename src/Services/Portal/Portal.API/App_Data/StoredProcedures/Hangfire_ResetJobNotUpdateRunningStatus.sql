CREATE OR ALTER PROCEDURE Hangfire_ResetJobNotUpdateRunningStatus
AS
BEGIN
    UPDATE dbo.HangfireScheduleJob
	SET IsRunning = 0
    Where IsRunning = 1 AND ABS(DATEDIFF(MINUTE, StartOnUtc, EndOnUtc)) >= 5;
END