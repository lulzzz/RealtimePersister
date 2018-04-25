CREATE PROCEDURE [dbo].[UpsertPrice](@InstrumentId INT, @Price FLOAT, @PriceDate DATETIME, @Timestamp DATETIME, @SequenceNumber bigint)
AS
BEGIN
	BEGIN TRY
		BEGIN TRAN
			DECLARE @result INT
			EXEC @result = UpdatePrice @InstrumentId, @Price, @PriceDate, @Timestamp, @SequenceNumber;
			IF (@result = 0) BEGIN
				EXEC @result = InsertPrice @InstrumentId, @Price, @PriceDate, @Timestamp, @SequenceNumber;
			END
		COMMIT TRAN
	END TRY
	BEGIN CATCH
		IF XACT_STATE() = -1
		BEGIN
			ROLLBACK TRAN
		END
		;THROW			
	END CATCH
    RETURN 0
END