CREATE PROCEDURE [dbo].[UpsertPrice](@InstrumentId INT, @Price FLOAT, @PriceDate DATETIME, @Timestamp DATETIME, @SequenceNumber bigint)
WITH NATIVE_COMPILATION, SCHEMABINDING, EXECUTE AS OWNER 
AS BEGIN ATOMIC WITH (TRANSACTION ISOLATION LEVEL = SNAPSHOT, LANGUAGE = N'English')
	DECLARE @result INT
	EXEC @result = [dbo].[UpdatePrice] @InstrumentId, @Price, @PriceDate, @Timestamp, @SequenceNumber;
	IF (@result = 0) BEGIN
		EXEC @result = [dbo].[InsertPrice] @InstrumentId, @Price, @PriceDate, @Timestamp, @SequenceNumber;
	END
END