CREATE PROCEDURE [dbo].[UpsertPriceHelper](@InstrumentId INT, @Price FLOAT, @PriceDate DATETIME, @Timestamp DATETIME)
WITH NATIVE_COMPILATION, SCHEMABINDING, EXECUTE AS OWNER 
AS BEGIN ATOMIC WITH (TRANSACTION ISOLATION LEVEL = SNAPSHOT, LANGUAGE = N'English')
	INSERT INTO dbo.Price(InstrumentId, Price, PriceDate, Timestamp) VALUES (@InstrumentId, @Price, @PriceDate, @Timestamp)
	RETURN 0
END