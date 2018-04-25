CREATE PROCEDURE [dbo].[InsertPortfolio](@Id INT, @Name NVARCHAR(50), @Balance FLOAT, @CheckRules BIT, @Timestamp DATETIME)
AS
BEGIN
	BEGIN TRY
		BEGIN TRAN
			EXEC InsertPortfolioHelper @Id, @Name, @Balance, @CheckRules, @Timestamp;
		COMMIT TRAN
	END TRY
	BEGIN CATCH
		IF XACT_STATE() = -1
			ROLLBACK TRAN
		;THROW			
	END CATCH
    RETURN 0
END