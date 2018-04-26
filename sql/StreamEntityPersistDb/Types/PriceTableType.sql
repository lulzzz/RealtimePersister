CREATE TYPE [dbo].[PriceTableType] AS TABLE
(
	InstrumentId INT, Price FLOAT, PriceDate DATETIME, Timestamp DATETIME, SequenceNumber bigint
)
