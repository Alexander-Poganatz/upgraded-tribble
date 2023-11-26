
CREATE OR ALTER PROCEDURE upd_EnvelopeTransaction(
	@uID INT
	,@eNumber SMALLINT
	,@tNumber INT
	,@amount INT
	,@tDate DATE
	,@tNote NVARCHAR(50)
)
AS
BEGIN
	UPDATE EnvelopeTransaction SET TransactionAmount = @amount, TransactionDate = @tDate, Note = @tNote
	WHERE EnvelopeID = (SELECT EnvelopeID FROM Envelope WHERE UserID = @uID AND EnvelopeNumber = @eNumber) AND TransactionNumber = @tNumber;
END
