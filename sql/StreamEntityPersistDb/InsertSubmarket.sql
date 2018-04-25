CREATE PROCEDURE [dbo].[InsertSubmarket](@Id INT, @MarketId INT, @Name NVARCHAR(50), @Timestamp DATETIME)
AS
BEGIN
	BEGIN TRY
		BEGIN TRAN
			EXEC InsertSubmarketHelper @Id, @MarketId, @Name, @Timestamp;
		COMMIT TRAN
	END TRY
	BEGIN CATCH
		IF XACT_STATE() = -1
			ROLLBACK TRAN
		;THROW			
	END CATCH
    RETURN 0
END