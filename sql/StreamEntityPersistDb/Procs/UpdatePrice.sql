CREATE PROCEDURE [dbo].[UpdatePrice](@InstrumentId INT, @Price FLOAT, @PriceDate DATETIME, @Timestamp DATETIME, @SequenceNumber bigint)
WITH NATIVE_COMPILATION, SCHEMABINDING, EXECUTE AS OWNER 
AS BEGIN ATOMIC WITH (TRANSACTION ISOLATION LEVEL = SNAPSHOT, LANGUAGE = N'English')
	UPDATE dbo.Price SET Price = @Price, PriceDate = @PriceDate, @Timestamp = Timestamp, SequenceNumber = @SequenceNumber WHERE InstrumentId = @InstrumentId
	RETURN @@rowcount
END