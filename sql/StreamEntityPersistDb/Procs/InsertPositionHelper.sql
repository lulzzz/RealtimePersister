CREATE PROCEDURE [dbo].[InsertPositionHelper](@Id INT, @InstrumentId INT, @PortfolioId INT, @Volume INT, @Price FLOAT, @Timestamp DATETIME)
WITH NATIVE_COMPILATION, SCHEMABINDING, EXECUTE AS OWNER 
AS BEGIN ATOMIC WITH (TRANSACTION ISOLATION LEVEL = SNAPSHOT, LANGUAGE = N'English')
	INSERT INTO dbo.Position(Id, InstrumentId, PortfolioId, Volume, Price, Timestamp) VALUES (@Id, @InstrumentId, @PortfolioId, @Volume, @Price, @Timestamp)
	RETURN 0
END