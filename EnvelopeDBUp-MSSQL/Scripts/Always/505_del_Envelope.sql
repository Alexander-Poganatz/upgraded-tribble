
CREATE OR ALTER PROCEDURE del_Envelope(
	@uID INT
	,@eNumber SMALLINT
)
AS
BEGIN

	BEGIN TRANSACTION;
	BEGIN TRY
		DELETE FROM EnvelopeTransaction WHERE EnvelopeID IN (SELECT EnvelopeID FROM Envelope WHERE UserID = @uID AND EnvelopeNumber = @eNumber);
		DELETE FROM Envelope WHERE UserID = @uID AND EnvelopeNumber = @eNumber;
		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH;
		ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END
