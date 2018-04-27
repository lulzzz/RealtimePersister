CREATE PROCEDURE [dbo].[InsertRule](@Id INT, @PortfolioId INT, @Expression NVARCHAR(MAX), @Timestamp DATETIME)
AS
BEGIN
	BEGIN TRY
		BEGIN TRAN
			EXEC InsertRuleHelper @Id, @PortfolioId, @Expression, @Timestamp;
		COMMIT TRAN
	END TRY
	BEGIN CATCH
		IF XACT_STATE() = -1
			ROLLBACK TRAN
		;THROW			
	END CATCH
    RETURN 0
END