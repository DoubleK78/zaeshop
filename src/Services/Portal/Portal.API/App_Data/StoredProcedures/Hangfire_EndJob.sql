CREATE OR ALTER PROCEDURE Hangfire_EndJob (
    @id INT
)
AS
BEGIN
	UPDATE dbo.HangfireScheduleJob
	SET IsRunning = 0, EndOnUtc = GETUTCDATE()
    WHERE Id = @id AND IsEnabled = 1
END