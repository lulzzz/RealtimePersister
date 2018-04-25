CREATE PROCEDURE [dbo].[UpsertPrice](@InstrumentId INT, @Price FLOAT, @PriceDate DATETIME, @Timestamp DATETIME)
AS
BEGIN
	BEGIN TRY
		BEGIN TRAN
			EXEC UpsertPriceHelper @InstrumentId, @Price, @PriceDate, @Timestamp;
		COMMIT TRAN
	END TRY
	BEGIN CATCH
		IF XACT_STATE() = -1
			ROLLBACK TRAN
		;THROW			
	END CATCH
    RETURN 0
END