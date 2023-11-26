
CREATE OR ALTER PROCEDURE ins_Envelope(
	@uID INT
	,@eName NVARCHAR(50)
)
AS
BEGIN
	DECLARE @newNum SMALLINT
	SELECT @newNum = ISNULL(MAX(EnvelopeNumber),0) + 1 FROM Envelope WHERE UserID = @uID;
	INSERT INTO Envelope(UserID, EnvelopeNumber, EnvelopeName) VALUES
	(@uID, @newNum, @eName);
	
	SELECT @newNum;
END
