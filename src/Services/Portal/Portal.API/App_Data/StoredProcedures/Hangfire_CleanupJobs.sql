CREATE OR ALTER PROCEDURE Hangfire_CleanupJobs
AS
BEGIN
    SET NOCOUNT ON;

    -- Truncate tables
    TRUNCATE TABLE [HangFire].[AggregatedCounter];
    TRUNCATE TABLE [HangFire].[Counter];
    TRUNCATE TABLE [HangFire].[JobParameter];
    TRUNCATE TABLE [HangFire].[JobQueue];
    TRUNCATE TABLE [HangFire].[List];
    TRUNCATE TABLE [HangFire].[State];
    
    -- Delete from Job table and reset identity
    DELETE FROM [HangFire].[Job];
    DBCC CHECKIDENT ('[HangFire].[Job]', RESEED, 0);
    
    -- Update LastJobId in Hash table
    UPDATE [HangFire].[Hash]
    SET Value = 1
    WHERE Field = 'LastJobId';
END;