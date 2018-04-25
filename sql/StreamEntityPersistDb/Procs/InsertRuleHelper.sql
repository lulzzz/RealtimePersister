CREATE PROCEDURE [dbo].[InsertRuleHelper](@Id INT, @PortfolioId INT, @Expression NVARCHAR(MAX), @Timestamp DATETIME)
WITH NATIVE_COMPILATION, SCHEMABINDING, EXECUTE AS OWNER 
AS BEGIN ATOMIC WITH (TRANSACTION ISOLATION LEVEL = SNAPSHOT,LANGUAGE = N'English')
	INSERT INTO [dbo].[Rule](Id, PortfolioId, Expression, Timestamp) VALUES (@Id, @PortfolioId, @Expression, @Timestamp)
	RETURN 0
END