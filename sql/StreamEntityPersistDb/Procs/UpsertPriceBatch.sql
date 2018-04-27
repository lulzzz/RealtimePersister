CREATE PROCEDURE [dbo].[UpsertPriceBatch](@PriceTable [PriceTableType] READONLY, @NumRows int)
AS BEGIN
	DECLARE @Id INT, @InstrumentId INT, @Price FLOAT, @PriceDate DATETIME, @Timestamp DATETIME, @SequenceNumber BIGINT

	WHILE @NumRows > 0
	BEGIN
		SELECT TOP 1 @Id = Id, @InstrumentId = InstrumentId, @Price = Price, @PriceDate = PriceDate, @Timestamp = Timestamp, @SequenceNumber = SequenceNumber
			FROM @PriceTable WHERE Id = @NumRows
		EXEC UpsertPrice @InstrumentId, @Price, @PriceDate, @Timestamp, @SequenceNumber
		SELECT @NumRows = @NumRows-1
	END
END