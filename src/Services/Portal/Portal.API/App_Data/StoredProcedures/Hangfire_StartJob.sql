CREATE OR ALTER PROCEDURE Hangfire_StartJob (
    @id INT
)
AS
BEGIN
	UPDATE dbo.HangfireScheduleJob
	SET IsRunning = 1, StartOnUtc = GETUTCDATE()
    WHERE Id = @id AND IsEnabled = 1
END