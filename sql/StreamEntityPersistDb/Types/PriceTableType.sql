CREATE TYPE [dbo].[PriceTableType] AS TABLE
(
	Id INT, InstrumentId INT, Price FLOAT, PriceDate DATETIME, Timestamp DATETIME, SequenceNumber bigint
)
