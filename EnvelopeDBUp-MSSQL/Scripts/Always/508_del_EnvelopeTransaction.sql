
CREATE OR ALTER PROCEDURE del_EnvelopeTransaction(
	@uID INT
	,@eNumber SMALLINT
	,@tNumber INT
)
AS
BEGIN
	DELETE FROM EnvelopeTransaction WHERE EnvelopeID = (SELECT EnvelopeID FROM Envelope WHERE UserID = @uID AND EnvelopeNumber = @eNumber) AND TransactionNumber = @tNumber;
END
