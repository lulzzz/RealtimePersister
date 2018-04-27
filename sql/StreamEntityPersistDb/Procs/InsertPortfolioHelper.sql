CREATE PROCEDURE [dbo].[InsertPortfolioHelper](@Id INT, @Name NVARCHAR(50), @Balance FLOAT, @CheckRules BIT, @Timestamp DATETIME)
WITH NATIVE_COMPILATION, SCHEMABINDING, EXECUTE AS OWNER 
AS BEGIN ATOMIC WITH (TRANSACTION ISOLATION LEVEL = SNAPSHOT,LANGUAGE = N'English')
	INSERT INTO dbo.Portfolio(Id, Name, Balance, CheckRules, Timestamp) VALUES (@Id, @Name, @Balance, @CheckRules, @Timestamp)
	RETURN 0
END