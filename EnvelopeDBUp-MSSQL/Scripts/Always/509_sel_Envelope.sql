
CREATE OR ALTER PROCEDURE sel_Envelope(
	@uID INT
	,@eNumber SMALLINT
)
AS
BEGIN
	SELECT EnvelopeName
	FROM Envelope
	WHERE UserID = @uID AND EnvelopeNumber = @eNumber;
END
