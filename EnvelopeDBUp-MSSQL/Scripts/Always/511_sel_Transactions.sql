
CREATE OR ALTER PROCEDURE sel_Transactions(
	@uID INT
	,@eNumber SMALLINT
	,@limitNum INT
	,@offsetNum INT
)
AS
BEGIN
	SELECT t.TransactionNumber, t.TransactionAmount, t.TransactionDate, t.Note
	FROM Envelope e
	INNER JOIN EnvelopeTransaction t ON e.EnvelopeID = t.EnvelopeID
	WHERE UserID = @uID AND EnvelopeNumber = @eNumber
	ORDER BY 1 DESC
	OFFSET @offsetNum ROWS FETCH NEXT @limitNum ROWS ONLY;
	
	SELECT COUNT(*) AS NumberOfTransactions,
	(SELECT oe.EnvelopeName FROM Envelope oe WHERE oe.UserID = @uID AND oe.EnvelopeNumber = @eNumber) AS EnvelopeName
	FROM Envelope e
	INNER JOIN EnvelopeTransaction t ON e.EnvelopeID = t.EnvelopeID
	WHERE UserID = @uID AND EnvelopeNumber = @eNumber;
	
END
