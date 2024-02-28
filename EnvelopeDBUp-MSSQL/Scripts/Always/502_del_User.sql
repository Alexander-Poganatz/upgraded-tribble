
CREATE OR ALTER PROCEDURE del_User(
	@uID INT
)
AS
BEGIN

	BEGIN TRANSACTION;
	BEGIN TRY
		DELETE FROM EnvelopeTransaction WHERE EnvelopeID IN (SELECT EnvelopeID FROM Envelope WHERE UserID = @uID);
		DELETE FROM Envelope WHERE UserID = @uID;
		DELETE FROM [User] WHERE UserID = @uID;
		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH;
		ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END