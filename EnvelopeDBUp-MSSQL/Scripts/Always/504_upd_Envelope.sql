
CREATE OR ALTER PROCEDURE upd_Envelope(
	@uID INT
	,@eNumber SMALLINT
	,@eName NVARCHAR(50)
)
AS
BEGIN
	UPDATE Envelope SET EnvelopeName = @eName WHERE UserID = @uID AND EnvelopeNumber = @eNumber;
END
