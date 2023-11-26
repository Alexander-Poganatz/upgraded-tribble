
CREATE OR ALTER PROCEDURE sel_Transaction(
	@uID INT
	,@eNumber SMALLINT
	,@tNumber INT
)
AS
BEGIN
	SELECT t.TransactionNumber, t.TransactionAmount, t.TransactionDate, t.Note
	FROM Envelope e
	INNER JOIN EnvelopeTransaction t ON e.EnvelopeID = t.EnvelopeID
	WHERE UserID = @uID AND EnvelopeNumber = @eNumber AND t.TransactionNumber = @tNumber;
END
