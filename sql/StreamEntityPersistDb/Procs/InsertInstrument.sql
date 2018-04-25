CREATE PROCEDURE [dbo].[InsertInstrument](@Id INT, @SubmarketId INT, @Name NVARCHAR(50), @Timestamp DATETIME)
AS
BEGIN
	BEGIN TRY
		BEGIN TRAN
			EXEC InsertInstrumentHelper @Id, @SubmarketId, @Name, @Timestamp;
		COMMIT TRAN
	END TRY
	BEGIN CATCH
		IF XACT_STATE() = -1
			ROLLBACK TRAN
		;THROW			
	END CATCH
    RETURN 0
END