CREATE PROCEDURE [dbo].[InsertPosition](@Id INT, @InstrumentId INT, @PortfolioId INT, @Volume INT, @Price FLOAT, @Timestamp DATETIME)
AS
BEGIN
	BEGIN TRY
		BEGIN TRAN
			EXEC InsertPositionHelper @Id, @InstrumentId, @PortfolioId, @Volume, @Price, @Timestamp;
		COMMIT TRAN
	END TRY
	BEGIN CATCH
		IF XACT_STATE() = -1
			ROLLBACK TRAN
		;THROW			
	END CATCH
    RETURN 0
END