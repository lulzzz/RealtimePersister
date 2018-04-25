CREATE PROCEDURE [dbo].[InsertPrice](@InstrumentId INT, @Price FLOAT, @PriceDate DATETIME, @Timestamp DATETIME, @SequenceNumber bigint)
WITH NATIVE_COMPILATION, SCHEMABINDING, EXECUTE AS OWNER 
AS BEGIN ATOMIC WITH (TRANSACTION ISOLATION LEVEL = SNAPSHOT, LANGUAGE = N'English')
	INSERT dbo.Price (InstrumentId, Price, PriceDate, Timestamp, SequenceNumber) VALUES (@InstrumentId, @Price, @PriceDate, @Timestamp, @SequenceNumber)
	RETURN @@rowcount
END