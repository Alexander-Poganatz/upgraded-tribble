
CREATE OR ALTER PROCEDURE transfer(
	@uID INT
	,@eSourceNumber SMALLINT
	,@eDestinationNumber SMALLINT
	,@amount INT
)
AS
BEGIN

	IF (SELECT COUNT(*) FROM Envelope WHERE UserID = @uID AND EnvelopeNumber IN (@eSourceNumber, @eDestinationNumber)) <> 2
		SELECT 0;
	ELSE
		BEGIN TRY
			BEGIN TRANSACTION;
			DECLARE @absAmount INT = ABS(@amount);
			DECLARE @negAmount INT = @absAmount * -1;
			DECLARE @utcNow DATE = GETUTCDATE()
			DECLARE @transferTo NVARCHAR(50) = CONCAT('Transfer to ', (SELECT EnvelopeName FROM Envelope WHERE UserID = @uID AND EnvelopeNumber = @eDestinationNumber))
			DECLARE @transferFrom NVARCHAR(50) = CONCAT('Transfer from ', (SELECT EnvelopeName FROM Envelope WHERE UserID = @uID AND EnvelopeNumber = @eSourceNumber))
			EXEC ins_EnvelopeTransaction @uID, @eSourceNumber, @negAmount, @utcNow, @transferTo;
			EXEC ins_EnvelopeTransaction @uID, @eDestinationNumber, @absAmount, @utcNow, @transferFrom;
			COMMIT TRANSACTION;
			SELECT 1;
		END TRY
		BEGIN CATCH;
			ROLLBACK TRANSACTION;
			THROW;
		END CATCH;
	
END
